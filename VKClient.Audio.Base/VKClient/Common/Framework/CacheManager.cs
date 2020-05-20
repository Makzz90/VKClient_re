using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VKClient.Common.Utils;
using Windows.Foundation;
using Windows.Storage;

namespace VKClient.Common.Framework
{
    public static class CacheManager
    {
        private static string _cacheFolderName = "CachedDataV4";
        private static string _stateFolderName = "CachedData";

        public static void EnsureCacheFolderExists()
        {
            using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storeForApplication.DirectoryExists(CacheManager._cacheFolderName))
                    storeForApplication.CreateDirectory(CacheManager._cacheFolderName);
                if (storeForApplication.DirectoryExists(CacheManager._stateFolderName))
                    return;
                storeForApplication.CreateDirectory(CacheManager._stateFolderName);
            }
        }

        public static void EraseAll()
        {
            IsolatedStorageFile.GetUserStoreForApplication().Remove();
        }

        public static string GetFilePath(string fileId, CacheManager.DataType dataType = CacheManager.DataType.CachedData, string pathSeparator = "/")
        {
            return CacheManager.GetFolderNameForDataType(dataType) + pathSeparator + fileId;
        }

        public static string GetFullFilePath(string fileId, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, CacheManager.GetFilePath(fileId, dataType, "\\"));
        }

        public static string TrySerializeToString(IBinarySerializable obj)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BinaryWriter writer = new BinaryWriter((Stream)memoryStream);
                    obj.Write(writer);
                    memoryStream.Position = 0L;
                    return CacheManager.AsciiToString(new BinaryReader((Stream)memoryStream).ReadBytes((int)memoryStream.Length));
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("TrySerializeToString.TryDeserialize failed.", ex);
            }
            return "";
        }

        public static void TryDeserializeFromString(IBinarySerializable obj, string serStr)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(CacheManager.StringToAscii(serStr)))
                {
                    BinaryReader reader = new BinaryReader((Stream)memoryStream);
                    obj.Read(reader);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("TrySerializeToString.TryDeserialize failed.", ex);
            }
        }

        public static byte[] StringToAscii(string s)
        {
            byte[] numArray = new byte[s.Length];
            for (int index = 0; index < s.Length; ++index)
            {
                char ch = s[index];
                numArray[index] = (int)ch > (int)sbyte.MaxValue ? (byte)63 : (byte)ch;
            }
            return numArray;
        }

        public static string AsciiToString(byte[] bytes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte num in bytes)
                stringBuilder = stringBuilder.Append((char)num);
            return stringBuilder.ToString();
        }

        public static bool TryDeserialize(IBinarySerializable obj, string fileId, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string filePath = CacheManager.GetFilePath(fileId, dataType, "/");
                    if (!storeForApplication.FileExists(filePath))
                        return false;
                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(filePath, FileMode.Open, FileAccess.Read))
                    {
                        BinaryReader reader = new BinaryReader((Stream)storageFileStream);
                        obj.Read(reader);
                    }
                }
                stopwatch.Stop();
                Logger.Instance.Info("CacheManager.TryDeserialize succeeded for fileId = {0}, in {1} ms.", fileId, stopwatch.ElapsedMilliseconds);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("CacheManager.TryDeserialize failed.", ex);
            }
            return false;
        }

        public static async Task<bool> TryDeserializeAsync(IBinarySerializable obj, string fileId, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
        {
            bool result;
            try
            {
                StorageFolder rootDirectory = await ApplicationData.Current.LocalFolder.GetFolderAsync(CacheManager.GetFolderNameForDataType(dataType));
                IsolatedStorageFile var_5 = IsolatedStorageFile.GetUserStoreForApplication();
                try
                {
                    if (!var_5.FileExists(CacheManager.GetFilePath(fileId, dataType, "/")))
                    {
                        result = false;
                        return result;
                    }
                }
                finally
                {
                    // int num;
                    if (/*num < 0 &&*/ var_5 != null)
                    {
                        var_5.Dispose();
                    }
                }
                Stream arg_143_0 = await rootDirectory.OpenStreamForReadAsync(fileId);
                BinaryReader reader = new BinaryReader(arg_143_0);
                obj.Read(reader);
                arg_143_0.Close();
                result = true;
            }
            catch (Exception var_8_15F)
            {
                Logger.Instance.Error("CacheManager.TryDeserializeAsync failed.", var_8_15F);
                result = false;
            }
            return result;
        }

        public static async Task<bool> TryDeleteAsync(string fileId)
        {
            bool result;
            try
            {
                await (await (await ApplicationData.Current.LocalFolder.GetFolderAsync(CacheManager.GetFolderNameForDataType(CacheManager.DataType.CachedData))).GetFileAsync(fileId)).DeleteAsync();
            }
            catch (Exception var_5_16D)
            {
                Logger.Instance.Error("CacheManager.TryDeleteAsync failed. File Id = " + fileId, var_5_16D);
                result = false;
                return result;
            }
            result = true;
            return result;
        }


        public static string GetFolderNameForDataType(CacheManager.DataType dataType)
        {
            if (dataType == CacheManager.DataType.CachedData)
                return CacheManager._cacheFolderName;
            if (dataType == CacheManager.DataType.StateData)
                return CacheManager._stateFolderName;
            throw new Exception("Unknown data type");
        }

        public static async Task<bool> TrySerializeAsync(IBinarySerializable obj, string fileId, bool trim = false, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
        {
            bool result;
            try
            {
                Stream arg_FC_0 = await (await ApplicationData.Current.LocalFolder.GetFolderAsync(CacheManager.GetFolderNameForDataType(dataType))).OpenStreamForWriteAsync(fileId, CreationCollisionOption.ReplaceExisting);
                BinaryWriter writer = new BinaryWriter(arg_FC_0);
                if (trim && obj is IBinarySerializableWithTrimSupport)
                {
                    (obj as IBinarySerializableWithTrimSupport).WriteTrimmed(writer);
                }
                else
                {
                    obj.Write(writer);
                }
                arg_FC_0.Close();
                result = true;
            }
            catch (Exception var_5_140)
            {
                Logger.Instance.Error("CacheManager.TrySerializeAsync failed.", var_5_140);
                result = false;
            }
            return result;
        }


        public static bool TrySaveRawCachedData(byte[] bytes, string fileId, FileMode fileMode)
        {
            try
            {
                new Stopwatch().Start();
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(CacheManager.GetFilePath(fileId, CacheManager.DataType.CachedData, "/"), fileMode))
                        storageFileStream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("CacheManager.TrySaveRawCachedData failed.", ex);
            }
            return false;
        }

        public static bool TrySerialize(IBinarySerializable obj, string fileId, bool trim = false, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(CacheManager.GetFilePath(fileId, dataType, "/"), FileMode.Create))
                    {
                        BinaryWriter writer = new BinaryWriter((Stream)storageFileStream);
                        if (trim && obj is IBinarySerializableWithTrimSupport)
                            (obj as IBinarySerializableWithTrimSupport).WriteTrimmed(writer);
                        else
                            obj.Write(writer);
                    }
                }
                stopwatch.Stop();
                Logger.Instance.Info("CacheManager.TrySerialize succeeded for fileId = {0}, in {1} ms.", fileId, stopwatch.ElapsedMilliseconds);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("CacheManager.TrySerialize failed.", ex);
            }
            return false;
        }

        public static Stream GetStreamForWrite(string fileId)
        {
            using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                return (Stream)storeForApplication.OpenFile(CacheManager.GetFilePath(fileId, CacheManager.DataType.CachedData, "/"), FileMode.Create);
        }

        public static async Task<StorageFile> GetFileAsync(string fileId)
        {
            StorageFile result;
            try
            {
                result = await StorageFile.GetFileFromPathAsync(CacheManager.GetFullFilePath(fileId, CacheManager.DataType.CachedData));
                return result;
            }
            catch (Exception var_3_7B)
            {
                Logger.Instance.Error("CacheManager.GetFileAsync failed.", var_3_7B);
            }
            result = null;
            return result;
        }


        public static bool TryDelete(string fileId, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
        {
            try
            {
                new Stopwatch().Start();
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string filePath = CacheManager.GetFilePath(fileId, dataType, "/");
                    if (!storeForApplication.FileExists(filePath))
                        return false;
                    storeForApplication.DeleteFile(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("CacheManager.TryDelete failed.", ex);
            }
            return false;
        }

        public enum DataType
        {
            CachedData,
            StateData,
        }
    }
}
