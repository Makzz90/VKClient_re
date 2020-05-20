using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;

namespace VKClient.Common.Library.VirtItems
{
  public class VoiceMessageItem : VirtualizableItemBase, IHandle<VoiceMessageUploaded>, IHandle
  {
    private const int FIXED_HEIGHT = 56;
    private const int BUTTON_WIDTH_HEIGHT = 56;
    private readonly double _portraitWidth;
    private readonly double _landscapeWidth;
    private bool _isHorizontal;
    private Border _borderPlay;
    private Border _borderPause;
    private WaveformControl _waveformControl;
    private TextBlock _textBlockDuration;
    private const int WAVEFORM_MARGIN_LEFT = 68;
    private readonly VoiceMessagePlayerWrapper _playerWrapper;
    private bool _isInitialized;

    public bool IsHorizontal
    {
      get
      {
        return this._isHorizontal;
      }
      set
      {
        if (this._isHorizontal == value)
          return;
        this._isHorizontal = value;
        this.UpdateWaveformWidth();
      }
    }

    private double CurrentWidth
    {
      get
      {
        if (!this._isHorizontal)
          return this._portraitWidth;
        return this._landscapeWidth;
      }
    }

    public Doc Doc { get; private set; }

    public override double FixedHeight
    {
      get
      {
        return 56.0;
      }
    }

    public VoiceMessageItem(double width, Thickness margin, Doc voiceMessageDoc, bool isHorizontal, double landscapeWidth)
      : base(width, margin,  new Thickness())
    {
      this._portraitWidth = width;
      this._isHorizontal = isHorizontal;
      this._landscapeWidth = landscapeWidth;
      this.Doc = voiceMessageDoc;
      this._playerWrapper = new VoiceMessagePlayerWrapper(voiceMessageDoc)
      {
        MediaEnded = (Action) (() => EventAggregator.Current.Publish(new VoiceMessagePlayEndedEvent(this.Doc)))
      };
      EventAggregator.Current.Subscribe(this);
    }

    private void UpdateWaveformWidth()
    {
      if (this._waveformControl == null)
        return;
      this._playerWrapper.WaveformWidth = Math.Max(0.0, this.CurrentWidth - 68.0);
    }

    protected override void GenerateChildren()
    {
      double maximum = 0.0;
      double num1 = 0.0;
      string currentDuration = "";
      if (this._isInitialized)
      {
        maximum = ((RangeBase) this._waveformControl).Maximum;
        num1 = ((RangeBase) this._waveformControl).Value;
        currentDuration = this._textBlockDuration.Text;
      }
      Ellipse ellipse = new Ellipse();
      double num2 = 56.0;
      ((FrameworkElement) ellipse).Width = num2;
      double num3 = 56.0;
      ((FrameworkElement) ellipse).Height = num3;
      double num4 = 2.0;
      ((Shape) ellipse).StrokeThickness = num4;
      SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneAccentBlueBrush"];
      ((Shape) ellipse).Stroke=((Brush) solidColorBrush1);
      this.Children.Add((FrameworkElement) ellipse);
      Grid grid1 = new Grid();
      double num5 = 56.0;
      ((FrameworkElement) grid1).Width = num5;
      double num6 = 56.0;
      ((FrameworkElement) grid1).Height = num6;
      SolidColorBrush solidColorBrush2 = new SolidColorBrush(Colors.Transparent);
      ((Panel) grid1).Background = ((Brush) solidColorBrush2);
      Grid grid2 = grid1;
      MetroInMotion.SetTilt((DependencyObject) grid2, 1.5);
      ImageBrush imageBrush1 = new ImageBrush();
      ImageLoader.SetImageBrushMultiResSource(imageBrush1, "/Resources/WallPost/AttachPlay.png");
      Border border1 = new Border();
      double num7 = 32.0;
      ((FrameworkElement) border1).Width = num7;
      double num8 = 32.0;
      ((FrameworkElement) border1).Height = num8;
      SolidColorBrush solidColorBrush3 = (SolidColorBrush) Application.Current.Resources["PhoneAccentBlueBrush"];
      border1.Background = ((Brush) solidColorBrush3);
      ImageBrush imageBrush2 = imageBrush1;
      ((UIElement) border1).OpacityMask=((Brush) imageBrush2);
      this._borderPlay = border1;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) grid2).Children).Add((UIElement) this._borderPlay);
      ImageBrush imageBrush3 = new ImageBrush();
      ImageLoader.SetImageBrushMultiResSource(imageBrush3, "/Resources/WallPost/AttachPause.png");
      Border border2 = new Border();
      double num9 = 32.0;
      ((FrameworkElement) border2).Width = num9;
      double num10 = 32.0;
      ((FrameworkElement) border2).Height = num10;
      int num11 = 1;
      ((UIElement) border2).Visibility = ((Visibility) num11);
      SolidColorBrush solidColorBrush4 = (SolidColorBrush) Application.Current.Resources["PhoneAccentBlueBrush"];
      border2.Background = ((Brush) solidColorBrush4);
      ImageBrush imageBrush4 = imageBrush3;
      ((UIElement) border2).OpacityMask=((Brush) imageBrush4);
      this._borderPause = border2;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) grid2).Children).Add((UIElement) this._borderPause);
      ((UIElement) grid2).Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.ToggleButtonPlayer_OnClick));
      this.Children.Add((FrameworkElement) grid2);
      double num12 = this.CurrentWidth - 68.0;
      Border border3 = new Border();
      double num13 = num12;
      ((FrameworkElement) border3).Width = num13;
      Border border4 = border3;
      Canvas.SetLeft((UIElement) border4, 68.0);
      WaveformControl waveformControl = new WaveformControl();
      double num14 = num12;
      ((FrameworkElement) waveformControl).Width = num14;
      double num15 = 32.0;
      ((FrameworkElement) waveformControl).Height = num15;
      int num16 = 0;
      ((Control) waveformControl).IsEnabled = (num16 != 0);
      this._waveformControl = waveformControl;
      border4.Child = ((UIElement) this._waveformControl);
      this.Children.Add((FrameworkElement) border4);
      TextBlock textBlock = new TextBlock();
      double num17 = 18.0;
      textBlock.FontSize = num17;
      SolidColorBrush solidColorBrush5 = (SolidColorBrush) Application.Current.Resources["PhoneBlue300_GrayBlue100Brush"];
      textBlock.Foreground = ((Brush) solidColorBrush5);
      string str = "00:00";
      textBlock.Text = str;
      this._textBlockDuration = textBlock;
      Canvas.SetLeft((UIElement) this._textBlockDuration, 68.0);
      Canvas.SetTop((UIElement) this._textBlockDuration, 32.0);
      this.Children.Add((FrameworkElement) this._textBlockDuration);
      this._playerWrapper.Init(this._waveformControl, this._textBlockDuration, this._borderPlay, this._borderPause);
      this._playerWrapper.SetValues(maximum, num1, currentDuration);
      this.UpdateWaveformWidth();
      this._isInitialized = true;
    }

    private void ToggleButtonPlayer_OnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      this.PlayPause();
    }

    public void PlayPause()
    {
      this._playerWrapper.PlayPause();
    }

    public void Handle(VoiceMessageUploaded message)
    {
      Doc voiceMessageDoc = message.VoiceMessageDoc;
      Doc doc = this.Doc;
      if (doc == null || doc.guid == Guid.Empty || doc.guid != voiceMessageDoc.guid)
        return;
      doc.owner_id = voiceMessageDoc.owner_id;
      doc.id = voiceMessageDoc.id;
    }
  }
}
