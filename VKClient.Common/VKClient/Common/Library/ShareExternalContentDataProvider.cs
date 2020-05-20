using System;
//using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VKClient.Common.Framework;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Storage;
using Windows.Storage.FileProperties;
//using Windows.Storage.Streams;

namespace VKClient.Common.Library
{
    public class ShareExternalContentDataProvider : IShareContentDataProvider
    {
        private const string SHARE_OPERATION_KEY = "ShareOperation";
        private const string PHOTOS_KEY = "ChoosenPhotos";
        private const string PHOTOS_PREVIEWS_KEY = "ChoosenPhotosPreviews";
        private const string VIDEOS_KEY = "ChosenVideos";
        private const string DOCUMENTS_KEY = "ChosenDocuments";
        private const string MESSAGE_KEY = "NewMessageContents";

        public ShareOperation ShareOperation { get; set; }

        public List<Stream> Photos { get; set; }

        public List<Stream> PhotosPreviews { get; set; }

        public List<StorageFile> Documents { get; set; }

        public List<StorageFile> Videos { get; set; }

        public string Message { get; set; }

        public ShareExternalContentDataProvider(ShareOperation shareOperation)
        {
            this.ShareOperation = shareOperation;
            this.Photos = new List<Stream>();
            this.PhotosPreviews = new List<Stream>();
            this.Videos = new List<StorageFile>();
            this.Documents = new List<StorageFile>();
            this.Message = "";
            this.Init();
        }

        private async void Init()
        {
            try
            {
                DataPackageView data = this.ShareOperation.Data;
                if (data.Contains(StandardDataFormats.StorageItems))
                {
                    IReadOnlyList<IStorageItem> storageItemsAsync = await this.ShareOperation.Data.GetStorageItemsAsync();
                    if (storageItemsAsync == null || ((IReadOnlyCollection<IStorageItem>)storageItemsAsync).Count == 0)
                        return;
                    IEnumerator<StorageFile> enumerator = storageItemsAsync.OfType<StorageFile>().GetEnumerator();
                    try
                    {
                    //int num;
                    //if (num != 1 && num != 2)
                    //  goto label_17;
                    label_6:
                        StorageFile file = enumerator.Current;
                        try
                        {
                            this.Photos.Add((await file.OpenReadAsync()).AsStreamForRead());
                            try
                            {
                                this.PhotosPreviews.Add((await file.GetThumbnailAsync((ThumbnailMode)0)).AsStreamForRead());
                            }
                            catch
                            {
                                this.PhotosPreviews.Add(null);
                            }
                        }
                        catch
                        {
                        }
                    label_16:
                        file = null;
                        //label_17:
                        if (enumerator.MoveNext())
                        {
                            file = enumerator.Current;
                            if (!VKConstants.SupportedImageExtensions.Contains(file.FileType.ToUpperInvariant()))
                            {
                                if (VKConstants.SupportedVideoExtensions.Contains(file.FileType.ToLowerInvariant()))
                                {
                                    this.Videos.Add(file);
                                    goto label_16;
                                }
                                else
                                {
                                    this.Documents.Add(file);
                                    goto label_16;
                                }
                            }
                            else
                                goto label_6;
                        }
                    }
                    finally
                    {
                        if (enumerator != null)
                            enumerator.Dispose();
                    }
                    enumerator = null;
                }
                if (data.Contains(StandardDataFormats.Text))
                {
                    string textAsync = await this.ShareOperation.Data.GetTextAsync();
                    if (!string.IsNullOrWhiteSpace(textAsync))
                        this.ComposeMessage(textAsync);
                }
                else if (data.Contains(StandardDataFormats.WebLink))
                {
                    try
                    {
                        Uri webLinkAsync = await this.ShareOperation.Data.GetWebLinkAsync();
                        if (webLinkAsync != null)
                            this.ComposeMessage(webLinkAsync.ToString());
                    }
                    catch
                    {
                    }
                }
                data = null;
            }
            catch
            {
            }
        }

        private void ComposeMessage(string message)
        {
            DataPackagePropertySetView properties = this.ShareOperation.Data.Properties;
            string title = properties.Title;
            string description = properties.Description;
            StringBuilder stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(title))
                stringBuilder.Append(title).Append("\n");
            if (!string.IsNullOrEmpty(description))
                stringBuilder.Append(description).Append("\n");
            stringBuilder.Append(message);
            this.Message = stringBuilder.ToString();
        }

        public void StoreDataToRepository()
        {
            ParametersRepository.SetParameterForId("ShareOperation", (object)this.ShareOperation);
            if (this.Photos != null && this.Photos.Count > 0)
            {
                ParametersRepository.SetParameterForId("ChoosenPhotos", (object)new List<Stream>((IEnumerable<Stream>)this.Photos));
                if (this.PhotosPreviews != null && this.PhotosPreviews.Count == this.Photos.Count)
                    ParametersRepository.SetParameterForId("ChoosenPhotosPreviews", (object)new List<Stream>((IEnumerable<Stream>)this.PhotosPreviews));
            }
            if (this.Videos != null && this.Videos.Count > 0)
                ParametersRepository.SetParameterForId("ChosenVideos", (object)new List<StorageFile>((IEnumerable<StorageFile>)this.Videos));
            if (this.Documents != null && this.Documents.Count > 0)
                ParametersRepository.SetParameterForId("ChosenDocuments", (object)new List<StorageFile>((IEnumerable<StorageFile>)this.Documents));
            ParametersRepository.SetParameterForId("NewMessageContents", (object)(this.Message ?? (this.Message = "")));
        }
    }
}
