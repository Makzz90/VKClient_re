using System;

namespace XamlAnimatedGif
{
  public class DownloadProgressChangedArgs
  {
    public Uri Uri { get; private set; }

    public double Percentage { get; private set; }

    public DownloadProgressChangedArgs(Uri uri, double percentage)
    {
      this.Uri = uri;
      this.Percentage = percentage;
    }
  }
}
