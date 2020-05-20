namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class EmptyDataMediaListItemViewModel : MediaListItemViewModelBase
  {
    public string ImageUri { get; private set; }

    public override string Id
    {
      get
      {
        return "";
      }
    }

    public EmptyDataMediaListItemViewModel(string imageUri)
      : base(ProfileMediaListItemType.EmptyData)
    {
      this.ImageUri = imageUri;
    }
  }
}
