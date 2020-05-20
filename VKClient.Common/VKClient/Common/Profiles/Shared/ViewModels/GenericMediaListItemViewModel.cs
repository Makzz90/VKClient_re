namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class GenericMediaListItemViewModel : MediaListItemViewModelBase
  {
    private readonly string _id;

    public string GenericTitle { get; private set; }

    public string GenericSubtitle { get; private set; }

    public string IconUri { get; private set; }

    public override string Id
    {
      get
      {
        return this._id;
      }
    }

    public GenericMediaListItemViewModel(string id, string genericTitle, string genericSubtitle, string iconUri)
      : base(ProfileMediaListItemType.Generic)
    {
      this._id = id;
      this.GenericTitle = genericTitle;
      this.GenericSubtitle = genericSubtitle;
      this.IconUri = iconUri;
    }
  }
}
