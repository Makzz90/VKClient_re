using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKMessenger.Library.Events;

namespace VKMessenger.Library.VirtItems
{
  public class MessageItem : VirtualizableItemBase, ISupportOrientationChange
  {
    private static readonly Thickness ChatIncomingMessageMargin = new Thickness(68.0, 0.0, 0.0, 0.0);
    private static readonly Thickness UserIncomingMessageMargin = new Thickness(0.0, 0.0, 0.0, 0.0);
    private static readonly double VERTICAL_WIDTH = 448.0;
    private MessageViewModel _mvm;
    private double _height;
    private Rectangle _bubbleRect;
    private Path _calloutPath;
    private Path _selectionPath;
    private TapHandlerItem _tapHandlerItem;
    private bool _isHorizontalOrientation;
    private User _userTyping;
    private bool _isTypingItem;
    private bool _isTypingInChat;
    private MessageContentItem _messageContentItem;
    private SystemMessageItem _systemMessageItem;
    private bool _handledDelivered;

    public MessageViewModel MVM
    {
      get
      {
        return this._mvm;
      }
    }

    public bool IsSystemMessage
    {
      get
      {
        return !string.IsNullOrWhiteSpace(this._mvm.Message.action);
      }
    }

    public bool IsHorizontalOrientation
    {
      get
      {
        return this._isHorizontalOrientation;
      }
      set
      {
        if (this._isHorizontalOrientation == value)
          return;
        this._isHorizontalOrientation = value;
        this.UpdateLayout();
      }
    }

    private static double HORIZONTAL_WIDTH
    {
      get
      {
        return ScaleFactor.IsFullHD() ? 821.0 : 768.0;
      }
    }

    public override double FixedHeight
    {
      get
      {
        return this._height;
      }
    }

    public MessageItem(MessageViewModel mvm, bool isHorizontalOrientation = false)
      : base(MessageItem.VERTICAL_WIDTH, new Thickness(0.0, 0.0, 0.0, 14.0),  new Thickness())
    {
      this._mvm = mvm;
      this._isHorizontalOrientation = isHorizontalOrientation;
      this.Width = this.IsHorizontalOrientation ? MessageItem.HORIZONTAL_WIDTH : MessageItem.VERTICAL_WIDTH;
      if (mvm.TypingUserId != 0L)
      {
        this._isTypingItem = true;
        this._isTypingInChat = mvm.TypingInChat;
        this._userTyping = mvm.TypingUser;
      }
      this.CreateLayout();
    }

    private new void UpdateLayout()
    {
      this.Width = this.IsHorizontalOrientation ? MessageItem.HORIZONTAL_WIDTH : MessageItem.VERTICAL_WIDTH;
      if (this._isTypingItem)
        return;
      if (!this.IsSystemMessage)
      {
        this._messageContentItem.IsHorizontalOrientation = this.IsHorizontalOrientation;
        ((FrameworkElement) this._bubbleRect).Width = this._messageContentItem.Width;
        ((FrameworkElement) this._bubbleRect).Height = this._messageContentItem.FixedHeight;
        double width = this._messageContentItem.Width;
        switch (this._mvm.MessageDirectionType)
        {
          case MessageDirectionType.InFromUser:
            ((FrameworkElement) this._selectionPath).Margin=(new Thickness(this._messageContentItem.Width + 10.0, 10.0, 0.0, 0.0));
            break;
          case MessageDirectionType.InFromUserInChat:
            Path selectionPath1 = this._selectionPath;
            Thickness margin1 = this._messageContentItem.Margin;
            // ISSUE: explicit reference operation
            Thickness thickness1 = new Thickness(margin1.Left + this._messageContentItem.Width + 10.0, 10.0, 0.0, 0.0);
            ((FrameworkElement) selectionPath1).Margin = thickness1;
            break;
          case MessageDirectionType.OutToUser:
          case MessageDirectionType.OutToChat:
            this._messageContentItem.Margin = new Thickness(this.Width - width, 0.0, 0.0, 0.0);
            Path selectionPath2 = this._selectionPath;
            Thickness margin2 = this._messageContentItem.Margin;
            // ISSUE: explicit reference operation
            Thickness thickness2 = new Thickness(((Thickness) @margin2).Left - 10.0 - ((FrameworkElement) this._selectionPath).Width, 10.0, 0.0, 0.0);
            ((FrameworkElement) selectionPath2).Margin = thickness2;
            ((FrameworkElement) this._calloutPath).Margin=(new Thickness(this.Width - 1.0, 16.0, 0.0, 0.0));
            ((FrameworkElement) this._bubbleRect).Margin=(new Thickness(this.Width - width, 0.0, 0.0, 0.0));
            break;
        }
        this._height = this._messageContentItem.FixedHeight;
        this._tapHandlerItem.SetWidthHeight(this.Width, this._height);
      }
      else
      {
        this._systemMessageItem.IsHorizontal = this.IsHorizontalOrientation;
        this._height = this._systemMessageItem.FixedHeight;
      }
    }

    private void CreateMenu()
    {
      if (this.IsSystemMessage || this._isTypingItem)
        return;
      List<MenuItem> menuItems = new List<MenuItem>();
      if (this._mvm.Message.id != 0)
      {
        MenuItem menuItem1 = new MenuItem();
        string conversationAppBarChoose = CommonResources.Conversation_AppBar_Choose;
        menuItem1.Header = conversationAppBarChoose;
        MenuItem menuItem2 = menuItem1;
        // ISSUE: method pointer
        menuItem2.Click += new RoutedEventHandler( this.miSelect_Click);
        menuItems.Add(menuItem2);
        MenuItem menuItem3 = new MenuItem();
        string conversationQuote = CommonResources.Conversation_Quote;
        menuItem3.Header = conversationQuote;
        MenuItem menuItem4 = menuItem3;
        // ISSUE: method pointer
        menuItem4.Click += new RoutedEventHandler( this.miQuote_Click);
        menuItems.Add(menuItem4);
        MenuItem menuItem5 = new MenuItem();
        string conversationForward = CommonResources.Conversation_Forward;
        menuItem5.Header = conversationForward;
        MenuItem menuItem6 = menuItem5;
        // ISSUE: method pointer
        menuItem6.Click += new RoutedEventHandler( this.miForward_Click);
        menuItems.Add(menuItem6);
        if (!string.IsNullOrWhiteSpace(this._mvm.Message.body))
        {
          MenuItem menuItem7 = new MenuItem();
          string conversationCopy = CommonResources.Conversation_Copy;
          menuItem7.Header = conversationCopy;
          MenuItem menuItem8 = menuItem7;
          // ISSUE: method pointer
          menuItem8.Click += new RoutedEventHandler( this.miCopy_Click);
          menuItems.Add(menuItem8);
        }
      }
      MenuItem menuItem9 = new MenuItem();
      string conversationDelete = CommonResources.Conversation_Delete;
      menuItem9.Header = conversationDelete;
      MenuItem menuItem10 = menuItem9;
      // ISSUE: method pointer
      menuItem10.Click += new RoutedEventHandler( this.miDelete_Click);
      menuItems.Add(menuItem10);
      this.SetMenu(menuItems);
    }

    private void miSelect_Click(object sender, RoutedEventArgs e)
    {
      EventAggregator current = EventAggregator.Current;
      MessageActionEvent messageActionEvent = new MessageActionEvent();
      messageActionEvent.MessageActionType = MessageActionType.EnterSelectMode;
      MessageViewModel mvm = this._mvm;
      messageActionEvent.Message = mvm;
      current.Publish(messageActionEvent);
    }

    private void miDelete_Click(object sender, RoutedEventArgs e)
    {
      if (MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, CommonResources.Conversation_DeleteMessage, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      EventAggregator current = EventAggregator.Current;
      MessageActionEvent messageActionEvent = new MessageActionEvent();
      messageActionEvent.MessageActionType = MessageActionType.Delete;
      MessageViewModel mvm = this._mvm;
      messageActionEvent.Message = mvm;
      current.Publish(messageActionEvent);
    }

    private void miCopy_Click(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(this._mvm.UIMessageText))
        return;
      Clipboard.SetText(this._mvm.UIMessageText);
    }

    private void miForward_Click(object sender, RoutedEventArgs e)
    {
      EventAggregator current = EventAggregator.Current;
      MessageActionEvent messageActionEvent = new MessageActionEvent();
      messageActionEvent.MessageActionType = MessageActionType.Forward;
      MessageViewModel mvm = this._mvm;
      messageActionEvent.Message = mvm;
      current.Publish(messageActionEvent);
    }

    private void miQuote_Click(object sender, RoutedEventArgs e)
    {
      EventAggregator current = EventAggregator.Current;
      MessageActionEvent messageActionEvent = new MessageActionEvent();
      messageActionEvent.MessageActionType = MessageActionType.Quote;
      MessageViewModel mvm = this._mvm;
      messageActionEvent.Message = mvm;
      current.Publish(messageActionEvent);
    }

    private void CreateLayout()
    {
      try
      {
        if (!this.IsSystemMessage)
        {
          if (!this._isTypingItem)
          {
            bool flag = !string.IsNullOrWhiteSpace(this._mvm.Message.body);
            int num1 = this._mvm.Attachments == null ? 0 : (Enumerable.Any<AttachmentViewModel>(this._mvm.Attachments, (Func<AttachmentViewModel, bool>)(a => a.AttachmentType == AttachmentType.Gift)) ? 1 : 0);
            double verticalWidth = 356.0;
            double horizontalWidth = 512.0;
            if (num1 != 0 && !flag)
              verticalWidth = horizontalWidth = 256.0;
            this._messageContentItem = new MessageContentItem(verticalWidth,  new Thickness(), this._mvm, horizontalWidth, this._isHorizontalOrientation, 0);
            MessageDirectionType messageDirectionType = this._mvm.MessageDirectionType;
            double width = this._messageContentItem.Width;
            SolidColorBrush bgBrush1 = this._mvm.BGBrush;
            Rectangle rectangle = new Rectangle();
            SolidColorBrush solidColorBrush = bgBrush1;
            ((Shape) rectangle).Fill = ((Brush) solidColorBrush);
            double num2 = width;
            ((FrameworkElement) rectangle).Width = num2;
            double fixedHeight = this._messageContentItem.FixedHeight;
            ((FrameworkElement) rectangle).Height = fixedHeight;
            this._bubbleRect = rectangle;
            this._selectionPath = new Path();
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure1 = new PathFigure();
            PathFigure pathFigure2 = pathFigure1;
            Point point1 =  new Point();
            // ISSUE: explicit reference operation
            point1.X = 361.0;
            // ISSUE: explicit reference operation
            point1.Y = 239.0;
            Point point2 = point1;
            pathFigure2.StartPoint = point2;
            LineSegment lineSegment1 = new LineSegment();
            Point p1 = new Point();
            // ISSUE: explicit reference operation
            p1.X = 371.0;
            // ISSUE: explicit reference operation
            p1.Y = 250.0;
            Point point3 = p1;
            lineSegment1.Point = point3;
            LineSegment lineSegment2 = lineSegment1;
            LineSegment lineSegment3 = new LineSegment();
            p1 = new Point(); 
            // ISSUE: explicit reference operation
            p1.X = 387.0;
            // ISSUE: explicit reference operation
            p1.Y = 230.0;
            Point point4 = p1;
            lineSegment3.Point = point4;
            LineSegment lineSegment4 = lineSegment3;
            ((PresentationFrameworkCollection<PathSegment>) pathFigure1.Segments).Add((PathSegment) lineSegment2);
            ((PresentationFrameworkCollection<PathSegment>) pathFigure1.Segments).Add((PathSegment) lineSegment4);
            ((PresentationFrameworkCollection<PathFigure>) pathGeometry.Figures).Add(pathFigure1);
            this._selectionPath.Data=((Geometry) pathGeometry);
            ((Shape) this._selectionPath).Stretch=((Stretch) 1);
            ((FrameworkElement) this._selectionPath).Height = 20.5;
            ((FrameworkElement) this._selectionPath).Width = 25.667;
            ((Shape) this._selectionPath).StrokeThickness = 5.0;
            ((Shape) this._selectionPath).Stroke=(Application.Current.Resources["PhoneVKSubtleBrush"] as Brush);
            ((UIElement) this._selectionPath).Visibility = this._mvm.SelectionMarkVisibility;
            switch (messageDirectionType)
            {
              case MessageDirectionType.InFromUser:
                this._messageContentItem.Margin = MessageItem.UserIncomingMessageMargin;
                ((FrameworkElement) this._selectionPath).Margin=(new Thickness(this._messageContentItem.Width + 10.0, 10.0, 0.0, 0.0));
                Path path1 = new Path();
                SolidColorBrush bgBrush2 = this._mvm.BGBrush;
                ((Shape) path1).Fill = ((Brush) bgBrush2);
                double num3 = 16.0;
                ((FrameworkElement) path1).Height = num3;
                double num4 = 13.0;
                ((FrameworkElement) path1).Width = num4;
                Thickness thickness1 = new Thickness(-12.0, 16.0, 0.0, 0.0);
                ((FrameworkElement) path1).Margin = thickness1;
                int num5 = 1;
                ((Shape) path1).Stretch=((Stretch) num5);
                p1 =  new Point();
                Geometry triangleGeometry1 = PathHelper.CreateTriangleGeometry(p1, new Point(100.0, 0.0), new Point(100.0, 100.0));
                path1.Data = triangleGeometry1;
                this._calloutPath = path1;
                break;
              case MessageDirectionType.InFromUserInChat:
                this._messageContentItem.Margin = MessageItem.ChatIncomingMessageMargin;
                Path selectionPath1 = this._selectionPath;
                Thickness margin1 = this._messageContentItem.Margin;
                // ISSUE: explicit reference operation
                Thickness thickness2 = new Thickness(margin1.Left + this._messageContentItem.Width + 10.0, 10.0, 0.0, 0.0);
                ((FrameworkElement) selectionPath1).Margin = thickness2;
                if (this._mvm.UIImageUrl != null)
                  base.VirtualizableChildren.Add((IVirtualizable) new VirtualizableImage(48.0, 48.0,  new Thickness(), this._mvm.UIImageUrl, (Action<VirtualizableImage>) (vi =>
                  {
                    if (this._mvm.AssociatedUser == null)
                      return;
                    Navigator.Current.NavigateToUserProfile(this._mvm.AssociatedUser.uid, "", "", false);
                  }), "", true, true, (Stretch) 3,  null, -1.0, false, true));
                ((FrameworkElement) this._bubbleRect).Margin = MessageItem.ChatIncomingMessageMargin;
                Path path2 = new Path();
                SolidColorBrush bgBrush3 = this._mvm.BGBrush;
                ((Shape) path2).Fill = ((Brush) bgBrush3);
                double num6 = 16.0;
                ((FrameworkElement) path2).Height = num6;
                double num7 = 13.0;
                ((FrameworkElement) path2).Width = num7;
                Thickness thickness3 = new Thickness(56.0, 16.0, 0.0, 0.0);
                ((FrameworkElement) path2).Margin = thickness3;
                int num8 = 1;
                ((Shape) path2).Stretch=((Stretch) num8);
                p1 =  new Point();
                Geometry triangleGeometry2 = PathHelper.CreateTriangleGeometry(p1, new Point(100.0, 0.0), new Point(100.0, 100.0));
                path2.Data = triangleGeometry2;
                this._calloutPath = path2;
                break;
              case MessageDirectionType.OutToUser:
              case MessageDirectionType.OutToChat:
                this._messageContentItem.Margin = new Thickness(this.Width - width, 0.0, 0.0, 0.0);
                Path selectionPath2 = this._selectionPath;
                Thickness margin2 = this._messageContentItem.Margin;
                // ISSUE: explicit reference operation
                Thickness thickness4 = new Thickness(((Thickness) @margin2).Left - 10.0 - ((FrameworkElement) this._selectionPath).Width, 10.0, 0.0, 0.0);
                ((FrameworkElement) selectionPath2).Margin = thickness4;
                Path path3 = new Path();
                SolidColorBrush bgBrush4 = this._mvm.BGBrush;
                ((Shape) path3).Fill = ((Brush) bgBrush4);
                double num9 = 16.0;
                ((FrameworkElement) path3).Height = num9;
                double num10 = 13.0;
                ((FrameworkElement) path3).Width = num10;
                Thickness thickness5 = new Thickness(this.Width - 1.0, 16.0, 0.0, 0.0);
                ((FrameworkElement) path3).Margin = thickness5;
                int num11 = 1;
                ((Shape) path3).Stretch=((Stretch) num11);
                p1 = new Point();
                Geometry triangleGeometry3 = PathHelper.CreateTriangleGeometry(p1, new Point(100.0, 0.0), new Point(0.0, 100.0));
                path3.Data = triangleGeometry3;
                this._calloutPath = path3;
                ((FrameworkElement) this._bubbleRect).Margin=(new Thickness(this.Width - width, 0.0, 0.0, 0.0));
                break;
              case MessageDirectionType.Undefined:
                return;
            }
            base.VirtualizableChildren.Add((IVirtualizable) this._messageContentItem);
            this._height = this._messageContentItem.FixedHeight;
            this._tapHandlerItem = new TapHandlerItem(this.Width, this._height, this._mvm);
            base.VirtualizableChildren.Add((IVirtualizable) this._tapHandlerItem);
          }
          else
          {
            this._height = 52.0;
            SolidColorBrush bgBrush1 = this._mvm.BGBrush;
            this._selectionPath = new Path();
            Rectangle rectangle = new Rectangle();
            SolidColorBrush solidColorBrush = bgBrush1;
            ((Shape) rectangle).Fill = ((Brush) solidColorBrush);
            double num1 = 80.0;
            ((FrameworkElement) rectangle).Width = num1;
            double num2 = 52.0;
            ((FrameworkElement) rectangle).Height = num2;
            this._bubbleRect = rectangle;
            VirtualizableImage virtualizableImage1 = new VirtualizableImage(48.0, 12.0, new Thickness(16.0, 22.0, 0.0, 0.0), MultiResolutionHelper.Instance.AppendResolutionSuffix("/Resources/New/TypingBubbleDots.png", true, ""),  null,  null, false, false, (Stretch) 3,  null, -1.0, false, false);
            base.VirtualizableChildren.Add((IVirtualizable) virtualizableImage1);
            if (this._isTypingInChat)
            {
              ((FrameworkElement) this._bubbleRect).Margin = MessageItem.ChatIncomingMessageMargin;
              VirtualizableImage virtualizableImage2 = virtualizableImage1;
              Thickness margin1 = virtualizableImage1.Margin;
              // ISSUE: explicit reference operation
              double left1 = margin1.Left;
              margin1 = ((FrameworkElement) this._bubbleRect).Margin;
              // ISSUE: explicit reference operation
              double left2 = margin1.Left;
              double num3 = left1 + left2;
              Thickness thickness1 = virtualizableImage1.Margin;
              // ISSUE: explicit reference operation
              double top1 = ((Thickness) @thickness1).Top;
              thickness1 = ((FrameworkElement) this._bubbleRect).Margin;
              // ISSUE: explicit reference operation
              double top2 = ((Thickness) @thickness1).Top;
              double num4 = top1 + top2;
              double num5 = 0.0;
              double num6 = 0.0;
              Thickness thickness2 = new Thickness(num3, num4, num5, num6);
              virtualizableImage2.Margin = thickness2;
              Path path = new Path();
              SolidColorBrush bgBrush2 = this._mvm.BGBrush;
              ((Shape) path).Fill = ((Brush) bgBrush2);
              double num7 = 16.0;
              ((FrameworkElement) path).Height = num7;
              double num8 = 13.0;
              ((FrameworkElement) path).Width = num8;
              Thickness thickness3 = new Thickness(56.0, 16.0, 0.0, 0.0);
              ((FrameworkElement) path).Margin = thickness3;
              int num9 = 1;
              ((Shape) path).Stretch=((Stretch) num9);
              Geometry triangleGeometry = PathHelper.CreateTriangleGeometry( new Point(), new Point(100.0, 0.0), new Point(100.0, 100.0));
              path.Data = triangleGeometry;
              this._calloutPath = path;
              if (this._userTyping == null || this._userTyping.photo_max == null)
                return;
              double width = 52.0;
              double height = 52.0;
              thickness1 =  new Thickness();
              Thickness margin2 = thickness1;
              string photoMax = this._userTyping.photo_max;
              Action<VirtualizableImage> callbackOnTap = (Action<VirtualizableImage>) (vi =>
              {
                if (this._mvm.AssociatedUser == null)
                  return;
                Navigator.Current.NavigateToUserProfile(this._mvm.AssociatedUser.uid, "", "", false);
              });
              string tag = "";
              int num10 = 1;
              int num11 = 1;
              int num12 = 3;
              // ISSUE: variable of the null type
              
              double placeholderOpacity = -1.0;
              int num13 = 0;
              int num14 = 1;
              base.VirtualizableChildren.Add((IVirtualizable) new VirtualizableImage(width, height, margin2, photoMax, callbackOnTap, tag, num10 != 0, num11 != 0, (Stretch) num12, null, placeholderOpacity, num13 != 0, num14 != 0));
            }
            else
            {
              Path path = new Path();
              SolidColorBrush bgBrush2 = this._mvm.BGBrush;
              ((Shape) path).Fill = ((Brush) bgBrush2);
              double num3 = 16.0;
              ((FrameworkElement) path).Height = num3;
              double num4 = 13.0;
              ((FrameworkElement) path).Width = num4;
              Thickness thickness = new Thickness(-12.0, 16.0, 0.0, 0.0);
              ((FrameworkElement) path).Margin = thickness;
              int num5 = 1;
              ((Shape) path).Stretch=((Stretch) num5);
              Geometry triangleGeometry = PathHelper.CreateTriangleGeometry(new Point(), new Point(100.0, 0.0), new Point(100.0, 100.0));
              path.Data = triangleGeometry;
              this._calloutPath = path;
            }
          }
        }
        else
        {
          this._systemMessageItem = new SystemMessageItem(MessageItem.VERTICAL_WIDTH,  new Thickness(), this._mvm, MessageItem.HORIZONTAL_WIDTH, this._isHorizontalOrientation);
          this._height = this._systemMessageItem.FixedHeight;
          base.VirtualizableChildren.Add((IVirtualizable) this._systemMessageItem);
        }
      }
      catch (Exception )
      {
        Logger.Instance.Error("Failed to create message layout!!");
      }
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      this.CreateMenu();
      this._mvm.PropertyChanged += new PropertyChangedEventHandler(this._mvm_PropertyChanged);
      if (this._calloutPath == null)
        return;
      this.UpdateFill();
      ((UIElement) this._selectionPath).Visibility = this._mvm.SelectionMarkVisibility;
      this.Children.Add((FrameworkElement) this._selectionPath);
      this.Children.Add((FrameworkElement) this._calloutPath);
      List<Rectangle>.Enumerator enumerator = RectangleHelper.CoverByRectangles(this._bubbleRect).GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
          this.Children.Add((FrameworkElement) enumerator.Current);
      }
      finally
      {
        enumerator.Dispose();
      }
    }

    protected override void ReleaseResourcesOnUnload()
    {
      base.ReleaseResourcesOnUnload();
      this._mvm.PropertyChanged -= new PropertyChangedEventHandler(this._mvm_PropertyChanged);
    }

    private void _mvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "UIStatusDelivered" && this._mvm.UIStatusDelivered == Visibility.Visible && !this._handledDelivered)
      {
        VirtualizableState currentState = this.CurrentState;
        if (currentState != VirtualizableState.Unloaded && this._mvm.StickerAttachment == null)
        {
          this.Unload();
          this.Load(currentState);
        }
        this._handledDelivered = true;
      }
      if (e.PropertyName == "BGBrush")
        this.UpdateFill();
      if (!(e.PropertyName == "SelectionMarkVisibility"))
        return;
      ((UIElement) this._selectionPath).Visibility = this._mvm.SelectionMarkVisibility;
    }

    private void UpdateFill()
    {
      if (this._calloutPath == null || this._bubbleRect == null)
        return;
      ((Shape) this._calloutPath).Fill = ((Brush) this._mvm.BGBrush);
      ((Shape) this._bubbleRect).Fill = ((Brush) this._mvm.BGBrush);
    }

    public void SetIsHorizontal(bool isHorizontal)
    {
      this.IsHorizontalOrientation = isHorizontal;
    }
  }
}
