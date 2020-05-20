using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.VirtItems
{
  public class WallPostAttachmentItem : VirtualizableItemBase
  {
    private WallPost _wallPost;

    public override double FixedHeight
    {
      get
      {
        return 25.0;
      }
    }

    public WallPostAttachmentItem(WallPost wallPost, Thickness margin)
      : base(300.0, margin, new Thickness())
    {
      this._wallPost = wallPost;
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      HyperlinkButton hyperlinkButton = new HyperlinkButton();
      hyperlinkButton.Margin = new Thickness(-12.0, 0.0, 0.0, 0.0);
      hyperlinkButton.Content = (object) CommonResources.Conversation_WallPost;
      hyperlinkButton.Click += new RoutedEventHandler(this.hypLink_Click);
      this.Children.Add((FrameworkElement) hyperlinkButton);
    }

    private void hypLink_Click(object sender, RoutedEventArgs e)
    {
      long toId = this._wallPost.to_id;
      long id = this._wallPost.id;
      long pollId = 0;
      long pollOwnerId = 0;
      if (this._wallPost.attachments != null)
      {
        Attachment attachment = this._wallPost.attachments.FirstOrDefault<Attachment>((Func<Attachment, bool>) (a => a.poll != null));
        if (attachment != null)
        {
          pollId = attachment.poll.poll_id;
          pollOwnerId = this._wallPost.copy_history == null || this._wallPost.copy_history.Count <= 0 ? this._wallPost.to_id : this._wallPost.copy_history[0].owner_id;
        }
      }
      Navigator.Current.NavigateToWallPostComments(id, toId, false, pollId, pollOwnerId, "");
    }
  }
}
