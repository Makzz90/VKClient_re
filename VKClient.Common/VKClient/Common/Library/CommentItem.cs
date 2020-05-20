using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.BLExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class CommentItem : VirtualizableItemBase, IHandle<CommentEdited>, IHandle
  {
    private static ThemeHelper _themeHelper = new ThemeHelper();
    private Thickness _mainTextBlockMargin = new Thickness(74.0, 40.0, 0.0, 0.0);
    private Comment _comment;
    private User _user;
    private User _user2;
    private Group _group;
    private double _height;
    private Border _imageLike;
    private TextBlock _textBlockLike;
    private TextItem _textTimeItem;
    private long _owner_id;
    //private long _ownerId;
    private List<MenuItem> _contextMenu;
    private Action<CommentItem> _deleteCommentCallback;
    private Action<CommentItem> _replyCommentCallback;
    private Action<CommentItem> _editCommentCallback;
    private Action<CommentItem> _tapCommentCallback;
    private MenuItem _deleteCommentMenuItem;
    private MenuItem _replyMenuItem;
    private MenuItem _copyMenuItem;
    private MenuItem _likeMenuItem;
    private MenuItem _unlikeMenuItem;
    private MenuItem _editMenuItem;
    private MenuItem _likesMenuItem;
    private MenuItem _reportMenuItem;
    private Action<CommentItem> _seeAllLikesCallback;
    private string _extraText;
    private string _thumbSrc;
    private bool _preview;
    private LikeObjectType _likeObjectType;
    private bool _isNotificationComment;
    private string _highlightedText;
    private const bool _friendsOnly = false;
    private const bool _isCommentAttachmends = true;
    private double _topMarginDate;
    private TextItem _textBlockName;

    public Comment Comment
    {
      get
      {
        return this._comment;
      }
    }

    public long OwnerId
    {
      get
      {
        return this._owner_id;
      }
    }

    public string Name
    {
      get
      {
        string str = "";
        if (this._user != null)
          str = this._user.Name;
        if (this._group != null)
          str = this._group.name ?? "";
        return str;
      }
    }

    public string NameWithoutLastName
    {
      get
      {
        string str = "";
        if (this._user != null)
          str = this._user.first_name;
        if (this._group != null)
          str = this._group.name ?? "";
        return str;
      }
    }

    public string ImageSrc
    {
      get
      {
        string str = "";
        if (this._user != null)
          str = this._user.photo_max;
        if (this._group != null)
          str = this._group.photo_200;
        return str;
      }
    }

    public string DateText
    {
      get
      {
        string str = UIStringFormatterHelper.FormatDateTimeForUI(VKClient.Common.Utils.Extensions.UnixTimeStampToDateTime((double) this._comment.date, true));
        if (this._comment.reply_to_uid > 0L && this._user2 != null)
          str = str + " " + string.Format(CommonResources.ToSmbdFrm, (object) this._user2.first_name_dat);
        else if (this._comment.reply_to_uid < 0L)
          str = str + " " + CommonResources.ToCommunity;
        if (!string.IsNullOrEmpty(this._extraText))
          str = str + " " + this._extraText;
        return str;
      }
    }

    public string LikesCountStr
    {
      get
      {
        int count = this._comment.likes.count;
        if (count != 0)
          return count.ToString();
        return "";
      }
    }

    public int LikesCount
    {
      get
      {
        if (this._comment.likes != null)
          return this._comment.likes.count;
        return 0;
      }
    }

    private bool CanReport
    {
      get
      {
        if (this._comment.from_id == AppGlobalStateManager.Current.LoggedInUserId)
          return false;
        if (this._likeObjectType != LikeObjectType.comment && this._likeObjectType != LikeObjectType.photo_comment)
          return this._likeObjectType == LikeObjectType.video_comment;
        return true;
      }
    }

    public override double FixedHeight
    {
      get
      {
        if (!this._isNotificationComment)
          return this._height;
        return this._height + 16.0;
      }
    }

    public CommentItem(double width, Thickness margin, LikeObjectType likeObjectType, Action<CommentItem> deleteCommentCallback, Action<CommentItem> replyCommentCallback, Action<CommentItem> editCommentCallback, long owner_id, Comment comment, User user, User user2, Group group, Action<CommentItem> tapCommentCallback = null, string extraText = "", string thumbSrc = "", Action<CommentItem> seeAllLikesCallback = null, bool preview = false, bool isNotificationComment = false, string hightlightedText = "")
      : base(width, margin, new Thickness())
    {
      this._preview = preview;
      this._extraText = extraText;
      this._thumbSrc = thumbSrc;
      this._deleteCommentCallback = deleteCommentCallback;
      this._replyCommentCallback = replyCommentCallback;
      this._editCommentCallback = editCommentCallback;
      this._tapCommentCallback = tapCommentCallback;
      this._seeAllLikesCallback = seeAllLikesCallback;
      this._likeObjectType = likeObjectType;
      this._comment = comment;
      this._user = user;
      this._user2 = user2;
      this._group = group;
      if (this._group == null && this._comment.from_id < 0L)
        this._group = GroupsService.Current.GetCachedGroup(-this._comment.from_id);
      this._highlightedText = hightlightedText;
      Border border = new Border();
      double num1 = 18.0;
      border.Width = num1;
      double num2 = 18.0;
      border.Height = num2;
      this._imageLike = border;
      this._owner_id = owner_id;
      this._isNotificationComment = isNotificationComment;
      int num3 = (int) (Visibility) Application.Current.Resources["PhoneDarkThemeVisibility"];
      BitmapImage bitmapImage = new BitmapImage(new Uri("/VKClient.Common;component/Resources/like-white.png", UriKind.Relative));
      this._imageLike.OpacityMask = (Brush) new ImageBrush()
      {
        ImageSource = (ImageSource) bitmapImage
      };
      this._textBlockLike = new TextBlock();
      this.CreateVirtualizableChildren();
      this.UpdateLikeImageAndTextBlock();
      this.HookupTapEvent();
      EventAggregator.Current.Subscribe((object) this);
    }

    private void HookupTapEvent()
    {
      this._view.Tap += (EventHandler<System.Windows.Input.GestureEventArgs>) ((s, e) =>
      {
        if (this._tapCommentCallback == null)
          return;
        e.Handled = true;
        this._tapCommentCallback(this);
      });
    }

    private void UpdateContextMenu()
    {
      this._contextMenu = new List<MenuItem>();
      if (this._replyCommentCallback != null)
      {
        this._replyMenuItem = new MenuItem();
        this._replyMenuItem.Header = (object) CommonResources.CommentItem_Reply;
        this._replyMenuItem.Click += new RoutedEventHandler(this.replyMenuItem_Tap);
        this._contextMenu.Add(this._replyMenuItem);
      }
      if (this._comment.likes.count > 0)
      {
        this._likesMenuItem = new MenuItem();
        this._likesMenuItem.Header = (object) CommonResources.Comment_Likes;
        this._likesMenuItem.Click += new RoutedEventHandler(this._likesMenuItem_Tap);
        this._contextMenu.Add(this._likesMenuItem);
      }
      if (this._comment.likes.can_like == 1 && this._comment.likes.user_likes != 1)
      {
        this._likeMenuItem = new MenuItem();
        this._likeMenuItem.Header = (object) CommonResources.PostCommentsPage_AppBar_Like;
        this._likeMenuItem.Click += new RoutedEventHandler(this.likeMenuItem_Tap);
        this._contextMenu.Add(this._likeMenuItem);
      }
      if (this._comment.likes.user_likes == 1)
      {
        this._unlikeMenuItem = new MenuItem();
        this._unlikeMenuItem.Header = (object) CommonResources.PostCommentsPage_AppBar_Unlike;
        this._unlikeMenuItem.Click += new RoutedEventHandler(this.unlikeMenuItem_Tap);
        this._contextMenu.Add(this._unlikeMenuItem);
      }
      if (this._comment.CanEdit() && this._editCommentCallback != null)
      {
        this._editMenuItem = new MenuItem();
        this._editMenuItem.Header = (object) CommonResources.CommentItem_EditComment;
        this._editMenuItem.Click += new RoutedEventHandler(this._editMenuItem_Tap);
        this._contextMenu.Add(this._editMenuItem);
      }
      this._copyMenuItem = new MenuItem();
      this._copyMenuItem.Header = (object) CommonResources.CommentItem_Copy;
      this._copyMenuItem.Click += new RoutedEventHandler(this.copyMenuItem_Tap);
      this._contextMenu.Add(this._copyMenuItem);
      if (this.CanReport)
      {
        this._reportMenuItem = new MenuItem();
        this._reportMenuItem.Header = (object) CommonResources.Report;
        this._reportMenuItem.Click += new RoutedEventHandler(this._reportMenuItem_Click);
        this._contextMenu.Add(this._reportMenuItem);
      }
      if (this._comment.CanDelete(this._owner_id) && this._deleteCommentCallback != null)
      {
        this._deleteCommentMenuItem = new MenuItem();
        this._deleteCommentMenuItem.Header = (object) CommonResources.CommentItem_DeleteComment;
        this._deleteCommentMenuItem.Click += new RoutedEventHandler(this.deleteCommentMenuItem_Tap);
        this._contextMenu.Add(this._deleteCommentMenuItem);
      }
      this._textBlockName.SetMenu(this._contextMenu.ToList<MenuItem>());
    }

    private void _reportMenuItem_Click(object sender, RoutedEventArgs e)
    {
      ReportContentHelper.ReportComment(this._comment.from_id, this._comment.id, this._likeObjectType);
    }

    private void _likesMenuItem_Tap(object sender, RoutedEventArgs e)
    {
      Navigator.Current.NavigateToLikesPage(this.OwnerId, this.Comment.cid, (int) this._likeObjectType, this.LikesCount);
    }

    private void copyMenuItem_Tap(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(this._comment.text);
    }

    private void replyMenuItem_Tap(object sender, RoutedEventArgs e)
    {
      this._replyCommentCallback(this);
    }

    private void deleteCommentMenuItem_Tap(object sender, RoutedEventArgs e)
    {
      if (MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, CommonResources.DeleteComment, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      this._deleteCommentCallback(this);
    }

    private void unlikeMenuItem_Tap(object sender, RoutedEventArgs e)
    {
      this.Like(false);
    }

    private void likeMenuItem_Tap(object sender, RoutedEventArgs e)
    {
      this.Like(true);
    }

    private void _editMenuItem_Tap(object sender, RoutedEventArgs e)
    {
      this._editCommentCallback(this);
    }

    private void UpdateLikeImageAndTextBlock()
    {
      double num = this.MeasureTextWidth();
      this._textBlockLike.Text = this.LikesCountStr;
      this._textBlockLike.Margin = new Thickness(this.Width - num, this._topMarginDate + 3.0, 0.0, 0.0);
      Border border = this._imageLike;
      Thickness margin = this._textBlockLike.Margin;
      double left = margin.Left - 18.0 - 6.0;
      margin = this._textBlockLike.Margin;
      double top = margin.Top + 6.0;
      double right = 0.0;
      double bottom = 0.0;
      Thickness thickness = new Thickness(left, top, right, bottom);
      border.Margin = thickness;
      if (this._comment.likes.count == 0)
        this._imageLike.Opacity = 0.0;
      else if (this._comment.likes.user_likes == 1)
      {
        this._imageLike.Background = (Brush) (Application.Current.Resources["PhoneActiveIconBrush"] as SolidColorBrush);
        this._textBlockLike.Foreground = (Brush) (Application.Current.Resources["PhoneNewsActionLikedForegroundBrush"] as SolidColorBrush);
        this._imageLike.Opacity = this._textBlockLike.Opacity = 1.0;
      }
      else
      {
        this._imageLike.Background = (Brush) (Application.Current.Resources["PhoneGreyIconBrush"] as SolidColorBrush);
        this._textBlockLike.Foreground = (Brush) (Application.Current.Resources["PhoneNewsActionForegroundBrush"] as SolidColorBrush);
        this._imageLike.Opacity = this._textBlockLike.Opacity = 1.0;
      }
    }

    private double MeasureTextWidth()
    {
      return new TextBlock()
      {
        FontFamily = new FontFamily("Segoe WP"),
        FontSize = 20.0,
        Text = this.LikesCountStr
      }.ActualWidth;
    }

    private void CreateVirtualizableChildren()
    {
      double top = this._isNotificationComment ? 0.0 : 16.0;
      this._textBlockName = new TextItem(this.Width, new Thickness(74.0, top - 6.0, 0.0, 0.0), this.Name, false, 25.333, "Segoe WP", 0.0, Application.Current.Resources["PhoneNameBlueBrush"] as SolidColorBrush, true, new Action(this.NavigateToSource));
      this.VirtualizableChildren.Add((IVirtualizable) this._textBlockName);
      double width = this.Width - 74.0;
      Thickness thickness;
      if (!string.IsNullOrEmpty(this._comment.text))
      {
        NewsTextItem newsTextItem = new NewsTextItem(width, new Thickness(74.0, 28.0 + top, 0.0, 0.0), this._comment.text ?? "", this._preview, null, 0.0, (FontFamily) null, 28.0, (Brush) null, false, 0.0, HorizontalAlignment.Left, "", TextAlignment.Left, false);
        this.VirtualizableChildren.Add((IVirtualizable) newsTextItem);
        thickness = newsTextItem.Margin;
        this._topMarginDate = thickness.Top + newsTextItem.FixedHeight + 4.0;
      }
      else
        this._topMarginDate = 28.0 + top;
      if (!this._comment.Attachments.IsNullOrEmpty())
      {
        string itemId = this._comment.from_id == 0L || this._comment.id <= 0L ? "" : string.Format("{0}_{1}", (object) this._comment.from_id, (object) this._comment.id);
        AttachmentsItem attachmentsItem = new AttachmentsItem(width, new Thickness(73.0, this._topMarginDate, 0.0, 0.0), this._comment.Attachments, (Geo) null, itemId, false, true, false, false, 0.0, false, false, "");
        this.VirtualizableChildren.Add((IVirtualizable) attachmentsItem);
        this._topMarginDate = this._topMarginDate + (attachmentsItem.FixedHeight + 12.0);
      }
      this._textTimeItem = new TextItem(width - 70.0, new Thickness(73.0, this._topMarginDate, 0.0, 0.0), this.DateText, true, 20.0, "Segoe WP", VKConstants.LineHeight, Application.Current.Resources["PhoneVKSubtleBrush"] as SolidColorBrush, true, null);
      this.VirtualizableChildren.Add((IVirtualizable) this._textTimeItem);
      thickness = this._textTimeItem.TextMargin;
      this._height = thickness.Top + this._textTimeItem.FixedHeight;
      if (!string.IsNullOrEmpty(this._highlightedText))
      {
        TextItem textItem = new TextItem(this.Width - 72.0, new Thickness(72.0, this._height, 0.0, 0.0), this._highlightedText, false, 20.0, "Segoe WP", 23.0, Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush, true, null);
        this.VirtualizableChildren.Add((IVirtualizable) textItem);
        this._height = this._height + textItem.FixedHeight;
      }
      if (!string.IsNullOrEmpty(this._thumbSrc))
      {
        this.VirtualizableChildren.Add((IVirtualizable) new VirtualizableImage(80.0, 80.0, new Thickness(77.0, this._height + 6.0, 0.0, 0.0), this._thumbSrc, new Action<VirtualizableImage>(this.OnThumbTap), "", true, true, Stretch.UniformToFill, (Brush) null, -1.0, false, false));
        this._height = this._height + 86.0;
      }
      this.VirtualizableChildren.Add((IVirtualizable) new VirtualizableImage(62.0, 62.0, new Thickness(0.0, top, 0.0, 0.0), this.ImageSrc, new Action<VirtualizableImage>(this.AvaTap), "", true, true, Stretch.UniformToFill, (Brush) null, -1.0, false, true));
      if (this._isNotificationComment || this._preview)
        return;
      this.VirtualizableChildren.Add((IVirtualizable) new UCItem(40.0, new Thickness(this.Width - 32.0, 5.0, 0.0, 0.0), (Func<UserControlVirtualizable>) (() => (UserControlVirtualizable) new MoreActionsUC()
      {
        TapCallback = new Action(this._onMoreOptionsTap)
      }), (Func<double>) (() => 40.0), (Action<UserControlVirtualizable>) null, 0.0, false));
    }

    private void AvaTap(VirtualizableImage obj)
    {
      this.NavigateToSource();
    }

    private void NavigateToSource()
    {
      if (this._comment.from_id > 0L)
        Navigator.Current.NavigateToUserProfile(this._comment.from_id, this.Name, "", false);
      else
        Navigator.Current.NavigateToGroup(-this._comment.from_id, this.Name, false);
    }

    private void OnThumbTap(VirtualizableImage obj)
    {
      if (this._tapCommentCallback == null)
        return;
      this._tapCommentCallback(this);
    }

    public void Like(bool like)
    {
      LikesService.Current.AddRemoveLike(like, this._owner_id, this._comment.cid, this._likeObjectType, (Action<BackendResult<ResponseWithId, ResultCode>>) (res => {}), "");
      if (like)
      {
        ++this._comment.likes.count;
        this._comment.likes.user_likes = 1;
      }
      else
      {
        --this._comment.likes.count;
        if (this._comment.likes.count < 0)
          this._comment.likes.count = 0;
        this._comment.likes.user_likes = 0;
        this._comment.likes.can_like = 1;
      }
      this.UpdateLikeImageAndTextBlock();
      this.UpdateContextMenu();
    }

    protected override void GenerateChildren()
    {
      Rectangle rectangle1 = new Rectangle();
      double num1 = 480.0;
      rectangle1.Width = num1;
      double num2 = 2.0;
      rectangle1.Height = num2;
      SolidColorBrush solidColorBrush1 = Application.Current.Resources["PhoneTableSeparatorBrush"] as SolidColorBrush;
      rectangle1.Fill = (Brush) solidColorBrush1;
      Thickness thickness1 = new Thickness(-16.0, this._isNotificationComment ? this.FixedHeight : -1.0, 0.0, 0.0);
      rectangle1.Margin = thickness1;
      Rectangle rectangle2 = rectangle1;
      if (!this._isNotificationComment)
      {
        Rectangle rect = new Rectangle();
        double num3 = 480.0;
        rect.Width = num3;
        double num4 = this.FixedHeight + 16.0;
        rect.Height = num4;
        SolidColorBrush solidColorBrush2 = Application.Current.Resources["PhoneNewsBackgroundBrush"] as SolidColorBrush;
        rect.Fill = (Brush) solidColorBrush2;
        Thickness thickness2 = new Thickness(-16.0, 0.0, 0.0, 0.0);
        rect.Margin = thickness2;
        foreach (FrameworkElement coverByRectangle in RectangleHelper.CoverByRectangles(rect))
          this.Children.Add(coverByRectangle);
        this.Children.Add((FrameworkElement) rectangle2);
      }
      this.Children.Add((FrameworkElement) this._imageLike);
      this.Children.Add((FrameworkElement) this._textBlockLike);
      this.UpdateContextMenu();
    }

    private void _onMoreOptionsTap()
    {
      ContextMenu contextMenu = ContextMenuService.GetContextMenu((DependencyObject) this._textBlockName.View);
      if (contextMenu == null)
        return;
      contextMenu.IsOpen = true;
    }

    private void buttonWrapper_Click(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.AvaTap((VirtualizableImage) null);
    }

    protected override void ReleaseResourcesOnUnload()
    {
      base.ReleaseResourcesOnUnload();
      this._contextMenu = (List<MenuItem>) null;
      this._textBlockName.ResetMenu();
    }

    public void Handle(CommentEdited message)
    {
      if (message.Comment.from_id != this._comment.from_id || message.Comment.cid != this._comment.cid || this.Parent == null)
        return;
      this.Parent.Substitute((IVirtualizable) this, (IVirtualizable) new CommentItem(this.Width, this.Margin, this._likeObjectType, this._deleteCommentCallback, this._replyCommentCallback, this._editCommentCallback, this._owner_id, message.Comment, this._user, this._user2, this._group, this._tapCommentCallback, this._extraText, this._thumbSrc, this._seeAllLikesCallback, this._preview, false, ""));
    }
  }
}
