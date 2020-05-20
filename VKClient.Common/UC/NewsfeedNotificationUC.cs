using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.BackendServices;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class NewsfeedNotificationUC : UserControlVirtualizable
  {
    private const double TEXT_MARGIN_LEFT_RIGHT_LARGE = 24.0;
    private UserNotification _userNotification;
    private UserNotificationNewsfeed _newsfeed;
    private double _width;
    private Action _hideCallback;
    private double _height;
    private List<UserNotificationImage> _images;
    private string _title;
    private string _message;
    private UserNotificationButton _button;
    private List<User> _users;
    private List<Group> _groups;
    private string _usersDescription;
    private string _groupsDescription;
    private bool _hasTitle;
    private bool _hasMessage;
    private bool _hasImages;
    private bool _hasButton;
    private bool _hasUsers;
    private bool _hasGroups;
    private bool _hasUsersDescription;
    private bool _hasGroupsDescription;
    private string _navigationUrl;
    private Uri _imageUri;
    private Image _image;
    private Canvas _imageContainer;
    private double _fixedHeight;
    private const double HIDE_BUTTON_WIDTH_HEIGHT = 56.0;
    private bool _isNavigating;
    internal StackPanel stackPanel;
    internal Canvas canvasDismiss;
    private bool _contentLoaded;

    public NewsfeedNotificationUC()
    {
      this.InitializeComponent();
    }

    public void Initialize(UserNotification userNotification, List<User> users, List<Group> groups, double width, Action hideCallback)
    {
      this._userNotification = userNotification;
      this._users = users;
      this._groups = groups;
      this._width = width;
      this._hideCallback = hideCallback;
      this._newsfeed = this._userNotification.newsfeed;
      this._images = this._newsfeed.images;
      this._title = this._newsfeed.title;
      this._message = this._newsfeed.message;
      this._button = this._newsfeed.button;
      this._usersDescription = this._newsfeed.users_description;
      this._groupsDescription = this._newsfeed.groups_description;
      this._hasTitle = !string.IsNullOrWhiteSpace(this._title);
      this._hasMessage = !string.IsNullOrWhiteSpace(this._message);
      List<UserNotificationImage> images = this._images;
      // ISSUE: explicit non-virtual call
      this._hasImages = images != null && images.Count > 0;
      UserNotificationButton button1 = this._button;
      this._hasButton = !string.IsNullOrWhiteSpace(button1 != null ? button1.title :  null) && this._button.action != null;
      List<User> users1 = this._users;
      // ISSUE: explicit non-virtual call
      this._hasUsers = users1 != null && users1.Count > 0;
      List<Group> groups1 = this._groups;
      // ISSUE: explicit non-virtual call
      this._hasGroups = groups1 != null && groups1.Count > 0;
      this._hasUsersDescription = !string.IsNullOrWhiteSpace(this._usersDescription);
      this._hasGroupsDescription = !string.IsNullOrWhiteSpace(this._groupsDescription);
      UserNotificationButton button2 = this._button;
      int num1;
      if (button2 == null)
      {
        num1 = 0;
      }
      else
      {
        UserNotificationButtonAction action = button2.action;
        UserNotificationButtonActionType? nullable = action != null ? new UserNotificationButtonActionType?(action.type) : new UserNotificationButtonActionType?();
        UserNotificationButtonActionType buttonActionType = UserNotificationButtonActionType.open_url;
        num1 = nullable.GetValueOrDefault() == buttonActionType ? (nullable.HasValue ? 1 : 0) : 0;
      }
      if (num1 != 0)
        this._navigationUrl = this._button.action.url;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this.stackPanel).Children).Clear();
      ((FrameworkElement) this.stackPanel).Width = width;
      this._height = 0.0;
      if (this._newsfeed.layout == UserNotificationNewsfeedLayout.banner)
        this.ComposeLargeUI();
      else
        this.ComposeSmallMediumUI();
      Rectangle rectangle1 = new Rectangle();
      double num2 = 16.0;
      ((FrameworkElement) rectangle1).Height = num2;
      SolidColorBrush solidColorBrush = (SolidColorBrush) Application.Current.Resources["PhoneNewsDividerBrush"];
      ((Shape) rectangle1).Fill = ((Brush) solidColorBrush);
      Rectangle rectangle2 = rectangle1;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this.stackPanel).Children).Add((UIElement) rectangle2);
      this._height = this._height + ((FrameworkElement) rectangle2).Height;
      this._fixedHeight = Math.Max(this._height, ((FrameworkElement) this.canvasDismiss).Height);
    }

    private void ComposeLargeUI()
    {
      if (this._hasImages)
      {
        Canvas image = this.GetImage();
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this.stackPanel).Children).Add((UIElement) image);
        this._height = this._height + ((FrameworkElement) image).Height;
      }
      Thickness margin;
      if (this._hasTitle)
      {
        TextBlock titleTextBlockLarge = this.GetTitleTextBlockLarge(this._hasImages ? 16.0 : 24.0);
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this.stackPanel).Children).Add((UIElement) titleTextBlockLarge);
        double height = this._height;
        margin = ((FrameworkElement) titleTextBlockLarge).Margin;
        // ISSUE: explicit reference operation
        double num1 = ((Thickness) @margin).Top + ((FrameworkElement) titleTextBlockLarge).ActualHeight;
        margin = ((FrameworkElement) titleTextBlockLarge).Margin;
        // ISSUE: explicit reference operation
        double bottom = ((Thickness) @margin).Bottom;
        double num2 = num1 + bottom;
        this._height = height + num2;
      }
      if (this._hasMessage)
      {
        TextBlock messageTextBlockLarge = this.GetMessageTextBlockLarge(!this._hasTitle ? (this._hasImages ? 16.0 : 24.0) : 10.0);
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this.stackPanel).Children).Add((UIElement) messageTextBlockLarge);
        double height = this._height;
        margin = ((FrameworkElement) messageTextBlockLarge).Margin;
        // ISSUE: explicit reference operation
        double num1 = ((Thickness) @margin).Top + ((FrameworkElement) messageTextBlockLarge).ActualHeight;
        margin = ((FrameworkElement) messageTextBlockLarge).Margin;
        // ISSUE: explicit reference operation
        double bottom = ((Thickness) @margin).Bottom;
        double num2 = num1 + bottom;
        this._height = height + num2;
      }
      if (this._hasUsers || this._hasGroups)
      {
        ItemsControl usersGroupsListLarge = this.GetUsersGroupsListLarge(24.0);
        if (usersGroupsListLarge != null)
        {
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this.stackPanel).Children).Add((UIElement) usersGroupsListLarge);
          double height = this._height;
          margin = ((FrameworkElement) usersGroupsListLarge).Margin;
          // ISSUE: explicit reference operation
          double num1 = ((Thickness) @margin).Top + ((FrameworkElement) usersGroupsListLarge).Height;
          margin = ((FrameworkElement) usersGroupsListLarge).Margin;
          // ISSUE: explicit reference operation
          double bottom = ((Thickness) @margin).Bottom;
          double num2 = num1 + bottom;
          this._height = height + num2;
        }
      }
      if (this._hasUsersDescription || this._hasGroupsDescription)
      {
        TextBlock descriptionTextBlockLarge = this.GetUsersGroupsDescriptionTextBlockLarge(this._hasUsers || this._hasGroups ? 8.0 : 24.0);
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this.stackPanel).Children).Add((UIElement) descriptionTextBlockLarge);
        double height = this._height;
        margin = ((FrameworkElement) descriptionTextBlockLarge).Margin;
        // ISSUE: explicit reference operation
        double num1 = ((Thickness) @margin).Top + ((FrameworkElement) descriptionTextBlockLarge).ActualHeight;
        margin = ((FrameworkElement) descriptionTextBlockLarge).Margin;
        // ISSUE: explicit reference operation
        double bottom = ((Thickness) @margin).Bottom;
        double num2 = num1 + bottom;
        this._height = height + num2;
      }
      if (this._hasButton)
      {
        FrameworkElement button = this.GetButton(this._width - 32.0, 24.0, 0.0);
        button.HorizontalAlignment = ((HorizontalAlignment) 1);
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this.stackPanel).Children).Add((UIElement) button);
        double height = this._height;
        margin = button.Margin;
        // ISSUE: explicit reference operation
        double num1 = ((Thickness) @margin).Top + button.Height;
        margin = button.Margin;
        // ISSUE: explicit reference operation
        double bottom = ((Thickness) @margin).Bottom;
        double num2 = num1 + bottom;
        this._height = height + num2;
      }
      if (this._hasButton && this._button.style == UserNotificationButtonStyle.cell)
        return;
      UIElementCollection children = ((Panel) this.stackPanel).Children;
      Rectangle rectangle = new Rectangle();
      double num = 24.0;
      ((FrameworkElement) rectangle).Height = num;
      ((PresentationFrameworkCollection<UIElement>) children).Add((UIElement) rectangle);
      this._height = this._height + 24.0;
    }

    private Canvas GetImage()
    {
      double num1;
      double num2;
      switch (this._newsfeed.layout)
      {
        case UserNotificationNewsfeedLayout.info:
          num2 = num1 = 44.0;
          break;
        case UserNotificationNewsfeedLayout.app:
          num2 = num1 = 80.0;
          break;
        default:
          UserNotificationImage image1 = this._images[0];
          double width1 = (double) image1.width;
          double height1 = (double) image1.height;
          if (width1 > 0.0 && height1 > 0.0)
          {
            double num3 = width1 / height1;
            num2 = this._width;
            num1 = num2 / num3;
            break;
          }
          num2 = num1 = this._width;
          break;
      }
      Image image2 = new Image();
      double num4 = num2;
      ((FrameworkElement) image2).Width = num4;
      double num5 = num1;
      ((FrameworkElement) image2).Height = num5;
      int num6 = 3;
      image2.Stretch=((Stretch) num6);
      int num7 = 1;
      ((FrameworkElement) image2).HorizontalAlignment = ((HorizontalAlignment) num7);
      int num8 = 1;
      ((FrameworkElement) image2).VerticalAlignment = ((VerticalAlignment) num8);
      this._image = image2;
      double num9 = (double) ScaleFactor.GetRealScaleFactor() / 100.0;
      int scaledImageWidth = (int) Math.Round(num2 * num9);
      int scaledImageHeight = (int) Math.Round(num1 * num9);
      UserNotificationImage m0 = Enumerable.FirstOrDefault<UserNotificationImage>(this._images, (Func<UserNotificationImage, bool>)(i =>
      {
        if (i.width >= scaledImageWidth)
          return i.height >= scaledImageHeight;
        return false;
      }));
      this._imageUri = ((m0 != null ? ((UserNotificationImage) m0).url :  null) ?? ((UserNotificationImage) Enumerable.Last<UserNotificationImage>(this._images)).url).ConvertToUri();
      Canvas canvas = new Canvas();
      double width2 = ((FrameworkElement) this._image).Width;
      ((FrameworkElement) canvas).Width = width2;
      double height2 = ((FrameworkElement) this._image).Height;
      ((FrameworkElement) canvas).Height = height2;
      this._imageContainer = canvas;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this._imageContainer).Children).Add((UIElement) this._image);
      return this._imageContainer;
    }

    private TextBlock GetTitleTextBlockLarge(double marginTop)
    {
      TextBlock textBlock = new TextBlock();
      double num1 = this._width - 48.0;
      ((FrameworkElement) textBlock).Width = num1;
      Thickness thickness = new Thickness(24.0, marginTop, 24.0, 0.0);
      ((FrameworkElement) textBlock).Margin = thickness;
      double num2 = 25.33;
      textBlock.FontSize = num2;
      int num3 = 1;
      textBlock.LineStackingStrategy=((LineStackingStrategy) num3);
      double num4 = 32.0;
      textBlock.LineHeight = num4;
      FontFamily fontFamily = new FontFamily("Segoe WP");
      textBlock.FontFamily = fontFamily;
      SolidColorBrush solidColorBrush = (SolidColorBrush) Application.Current.Resources["PhoneAlmostBlackBrush"];
      textBlock.Foreground = ((Brush) solidColorBrush);
      int num5 = 0;
      textBlock.TextAlignment=((TextAlignment) num5);
      int num6 = 2;
      textBlock.TextWrapping=((TextWrapping) num6);
      string title = this._title;
      textBlock.Text = title;
      return textBlock;
    }

    private TextBlock GetMessageTextBlockLarge(double marginTop)
    {
      TextBlock textBlock = new TextBlock();
      double num1 = this._width - 48.0;
      ((FrameworkElement) textBlock).Width = num1;
      Thickness thickness = new Thickness(24.0, marginTop, 24.0, 0.0);
      ((FrameworkElement) textBlock).Margin = thickness;
      double num2 = 20.0;
      textBlock.FontSize = num2;
      int num3 = 1;
      textBlock.LineStackingStrategy=((LineStackingStrategy) num3);
      double num4 = 24.0;
      textBlock.LineHeight = num4;
      FontFamily fontFamily = new FontFamily("Segoe WP");
      textBlock.FontFamily = fontFamily;
      SolidColorBrush solidColorBrush = (SolidColorBrush) Application.Current.Resources["PhoneDarkGrayBrush"];
      textBlock.Foreground = ((Brush) solidColorBrush);
      int num5 = 0;
      textBlock.TextAlignment=((TextAlignment) num5);
      int num6 = 2;
      textBlock.TextWrapping=((TextWrapping) num6);
      string message = this._message;
      textBlock.Text = message;
      return textBlock;
    }

    private ItemsControl GetUsersGroupsListLarge(double marginTop)
    {
      ItemsControl usersGroupsList = this.GetUsersGroupsList();
      if (usersGroupsList != null)
      {
        ((FrameworkElement) usersGroupsList).Margin=(new Thickness(0.0, marginTop, 0.0, 0.0));
        ((FrameworkElement) usersGroupsList).HorizontalAlignment = ((HorizontalAlignment) 1);
        ((FrameworkElement) usersGroupsList).VerticalAlignment = ((VerticalAlignment) 0);
      }
      return usersGroupsList;
    }

    private TextBlock GetUsersGroupsDescriptionTextBlockLarge(double marginTop)
    {
      string str1;
      if (this._hasUsersDescription)
      {
        str1 = this._usersDescription;
      }
      else
      {
        if (!this._hasGroupsDescription)
          return  null;
        str1 = this._groupsDescription;
      }
      TextBlock textBlock = new TextBlock();
      double num1 = this._width - 48.0;
      ((FrameworkElement) textBlock).Width = num1;
      Thickness thickness = new Thickness(24.0, marginTop, 24.0, 0.0);
      ((FrameworkElement) textBlock).Margin = thickness;
      double num2 = 18.0;
      textBlock.FontSize = num2;
      int num3 = 1;
      textBlock.LineStackingStrategy=((LineStackingStrategy) num3);
      double num4 = 22.0;
      textBlock.LineHeight = num4;
      FontFamily fontFamily = new FontFamily("Segoe WP");
      textBlock.FontFamily = fontFamily;
      SolidColorBrush solidColorBrush = (SolidColorBrush) Application.Current.Resources["PhoneDarkGrayBrush"];
      textBlock.Foreground = ((Brush) solidColorBrush);
      int num5 = 0;
      textBlock.TextAlignment=((TextAlignment) num5);
      int num6 = 2;
      textBlock.TextWrapping=((TextWrapping) num6);
      string str2 = str1;
      textBlock.Text = str2;
      return textBlock;
    }

    private void ComposeSmallMediumUI()
    {
      Canvas canvas1 = new Canvas();
      double width = this._width;
      ((FrameworkElement) canvas1).Width = width;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      ((Panel) canvas1).Background = ((Brush) solidColorBrush);
      int num1 = 0;
      ((FrameworkElement) canvas1).VerticalAlignment = ((VerticalAlignment) num1);
      Canvas canvas2 = canvas1;
      double val1 = 0.0;
      double num2 = 0.0;
      if (this._hasImages)
      {
        Canvas image = this.GetImage();
        Canvas.SetLeft((UIElement) image, 16.0);
        Canvas.SetTop((UIElement) image, 16.0);
        ((PresentationFrameworkCollection<UIElement>) ((Panel) canvas2).Children).Add((UIElement) image);
        val1 = 16.0 + ((FrameworkElement) image).Height;
        num2 = 16.0 + ((FrameworkElement) image).Width;
      }
      double val2 = 0.0;
      double num3 = 16.0;
      double num4 = 4.0;
      if (this._newsfeed.layout == UserNotificationNewsfeedLayout.info)
      {
        num3 = 12.0;
        num4 = 8.0;
      }
      double marginLeft = Math.Max(16.0, num2 + 16.0);
      double num5 = this._width - marginLeft - 56.0 - 8.0;
      if (this._hasTitle)
      {
        TextBlock blockMediumSmall = this.GetTitleTextBlockMediumSmall(num5);
        Canvas.SetTop((UIElement) blockMediumSmall, num3);
        Canvas.SetLeft((UIElement) blockMediumSmall, marginLeft);
        ((PresentationFrameworkCollection<UIElement>) ((Panel) canvas2).Children).Add((UIElement) blockMediumSmall);
        val2 += Canvas.GetTop((UIElement) blockMediumSmall) + ((FrameworkElement) blockMediumSmall).ActualHeight;
      }
      if (this._hasMessage)
      {
        TextBlock blockMediumSmall = this.GetMessageTextBlockMediumSmall(num5);
        double num6 = this._hasTitle ? num4 : num3;
        Canvas.SetTop((UIElement) blockMediumSmall, val2 + num6);
        Canvas.SetLeft((UIElement) blockMediumSmall, marginLeft);
        ((PresentationFrameworkCollection<UIElement>) ((Panel) canvas2).Children).Add((UIElement) blockMediumSmall);
        val2 += num6 + ((FrameworkElement) blockMediumSmall).ActualHeight;
      }
      this._height = Math.Max(val1, val2);
      ((FrameworkElement) canvas2).Height = this._height;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this.stackPanel).Children).Add((UIElement) canvas2);
      StackPanel stackPanel1 = new StackPanel();
      Thickness thickness = new Thickness(16.0, 8.0, 0.0, 0.0);
      ((FrameworkElement) stackPanel1).Margin = thickness;
      double num7 = 64.0;
      ((FrameworkElement) stackPanel1).Height = num7;
      int num8 = 0;
      ((FrameworkElement) stackPanel1).HorizontalAlignment = ((HorizontalAlignment) num8);
      int num9 = 1;
      stackPanel1.Orientation=((Orientation) num9);
      StackPanel stackPanel2 = stackPanel1;
      ItemsControl itemsControl =  null;
      if (this._hasUsers || this._hasGroups)
      {
        itemsControl = this.GetUsersGroupsListMediumSmall(12.0);
        if (itemsControl != null)
          ((PresentationFrameworkCollection<UIElement>) ((Panel) stackPanel2).Children).Add((UIElement) itemsControl);
      }
      if (this._hasUsersDescription || this._hasGroupsDescription)
      {
        TextBlock blockMediumSmall = this.GetUsersGroupsDescriptionTextBlockMediumSmall(itemsControl != null ? ((FrameworkElement) itemsControl).Width : 0.0);
        ((PresentationFrameworkCollection<UIElement>) ((Panel) stackPanel2).Children).Add((UIElement) blockMediumSmall);
      }
      bool flag = ((PresentationFrameworkCollection<UIElement>) ((Panel) stackPanel2).Children).Count > 0;
      Thickness margin;
      if (flag)
      {
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this.stackPanel).Children).Add((UIElement) stackPanel2);
        double height1 = this._height;
        double num6;
        if (((PresentationFrameworkCollection<UIElement>) ((Panel) stackPanel2).Children).Count <= 0)
        {
          num6 = 0.0;
        }
        else
        {
          double height2 = ((FrameworkElement) stackPanel2).Height;
          margin = ((FrameworkElement) stackPanel2).Margin;
          // ISSUE: explicit reference operation
          double top = ((Thickness) @margin).Top;
          num6 = height2 + top;
        }
        this._height = height1 + num6;
      }
      if (this._hasButton)
      {
        double marginTop = flag ? 8.0 : 20.0;
        FrameworkElement button = this.GetButton(num5, marginTop, marginLeft);
        button.HorizontalAlignment = ((HorizontalAlignment) 0);
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this.stackPanel).Children).Add((UIElement) button);
        double height = this._height;
        margin = button.Margin;
        // ISSUE: explicit reference operation
        double num6 = ((Thickness) @margin).Top + button.Height;
        margin = button.Margin;
        // ISSUE: explicit reference operation
        double bottom = ((Thickness) @margin).Bottom;
        double num10 = num6 + bottom;
        this._height = height + num10;
      }
      if (this._hasButton && this._button.style == UserNotificationButtonStyle.cell)
        return;
      UIElementCollection children = ((Panel) this.stackPanel).Children;
      Rectangle rectangle = new Rectangle();
      double num11 = 20.0;
      ((FrameworkElement) rectangle).Height = num11;
      ((PresentationFrameworkCollection<UIElement>) children).Add((UIElement) rectangle);
      this._height = this._height + 20.0;
    }

    private TextBlock GetTitleTextBlockMediumSmall(double width)
    {
      TextBlock textBlock = new TextBlock();
      double num1 = width;
      ((FrameworkElement) textBlock).Width = num1;
      double num2 = 22.67;
      textBlock.FontSize = num2;
      int num3 = 1;
      textBlock.LineStackingStrategy=((LineStackingStrategy) num3);
      double num4 = 26.0;
      textBlock.LineHeight = num4;
      FontFamily fontFamily = new FontFamily("Segoe WP");
      textBlock.FontFamily = fontFamily;
      SolidColorBrush solidColorBrush = (SolidColorBrush) Application.Current.Resources["PhoneAlmostBlackBrush"];
      textBlock.Foreground = ((Brush) solidColorBrush);
      int num5 = 2;
      textBlock.TextWrapping=((TextWrapping) num5);
      string title = this._title;
      textBlock.Text = title;
      return textBlock;
    }

    private TextBlock GetMessageTextBlockMediumSmall(double width)
    {
      TextBlock textBlock = new TextBlock();
      double num1 = width;
      ((FrameworkElement) textBlock).Width = num1;
      int num2 = 1;
      textBlock.LineStackingStrategy=((LineStackingStrategy) num2);
      double num3 = 20.0;
      textBlock.FontSize = num3;
      double num4 = 24.0;
      textBlock.LineHeight = num4;
      FontFamily fontFamily = new FontFamily("Segoe WP");
      textBlock.FontFamily = fontFamily;
      SolidColorBrush solidColorBrush = (SolidColorBrush) Application.Current.Resources["PhoneDarkGrayBrush"];
      textBlock.Foreground = ((Brush) solidColorBrush);
      int num5 = 2;
      textBlock.TextWrapping=((TextWrapping) num5);
      string message = this._message;
      textBlock.Text = message;
      return textBlock;
    }

    private ItemsControl GetUsersGroupsListMediumSmall(double marginTop)
    {
      ItemsControl usersGroupsList = this.GetUsersGroupsList();
      if (usersGroupsList != null)
      {
        ((FrameworkElement) usersGroupsList).Margin=(new Thickness(0.0, marginTop, 0.0, 0.0));
        ((FrameworkElement) usersGroupsList).VerticalAlignment = ((VerticalAlignment) 0);
      }
      return usersGroupsList;
    }

    private TextBlock GetUsersGroupsDescriptionTextBlockMediumSmall(double usersGroupsTotalWidth)
    {
      string str1;
      if (this._hasUsersDescription)
      {
        str1 = this._usersDescription;
      }
      else
      {
        if (!this._hasGroupsDescription)
          return  null;
        str1 = this._groupsDescription;
      }
      double num1 = this._width - usersGroupsTotalWidth - 32.0;
      if (!this._hasImages && !this._hasTitle && !this._hasMessage)
        num1 -= 48.0;
      TextBlock textBlock = new TextBlock();
      double num2 = num1;
      ((FrameworkElement) textBlock).Width = num2;
      Thickness thickness = new Thickness(16.0, 19.0, 0.0, 0.0);
      ((FrameworkElement) textBlock).Margin = thickness;
      int num3 = 0;
      ((FrameworkElement) textBlock).VerticalAlignment = ((VerticalAlignment) num3);
      double num4 = 18.0;
      textBlock.FontSize = num4;
      int num5 = 1;
      textBlock.LineStackingStrategy=((LineStackingStrategy) num5);
      double num6 = 22.0;
      textBlock.LineHeight = num6;
      FontFamily fontFamily = new FontFamily("Segoe WP");
      textBlock.FontFamily = fontFamily;
      SolidColorBrush solidColorBrush = (SolidColorBrush) Application.Current.Resources["PhoneDarkGrayBrush"];
      textBlock.Foreground = ((Brush) solidColorBrush);
      string str2 = str1;
      textBlock.Text = str2;
      return textBlock;
    }

    private ItemsControl GetUsersGroupsList()
    {
      int num1 = this._newsfeed.layout == UserNotificationNewsfeedLayout.banner ? 5 : 4;
      IList list1;
      if (this._hasUsers)
      {
        list1 = (IList) Enumerable.ToList<User>(Enumerable.Take<User>(this._users, num1));
      }
      else
      {
        if (!this._hasGroups)
          return  null;
        list1 = (IList) Enumerable.ToList<Group>(Enumerable.Take<Group>(this._groups, num1));
      }
      ItemsControl itemsControl = new ItemsControl();
      double num2 = (double) list1.Count * 44.0 - 4.0;
      ((FrameworkElement) itemsControl).Width = num2;
      double num3 = 40.0;
      ((FrameworkElement) itemsControl).Height = num3;
      IList list2 = list1;
      itemsControl.ItemsSource = ((IEnumerable) list2);
      DataTemplate dataTemplate = (DataTemplate) base.Resources["UserGroupItemTemplate"];
      itemsControl.ItemTemplate = dataTemplate;
      ItemsPanelTemplate itemsPanelTemplate = (ItemsPanelTemplate) base.Resources["HorizontalItemsPanelTemplate"];
      itemsControl.ItemsPanel = itemsPanelTemplate;
      return itemsControl;
    }

    private FrameworkElement GetButton(double maxWidth, double marginTop, double marginLeft = 0.0)
    {
      EventHandler<System.Windows.Input.GestureEventArgs> eventHandler1 = (EventHandler<System.Windows.Input.GestureEventArgs>) ((sender, args) =>
      {
        args.Handled = true;
        switch (this._button.action.type)
        {
          case UserNotificationButtonActionType.open_url:
            this.HandleButtonTap();
            break;
          case UserNotificationButtonActionType.enable_top_newsfeed:
            this.HandleNewsfeedPromoButton();
            break;
        }
      });
      if (this._button.style == UserNotificationButtonStyle.cell)
      {
        Grid grid = new Grid();
        double num1 = 56.0;
        ((FrameworkElement) grid).Height = num1;
        double width = this._width;
        ((FrameworkElement) grid).Width = width;
        Thickness thickness1 = new Thickness(0.0, marginTop, 0.0, 0.0);
        ((FrameworkElement) grid).Margin = thickness1;
        Rectangle rectangle1 = new Rectangle();
        double num2 = 1.0;
        ((FrameworkElement) rectangle1).Height = num2;
        Thickness thickness2 = new Thickness(16.0, 0.0, 16.0, 0.0);
        ((FrameworkElement) rectangle1).Margin = thickness2;
        int num3 = 0;
        ((FrameworkElement) rectangle1).VerticalAlignment = ((VerticalAlignment) num3);
        SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneForegroundBrush"];
        ((Shape) rectangle1).Fill = ((Brush) solidColorBrush1);
        double num4 = 0.1;
        ((UIElement) rectangle1).Opacity = num4;
        Rectangle rectangle2 = rectangle1;
        ((PresentationFrameworkCollection<UIElement>) ((Panel) grid).Children).Add((UIElement) rectangle2);
        Border border1 = new Border();
        SolidColorBrush solidColorBrush2 = new SolidColorBrush(Colors.Transparent);
        border1.Background = ((Brush) solidColorBrush2);
        Border border2 = border1;
        MetroInMotion.SetTilt((DependencyObject) border2, 1.5);
        TextBlock textBlock1 = new TextBlock();
        Thickness thickness3 = new Thickness(0.0, 12.0, 0.0, 0.0);
        ((FrameworkElement) textBlock1).Margin = thickness3;
        int num5 = 0;
        ((FrameworkElement) textBlock1).VerticalAlignment = ((VerticalAlignment) num5);
        FontFamily fontFamily = new FontFamily("Segoe WP Semibold");
        textBlock1.FontFamily = fontFamily;
        double num6 = 21.33;
        textBlock1.FontSize = num6;
        SolidColorBrush solidColorBrush3 = (SolidColorBrush) Application.Current.Resources["PhoneAccentBlueBrush"];
        textBlock1.Foreground = ((Brush) solidColorBrush3);
        int num7 = 0;
        textBlock1.TextAlignment=((TextAlignment) num7);
        string title = this._button.title;
        textBlock1.Text = title;
        TextBlock textBlock2 = textBlock1;
        border2.Child = ((UIElement) textBlock2);
        ((PresentationFrameworkCollection<UIElement>) ((Panel) grid).Children).Add((UIElement) border2);
        EventHandler<System.Windows.Input.GestureEventArgs> eventHandler2 = eventHandler1;
        ((UIElement) grid).Tap += (eventHandler2);
        return (FrameworkElement) grid;
      }
      Style style1 = (Style) Application.Current.Resources[this._button.style == UserNotificationButtonStyle.primary ? "VKButtonPrimaryStyle" : "VKButtonSecondaryStyle"];
      Button button = new Button();
      Thickness thickness = new Thickness(marginLeft - 12.0, marginTop - 12.0, -12.0, -12.0);
      ((FrameworkElement) button).Margin = thickness;
      double num8 = 68.0;
      ((FrameworkElement) button).Height = num8;
      double num9 = maxWidth + 24.0;
      ((FrameworkElement) button).MaxWidth = num9;
      Style style2 = style1;
      ((FrameworkElement) button).Style = style2;
      string title1 = this._button.title;
      ((ContentControl) button).Content = title1;
      EventHandler<System.Windows.Input.GestureEventArgs> eventHandler3 = eventHandler1;
      ((UIElement) button).Tap += (eventHandler3);
      return (FrameworkElement) button;
    }

    public double CalculateTotalHeight()
    {
      return this._fixedHeight;
    }

    private void Dismiss_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.HideNotification(NewsFeedNotificationHideReason.decline);
    }

    private void HandleNewsfeedPromoButton()
    {
      AppGlobalStateManager.Current.GlobalState.NewsfeedTopEnabled = true;
      NewsViewModel.Instance.TopFeedPromoAnswer = new bool?(true);
      NewsViewModel.Instance.TopFeedPromoId = this._userNotification.id;
      NewsViewModel.Instance.UpdateFeedType();
      this.HideNotification(NewsFeedNotificationHideReason.accept);
    }

    private async void HandleButtonTap()
    {
      if (this._isNavigating || string.IsNullOrEmpty(this._navigationUrl))
        return;
      this._isNavigating = true;
      if (this._navigationUrl.StartsWith("webview"))
      {
        this._navigationUrl = string.Format("https{0}", this._navigationUrl.Substring("webview".Length));
        Navigator.Current.NavigateToWebViewPage(this._navigationUrl, false);
      }
      else
      {
        Navigator.Current.NavigateToWebUri(this._navigationUrl, false, false);
        await Task.Delay(300);
      }
      this.HideNotification(NewsFeedNotificationHideReason.accept);
      this._isNavigating = false;
    }

    private void HideNotification(NewsFeedNotificationHideReason reason)
    {
      InternalService.Instance.HideUserNotification(this._userNotification.id, reason, (Action<BackendResult<bool, ResultCode>>) (result => {}));
      Action hideCallback = this._hideCallback;
      if (hideCallback == null)
        return;
      hideCallback();
    }

    public override void LoadFullyNonVirtualizableItems()
    {
      if (this._image == null)
        return;
      if (this._newsfeed.layout == UserNotificationNewsfeedLayout.banner)
      {
        ((Panel) this._imageContainer).Background = ((Brush) Application.Current.Resources["PhoneChromeBrush"]);
        this._image.ImageOpened-=(new EventHandler<RoutedEventArgs>(this.OnImageOpened));
        this._image.ImageOpened+=(new EventHandler<RoutedEventArgs>(this.OnImageOpened));
      }
      VeryLowProfileImageLoader.SetUriSource(this._image, this._imageUri);
    }

    public override void ReleaseResources()
    {
      if (this._image == null)
        return;
      VeryLowProfileImageLoader.SetUriSource(this._image,  null);
      ((Panel) this._imageContainer).Background = ((Brush) Application.Current.Resources["PhoneChromeBrush"]);
    }

    public override void ShownOnScreen()
    {
      if (!(this._imageUri !=  null) || !this._imageUri.IsAbsoluteUri)
        return;
      VeryLowProfileImageLoader.SetPriority(this._imageUri.OriginalString, DateTime.Now.Ticks);
    }

    private void OnImageOpened(object sender, RoutedEventArgs routedEventArgs)
    {
      if (this._imageContainer == null)
        return;
      ((Panel) this._imageContainer).Background = ((Brush) new SolidColorBrush(Colors.Transparent));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/NewsfeedNotificationUC.xaml", UriKind.Relative));
      this.stackPanel = (StackPanel) base.FindName("stackPanel");
      this.canvasDismiss = (Canvas) base.FindName("canvasDismiss");
    }
  }
}
