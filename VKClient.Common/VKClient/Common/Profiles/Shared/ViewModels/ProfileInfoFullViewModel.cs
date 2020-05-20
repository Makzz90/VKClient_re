using System.Collections.ObjectModel;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public abstract class ProfileInfoFullViewModel
  {
    public ObservableCollection<ProfileInfoSectionItem> InfoSections { get; private set; }

    public abstract string Name { get; }

    protected ProfileInfoFullViewModel()
    {
      this.InfoSections = new ObservableCollection<ProfileInfoSectionItem>();
    }
  }
}
