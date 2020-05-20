using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.CommonExtensions;

namespace VKClient.Common.UC
{
  public class BirthdayListItemUC : UserControl
  {
      public static readonly DependencyProperty UserNameProperty = DependencyProperty.Register("UserName", typeof(string), typeof(BirthdayListItemUC), new PropertyMetadata(new PropertyChangedCallback((d, e) => ((BirthdayListItemUC)d).UpdateName())));
      public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(BirthdayListItemUC), new PropertyMetadata(new PropertyChangedCallback((d, e) => ((BirthdayListItemUC)d).UpdateDescription())));
      public static readonly DependencyProperty GiftVisibilityProperty = DependencyProperty.Register("GiftVisibility", typeof(Visibility), typeof(BirthdayListItemUC), new PropertyMetadata(new PropertyChangedCallback((d, e) => ((BirthdayListItemUC)d).UpdateGiftVisibility())));
    internal TextBlock textBlockUserName;
    internal TextBlock textBlockDescription;
    internal Border borderSendGift;
    private bool _contentLoaded;

    public string UserName
    {
      get
      {
        return (string) base.GetValue(BirthdayListItemUC.UserNameProperty);
      }
      set
      {
        base.SetValue(BirthdayListItemUC.UserNameProperty, value);
      }
    }

    public string Description
    {
      get
      {
        return (string) base.GetValue(BirthdayListItemUC.DescriptionProperty);
      }
      set
      {
        base.SetValue(BirthdayListItemUC.DescriptionProperty, value);
      }
    }

    public Visibility GiftVisibility
    {
      get
      {
        return (Visibility) base.GetValue(BirthdayListItemUC.GiftVisibilityProperty);
      }
      set
      {
        base.SetValue(BirthdayListItemUC.GiftVisibilityProperty, value);
      }
    }

    public event EventHandler<System.Windows.Input.GestureEventArgs> ItemTap;

    public event EventHandler<System.Windows.Input.GestureEventArgs> ItemHold;

    public event EventHandler<System.Windows.Input.GestureEventArgs> GiftTap;

    public event EventHandler<System.Windows.Input.GestureEventArgs> GiftHold;

    public BirthdayListItemUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void UpdateName()
    {
      this.textBlockUserName.Text = this.UserName;
      this.UpdateNameSize();
    }

    private void UpdateDescription()
    {
      this.textBlockDescription.Text = this.Description;
      ((UIElement) this.textBlockDescription).Visibility = ((!string.IsNullOrEmpty(this.Description)).ToVisiblity());
      this.UpdateName();
    }

    private void UpdateGiftVisibility()
    {
      ((UIElement) this.borderSendGift).Visibility = this.GiftVisibility;
      this.UpdateName();
    }

    private void UpdateNameSize()
    {
      double maxWidth = base.Width - 96.0;
      Thickness margin1;
      if (((UIElement) this.borderSendGift).Visibility == Visibility.Visible)
      {
        double num1 = maxWidth;
        Thickness margin2 = ((FrameworkElement) this.borderSendGift).Margin;
        // ISSUE: explicit reference operation
        double num2 = ((Thickness) @margin2).Left + ((FrameworkElement) this.borderSendGift).Width;
        margin1 = ((FrameworkElement) this.borderSendGift).Margin;
        // ISSUE: explicit reference operation
        double right = margin1.Right;
        double num3 = num2 + right;
        maxWidth = num1 - num3;
      }
      if (!string.IsNullOrEmpty(this.Description))
      {
        double num1 = maxWidth;
        margin1 = ((FrameworkElement) this.textBlockDescription).Margin;
        // ISSUE: explicit reference operation
        double num2 = margin1.Left + ((FrameworkElement) this.textBlockDescription).ActualWidth;
        margin1 = ((FrameworkElement) this.textBlockDescription).Margin;
        // ISSUE: explicit reference operation
        double right = margin1.Right;
        double num3 = num2 + right;
        maxWidth = num1 - num3;
      }
      this.textBlockUserName.CorrectText(maxWidth);
    }

    private void Birthday_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      EventHandler<System.Windows.Input.GestureEventArgs> itemTap = this.ItemTap;
      if (itemTap == null)
        return;
      object sender1 = sender;
      GestureEventArgs e1 = e;
      itemTap(sender1, e1);
    }

    private void Birthday_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      EventHandler<System.Windows.Input.GestureEventArgs> itemHold = this.ItemHold;
      if (itemHold == null)
        return;
      object sender1 = sender;
      GestureEventArgs e1 = e;
      itemHold(sender1, e1);
    }

    private void SendGift_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      EventHandler<System.Windows.Input.GestureEventArgs> giftTap = this.GiftTap;
      if (giftTap == null)
        return;
      object sender1 = sender;
      GestureEventArgs e1 = e;
      giftTap(sender1, e1);
    }

    private void SendGift_OnHold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      EventHandler<System.Windows.Input.GestureEventArgs> giftHold = this.GiftHold;
      if (giftHold == null)
        return;
      object sender1 = sender;
      GestureEventArgs e1 = e;
      giftHold(sender1, e1);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/BirthdayListItemUC.xaml", UriKind.Relative));
      this.textBlockUserName = (TextBlock) base.FindName("textBlockUserName");
      this.textBlockDescription = (TextBlock) base.FindName("textBlockDescription");
      this.borderSendGift = (Border) base.FindName("borderSendGift");
    }
  }
}
