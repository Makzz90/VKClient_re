using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using VKClient.Common.Framework;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

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
            DataPackageView dataPackageView = this.ShareOperation.Data;
            if (dataPackageView.Contains(StandardDataFormats.StorageItems))
            {
                IReadOnlyList<IStorageItem> readOnlyList = await dataPackageView.GetStorageItemsAsync();
                if (readOnlyList == null || readOnlyList.Count == 0)
                {
                    return;
                }
                IEnumerator<StorageFile> enumerator = Enumerable.OfType<StorageFile>(readOnlyList).GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        StorageFile storageFile = enumerator.Current;
                        if (VKConstants.SupportedImageExtensions.Contains(storageFile.FileType.ToUpperInvariant()))
                        {
                            try
                            {
                                Stream var_3_1AB = (await storageFile.OpenReadAsync()).AsStreamForRead();
                                this.Photos.Add(var_3_1AB);
                                try
                                {
                                    Stream var_5_233 = (await storageFile.GetThumbnailAsync(0)).AsStreamForRead();
                                    this.PhotosPreviews.Add(var_5_233);
                                }
                                catch
                                {
                                    this.PhotosPreviews.Add(null);
                                }
                                goto IL_2AC;
                            }
                            catch
                            {
                                goto IL_2AC;
                            }
                            //goto IL_262;
                        }
                        goto IL_262;
                    IL_2AC:
                        storageFile = null;
                        continue;
                    IL_262:
                        if (VKConstants.SupportedVideoExtensions.Contains(storageFile.FileType.ToLowerInvariant()))
                        {
                            this.Videos.Add(storageFile);
                            goto IL_2AC;
                        }
                        this.Documents.Add(storageFile);
                        goto IL_2AC;
                    }
                }
                finally
                {
                    //int num;
                    if ( enumerator != null)
                    {
                        enumerator.Dispose();
                    }
                }
                enumerator = null;
            }
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string var_7_364 = await dataPackageView.GetTextAsync();
                if (!string.IsNullOrWhiteSpace(var_7_364))
                {
                    this.Message = var_7_364;
                }
            }
            else if (dataPackageView.Contains(StandardDataFormats.WebLink))
            {
                try
                {
                    object arg_409_0 = await dataPackageView.GetWebLinkAsync();
                    string var_9_415 = (arg_409_0 != null) ? arg_409_0.ToString() : null;
                    if (!string.IsNullOrWhiteSpace(var_9_415))
                    {
                        this.Message = var_9_415;
                    }
                }
                catch (UriFormatException)
                {
                }
            }
            dataPackageView = null;
        }
        catch
        {
        }
    }


    public void StoreDataToRepository()
    {
      ParametersRepository.SetParameterForId("ShareOperation", this.ShareOperation);
      if (this.Photos != null && this.Photos.Count > 0)
      {
        ParametersRepository.SetParameterForId("ChoosenPhotos", new List<Stream>((IEnumerable<Stream>) this.Photos));
        if (this.PhotosPreviews != null && this.PhotosPreviews.Count == this.Photos.Count)
          ParametersRepository.SetParameterForId("ChoosenPhotosPreviews", new List<Stream>((IEnumerable<Stream>) this.PhotosPreviews));
      }
      if (this.Videos != null && this.Videos.Count > 0)
        ParametersRepository.SetParameterForId("ChosenVideos", new List<StorageFile>((IEnumerable<StorageFile>) this.Videos));
      if (this.Documents != null && this.Documents.Count > 0)
        ParametersRepository.SetParameterForId("ChosenDocuments", new List<StorageFile>((IEnumerable<StorageFile>) this.Documents));
      ParametersRepository.SetParameterForId("NewMessageContents", (this.Message ?? (this.Message = "")));
    }
  }
}
