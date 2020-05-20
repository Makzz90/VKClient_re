using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  [TemplatePart(Name = "PART_Canvas", Type = typeof (Canvas))]
  [TemplatePart(Name = "PART_CanvasMask", Type = typeof (Canvas))]
  public class WaveformControl : Slider
  {
      public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList<int>), typeof(WaveformControl), new PropertyMetadata(new PropertyChangedCallback((d, e) => ((WaveformControl)d).RenderWaveform())));
      public static readonly DependencyProperty WaveformWidthProperty = DependencyProperty.Register("WaveformWidth", typeof(double), typeof(WaveformControl), new PropertyMetadata(new PropertyChangedCallback((d, e) => ((WaveformControl)d).UpdateWaveformWidth())));
      private const string PART_CanvasName = "PART_Canvas";
    private const string PART_CanvasMaskName = "PART_CanvasMask";
    private Canvas _canvas;
    private Canvas _canvasMask;
    private const int WAVEFORM_ITEM_MIN_HEIGHT = 3;
    private const int WAVEFORM_ITEM_MAX_HEIGHT = 32;
    private const int WAVEFORM_ITEM_WIDTH = 3;
    private const int WAVEFORM_ITEM_BETWEEN = 1;

    public IList<int> ItemsSource
    {
      get
      {
        return (IList<int>) base.GetValue(WaveformControl.ItemsSourceProperty);
      }
      set
      {
        base.SetValue(WaveformControl.ItemsSourceProperty, value);
      }
    }

    public double WaveformWidth
    {
      get
      {
        return (double) base.GetValue(WaveformControl.WaveformWidthProperty);
      }
      set
      {
        base.SetValue(WaveformControl.WaveformWidthProperty, value);
      }
    }

    public List<int> Waveform { get; set; }

    public WaveformControl()
    {
      //base.\u002Ector();
        base.DefaultStyleKey = typeof(WaveformControl);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        this._canvas = (base.GetTemplateChild("PART_Canvas") as Canvas);
        this._canvasMask = (base.GetTemplateChild("PART_CanvasMask") as Canvas);
        this.RenderWaveform();
    }

    private void UpdateWaveformWidth()
    {
      this.UpdateWaveformItemsSource(this.Waveform);
    }

    private void UpdateWaveformItemsSource(List<int> waveform)
    {
      if (waveform == null || waveform.Count == 0)
        return;
      int targetLength = (int) (Math.Max(0.0, this.WaveformWidth) / 4.0);
      List<int> intList1 = WaveformUtils.Resample(waveform, targetLength);
      int num1 = Enumerable.Max((IEnumerable<int>) intList1);
      List<int> intList2 = new List<int>();
      List<int>.Enumerator enumerator = intList1.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          int num2 = (int) Math.Round(32.0 * ((double) enumerator.Current * 1.0 / (double) num1));
          if (num2 < 3)
            num2 = 3;
          if (num2 % 2 != 0)
            ++num2;
          intList2.Add(num2);
        }
      }
      finally
      {
        enumerator.Dispose();
      }
      this.ItemsSource = (IList<int>) intList2;
    }

    private void RenderWaveform()
    {
      if (this.ItemsSource == null || this._canvas == null || this._canvasMask == null)
        return;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this._canvas).Children).Clear();
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this._canvasMask).Children).Clear();
      for (int index = 0; index < ((ICollection<int>) this.ItemsSource).Count; ++index)
      {
        int waveformItem = this.ItemsSource[index];
        int left = index * 4;
        double top = 16.0 - (double) waveformItem / 2.0;
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this._canvas).Children).Add((UIElement) this.GetWaveformItem(waveformItem, left, top));
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this._canvasMask).Children).Add((UIElement) this.GetWaveformItem(waveformItem, left, top));
      }
    }

    private FrameworkElement GetWaveformItem(int waveformItem, int left, double top)
    {
      Rectangle rectangle = new Rectangle();
      double num1 = 3.0;
      ((FrameworkElement) rectangle).Width = num1;
      double num2 = (double) waveformItem;
      ((FrameworkElement) rectangle).Height = num2;
      double num3 = 3.0;
      rectangle.RadiusX = num3;
      double num4 = 3.0;
      rectangle.RadiusY = num4;
      Thickness thickness = new Thickness(0.0, 0.0, 1.0, 0.0);
      ((FrameworkElement) rectangle).Margin = thickness;
      Brush foreground = ((Control) this).Foreground;
      ((Shape) rectangle).Fill = foreground;
      double num5 = (double) left;
      Canvas.SetLeft((UIElement) rectangle, num5);
      double num6 = top;
      Canvas.SetTop((UIElement) rectangle, num6);
      return (FrameworkElement) rectangle;
    }
  }
}
