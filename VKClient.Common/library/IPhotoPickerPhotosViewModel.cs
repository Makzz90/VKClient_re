using System;
using System.Collections.ObjectModel;
using VKClient.Photos.Library;

namespace VKClient.Common.Library
{
  public interface IPhotoPickerPhotosViewModel
  {
    ObservableCollection<AlbumPhoto> AlbumPhotos { get; }

    int MaxAllowedToSelect { get; }

    int SelectedCount { get; }

    int CountToLoad { get; set; }

    bool SuppressEXIFFetch { get; set; }

    void LoadData(bool refresh = true, Action callback = null);

    void CleanupSession();
  }
}
