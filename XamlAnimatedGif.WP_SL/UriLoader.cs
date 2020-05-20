using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace XamlAnimatedGif
{
    public class UriLoader
    {
        private const string ROOT_FOLDER_NAME = "GifCache";

        public event EventHandler<DownloadProgressChangedArgs> DownloadProgressChanged;

        private static async Task<Stream> GetStreamFromUriCoreAsync(Uri uri)
        {
            string scheme = uri.Scheme;
            Stream result;
            if (!(scheme == "ms-appx") && !(scheme == "ms-appdata"))
            {
                if (!(scheme == "file"))
                {
                    throw new NotSupportedException("Only ms-appx:, ms-appdata:, http:, https: and file: URIs are supported");
                }
                result = await (await StorageFile.GetFileFromPathAsync(uri.LocalPath)).OpenStreamForReadAsync();
            }
            else
            {
                result = await (await StorageFile.GetFileFromApplicationUriAsync(uri)).OpenStreamForReadAsync();
            }
            return result;
        }

        private static async Task<Stream> OpenTempFileStreamAsync(string fileName)
        {
            IStorageFile windowsRuntimeFile;
            Stream result;
            try
            {
                windowsRuntimeFile = await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("GifCache", CreationCollisionOption.OpenIfExists)).GetFileAsync(fileName);
            }
            catch (FileNotFoundException)
            {
                result = null;
                return result;
            }
            result = await windowsRuntimeFile.OpenStreamForReadAsync();
            return result;
        }

        private static async Task<Stream> CreateTempFileStreamAsync(string fileName)
        {
            return await (await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("GifCache", CreationCollisionOption.OpenIfExists)).CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting)).OpenStreamForWriteAsync();
        }

        private static async Task DeleteTempFileAsync(string fileName)
        {
            try
            {
                await (await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("GifCache", CreationCollisionOption.OpenIfExists)).GetFileAsync(fileName)).DeleteAsync();
            }
            catch (FileNotFoundException)
            {
            }
        }

        private static async Task ClearCacheFolderAsync()
        {
            try
            {
                IReadOnlyList<StorageFile> readOnlyList = await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("GifCache", CreationCollisionOption.OpenIfExists)).GetFilesAsync();
                IEnumerator<StorageFile> enumerator = readOnlyList.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        await enumerator.Current.DeleteAsync();
                    }
                }
                finally
                {
                    if (enumerator != null)
                    {
                        enumerator.Dispose();
                    }
                }
                enumerator = null;
            }
            catch (FileNotFoundException)
            {
            }
        }

        private static string GetCacheFileName(System.Uri uri)
        {
            return uri.GetHashCode().ToString();
        }

        public Task<Stream> GetStreamFromUriAsync(System.Uri uri, CancellationToken cancellationToken)
        {
            if (uri.Scheme == "http" || uri.Scheme == "https")
                return this.GetNetworkStreamAsync(uri, cancellationToken);
            return UriLoader.GetStreamFromUriCoreAsync(uri);
        }

        public async Task ClearCache()
        {
            await UriLoader.ClearCacheFolderAsync();
        }

        private async Task<Stream> GetNetworkStreamAsync(System.Uri uri, CancellationToken cancellationToken)
        {
            string cacheFileName = UriLoader.GetCacheFileName(uri);
            if (await UriLoader.OpenTempFileStreamAsync(cacheFileName) == null)
                await this.DownloadToCacheFileAsync(uri, cacheFileName, cancellationToken);
            return await UriLoader.OpenTempFileStreamAsync(cacheFileName);
        }

        private async Task DownloadToCacheFileAsync(Uri uri, string fileName, CancellationToken cancellationToken)
        {
            Exception obj = null;
            int num = 0;
            try
            {
                HttpClient httpClient = new HttpClient();
                try
                {
                    IBuffer source = await httpClient.GetBufferAsync(uri).AsTask(cancellationToken, new Progress<HttpProgress>(delegate(HttpProgress progress)
                    {
                        ulong? totalBytesToReceive = progress.TotalBytesToReceive;
                        if (totalBytesToReceive.HasValue)
                        {
                            double percentage = Math.Round(progress.BytesReceived * 100.0 / totalBytesToReceive.Value, 2);
                            EventHandler<DownloadProgressChangedArgs> expr_3E = this.DownloadProgressChanged;
                            if (expr_3E == null)
                            {
                                return;
                            }
                            expr_3E.Invoke(this, new DownloadProgressChangedArgs(uri, percentage));
                        }
                    }));
                    Stream stream = source.AsStream();
                    try
                    {
                        Stream stream2 = await UriLoader.CreateTempFileStreamAsync(fileName);
                        try
                        {
                            await stream.CopyToAsync(stream2);
                        }
                        finally
                        {
                            if (stream2 != null)
                            {
                                stream2.Dispose();
                            }
                        }
                        stream2 = null;
                    }
                    finally
                    {
                        if (stream != null)
                        {
                            stream.Dispose();
                        }
                    }
                    stream = null;
                }
                finally
                {
                    if (httpClient != null)
                    {
                        httpClient.Dispose();
                    }
                }
                httpClient = null;
            }
            catch (Exception obj_0)
            {
                num = 1;
                obj = obj_0;
            }
            
            if (num == 1)
            {
                await UriLoader.DeleteTempFileAsync(fileName);
                Exception expr_2FC = obj as Exception;
                if (expr_2FC == null)
                {
                    throw obj;
                }
                ExceptionDispatchInfo.Capture(expr_2FC).Throw();
            }
            obj = null;
        }


        private static Stream GenerateStreamFromString(string s)
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter((Stream)memoryStream);
            string str = s;
            streamWriter.Write(str);
            streamWriter.Flush();
            long num = 0;
            memoryStream.Position = num;
            return (Stream)memoryStream;
        }
    }
}
