using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class NewsActivityCommentUC : UserControlVirtualizable
  {
    private double _height = 96.0;
    private const int TEXT_MAX_WIDTH = 364;
    private const int TEXT_COMMENT_LINE_HEIGHT = 24;
    private string _userName;
    private Uri _userPhotoUri;
    private bool _addSeparator;
    internal Canvas canvasBackground;
    internal Canvas canvas;
    internal Image image;
    internal TextBlock textBlockName;
    internal RichTextBox textBoxComment;
    internal Border borderChevron;
    internal Rectangle rectSeparator;
    private bool _contentLoaded;

    public NewsActivityCommentUC()
    {
      this.InitializeComponent();
      this.textBlockName.Text = ("");
      ((UIElement) this.rectSeparator).Visibility = Visibility.Collapsed;
    }

    public static double CalculateHeight(NewsActivityComment activityComment)
    {
        return TextBlockMeasurementHelper.MeasureHeight(364.0, UIStringFormatterHelper.SubstituteMentionsWithNames((activityComment != null ? activityComment.text : null) ?? ""), new FontFamily("Segoe WP"), 20.0, 24.0, (LineStackingStrategy)1, (TextWrapping)2, new Thickness()) > 24.0 ? 96.0 : 72.0;
    }

    public void Initialize(NewsActivityComment activityComment, IEnumerable<User> users, IEnumerable<Group> groups, bool addSeparator)
    {
      if (activityComment.from_id > 0L)
      {
          User user = users != null ? Enumerable.FirstOrDefault<User>(users, (Func<User, bool>)(u => u.id == activityComment.from_id)) : null;
        if (user != null)
        {
          this._userName = user.Name;
          string photoMax = user.photo_max;
          if (!string.IsNullOrEmpty(photoMax))
            this._userPhotoUri = new Uri(photoMax);
        }
      }
      else
      {
          Group group = groups != null ? Enumerable.FirstOrDefault<Group>(groups, (Func<Group, bool>)(u => u.id == -activityComment.from_id)) : null;
        if (group != null)
        {
          this._userName = group.name;
          string photo200 = group.photo_200;
          if (!string.IsNullOrEmpty(photo200))
            this._userPhotoUri = new Uri(photo200);
        }
      }
      BrowserNavigationService.SetText((DependencyObject) this.textBoxComment, UIStringFormatterHelper.SubstituteMentionsWithNames(activityComment.text ?? ""));
      BrowserNavigationService.SetDisableHyperlinks((DependencyObject) this.textBoxComment, true);
      ((UIElement) this.textBoxComment).Measure(new Size(364.0, double.PositiveInfinity));
      Size desiredSize = ((UIElement) this.textBoxComment).DesiredSize;
      // ISSUE: explicit reference operation
      double height = ((Size) @desiredSize).Height;
      if (!double.IsNaN(height) && !double.IsInfinity(height) && height < 48.0)
        this._height = this._height - 24.0;
      this._addSeparator = addSeparator;
      this.textBlockName.Text = this._userName;
      this.textBlockName.CorrectText(364.0);
      Canvas.SetTop((UIElement) this.borderChevron, this._height / 2.0 - ((FrameworkElement) this.borderChevron).Height / 2.0);
      if (this._addSeparator)
      {
        Canvas.SetTop((UIElement) this.rectSeparator, this._height - ((FrameworkElement) this.rectSeparator).Height);
        ((UIElement) this.rectSeparator).Visibility = Visibility.Visible;
      }
      ((FrameworkElement) this.canvasBackground).Height = this._height;
      ((FrameworkElement) this.canvas).Height = this._height;
    }

    public override void LoadFullyNonVirtualizableItems()
    {
      VeryLowProfileImageLoader.SetUriSource(this.image, this._userPhotoUri);
    }

    public override void ReleaseResources()
    {
      VeryLowProfileImageLoader.SetUriSource(this.image,  null);
    }

    public override void ShownOnScreen()
    {
      if (!(this._userPhotoUri !=  null) || !this._userPhotoUri.IsAbsoluteUri)
        return;
      VeryLowProfileImageLoader.SetPriority(this._userPhotoUri.OriginalString, DateTime.Now.Ticks);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/NewsActivityCommentUC.xaml", UriKind.Relative));
      this.canvasBackground = (Canvas) base.FindName("canvasBackground");
      this.canvas = (Canvas) base.FindName("canvas");
      this.image = (Image) base.FindName("image");
      this.textBlockName = (TextBlock) base.FindName("textBlockName");
      this.textBoxComment = (RichTextBox) base.FindName("textBoxComment");
      this.borderChevron = (Border) base.FindName("borderChevron");
      this.rectSeparator = (Rectangle) base.FindName("rectSeparator");
    }
  }
}
