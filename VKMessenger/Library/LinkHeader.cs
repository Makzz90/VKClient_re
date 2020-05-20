using System.Windows;
using System.Windows.Media;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Utils;

namespace VKMessenger.Library
{
  public sealed class LinkHeader
  {
      public Link Link { get; private set; }

      public long MessageId { get; private set; }

      public string Title { get; private set; }

      public TextWrapping TitleWrapping { get; private set; }

      public string Domain { get; private set; }

      public string Description { get; private set; }

      public Visibility DescriptionVisibility { get; private set; }

      public string Thumbnail { get; private set; }

      public SolidColorBrush ThumbnailBackground { get; private set; }

      public string ThumbnailPlaceholderLetter { get; private set; }

      public string Url { get; private set; }

    public LinkHeader(Link link, long messageId = 0)
    {
      this.Link = link;
      this.MessageId = messageId;
      this.Title = Extensions.ForUI(link.title);
      this.Description = Extensions.ForUI(link.description);
      this.Domain = Extensions.ForUI(link.url);
      this.Url = link.url;
      if (this.Title.Length > 55)
      {
        this.DescriptionVisibility = Visibility.Collapsed;
        this.TitleWrapping = (TextWrapping) 2;
      }
      else
      {
        this.DescriptionVisibility = Visibility.Visible;
        this.TitleWrapping = (TextWrapping) 1;
        if (string.IsNullOrWhiteSpace(this.Description))
          this.Description = "...";
      }
      this.Thumbnail = link.image_src;
      if (this.Thumbnail != null)
      {
        this.ThumbnailBackground = (SolidColorBrush) Application.Current.Resources["PhoneChromeBrush"];
        this.ThumbnailPlaceholderLetter = "";
      }
      else
      {
        this.ThumbnailBackground = (SolidColorBrush) Application.Current.Resources["PhonePollSliderBackgroundBrush"];
        this.ThumbnailPlaceholderLetter = this.Domain[0].ToString().ToUpper();
      }
    }
  }
}
