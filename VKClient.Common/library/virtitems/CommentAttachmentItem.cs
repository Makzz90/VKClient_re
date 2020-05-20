using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.VirtItems
{
  public class CommentAttachmentItem : VirtualizableItemBase
  {
    private readonly Comment _comment;

    public override double FixedHeight
    {
      get
      {
        return 25.0;
      }
    }

    public CommentAttachmentItem(Comment comment, Thickness margin)
        : base(300.0, margin, new Thickness())
    {
      this._comment = comment;
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      HyperlinkButton hyperlinkButton1 = new HyperlinkButton();
      Thickness thickness = new Thickness(-12.0, 0.0, 0.0, 0.0);
      ((FrameworkElement) hyperlinkButton1).Margin = thickness;
      string comment = CommonResources.Comment;
      ((ContentControl) hyperlinkButton1).Content = comment;
      HyperlinkButton hyperlinkButton2 = hyperlinkButton1;
      // ISSUE: method pointer
      ((ButtonBase) hyperlinkButton2).Click+=(new RoutedEventHandler( this.hypLink_Click));
      this.Children.Add((FrameworkElement) hyperlinkButton2);
    }

    private void hypLink_Click(object sender, RoutedEventArgs e)
    {
      Navigator.Current.NavigateToWallPostComments(this._comment.post_id, this._comment.owner_id, true, 0, 0, "");
    }
  }
}
