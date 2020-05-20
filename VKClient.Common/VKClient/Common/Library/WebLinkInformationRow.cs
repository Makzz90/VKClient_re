using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class WebLinkInformationRow : InformationRow
  {
    private string _navigateUri;

    public WebLinkInformationRow(string navigateUri)
    {
      this._navigateUri = navigateUri;
      this.Title = this._navigateUri;
      this.CanNavigate = true;
      this.IsHighlighted = true;
    }

    public override void Navigate()
    {
      Navigator.Current.NavigateToWebUri(this._navigateUri, true, false);
    }
  }
}
