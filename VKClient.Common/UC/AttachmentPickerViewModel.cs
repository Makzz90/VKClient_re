using System.Collections.Generic;
using System.Collections.ObjectModel;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class AttachmentPickerViewModel : ViewModelBase
  {
      public IPhotoPickerPhotosViewModel PhotosVM { get; private set; }

      public ObservableCollection<AttachmentPickerItemViewModel> AttachmentTypes { get; private set; }

      public int MaxCount { get; private set; }

    public AttachmentPickerViewModel(List<AttachmentPickerItemViewModel> attachmentTypes, int maxCount)
    {
      this.AttachmentTypes = new ObservableCollection<AttachmentPickerItemViewModel>(attachmentTypes);
      this.MaxCount = maxCount;
      this.PhotosVM = Navigator.Current.GetPhotoPickerPhotosViewModelInstance(maxCount);
      this.PhotosVM.CountToLoad = 5;
    }
  }
}
