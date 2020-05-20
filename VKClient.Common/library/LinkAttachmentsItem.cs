using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class LinkAttachmentsItem : VirtualizableItemBase
  {
    private readonly double _marginBetweenTitleAndFirstLink = 10.0;
    private readonly double _marginBetweenLinks = 24.0;
    private readonly double _linkHeight = 62.0;
    private List<Link> _links;
    private double _height;

    public override double FixedHeight
    {
      get
      {
        return this._height;
      }
    }

    public LinkAttachmentsItem(double width, Thickness margin, List<Link> links)
      : base(width, margin, new Thickness())
    {
      this._links = links;
      this.MeasureLayout();
      MetroInMotion.SetTilt((DependencyObject) this._view, VKConstants.DefaultTilt);
      Canvas view = this._view;
      RectangleGeometry rectangleGeometry = new RectangleGeometry();
      Rect rect = new Rect(-5.0, 0.0, width + 5.0, this.FixedHeight + 10.0);
      rectangleGeometry.Rect = rect;
      ((UIElement) view).Clip=((Geometry) rectangleGeometry);
    }

    private void MeasureLayout()
    {
      if (this._links.Count == 0)
      {
        this._height = 0.0;
      }
      else
      {
        this._height = 24.0;
        this._height = this._height + ((double) this._links.Count * this._linkHeight + this._marginBetweenLinks * (double) (this._links.Count - 1));
      }
    }

    protected override void GenerateChildren()
    {
      if (this._links.Count == 0)
        return;
      Thickness thickness = new Thickness(0.0, -7.0, 0.0, 0.0);
      Canvas view = this._view;
      Thickness margin1 = thickness;
      double width1 = double.NaN;
      double height1 = double.NaN;
      string text1 = this._links.Count == 1 ? CommonResources.Link : CommonResources.Links;
      VirtualizableItemBase.TextBlockParam parameters1 = new VirtualizableItemBase.TextBlockParam();
      parameters1.Foreground = VKConstants.GrayColorHex.GetColor();
      parameters1.FontFamily = "Segoe WP Semibold";
      // ISSUE: variable of the null type
      this.CreateTextBlockAddToChildren(view, margin1, width1, height1, text1, parameters1, null);
      // ISSUE: explicit reference operation
      thickness.Top = 24.0;
      foreach (Link link in this._links)
      {
        // ISSUE: explicit reference operation
        Canvas addToChildren = this.CreateAddToChildren<Canvas>(this._view, new Thickness(0.0, ((Thickness) @thickness).Top, 0.0, 0.0), this.Width, 60.0, link.url);
        ((UIElement) addToChildren).Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.linkCanvas_Tap));
        Canvas parent1 = addToChildren;
        Thickness margin2 = new Thickness(-3.0, -15.0, 0.0, 0.0);
        double width2 = this.Width;
        double height2 = double.NaN;
        string text2 = string.IsNullOrWhiteSpace(link.title) ? link.url : link.title;
        VirtualizableItemBase.TextBlockParam parameters2 = new VirtualizableItemBase.TextBlockParam();
        parameters2.Style = "PhoneTextExtraLargeStyle";
        // ISSUE: variable of the null type
        this.CreateTextBlockAddToChildren(parent1, margin2, width2, height2, text2, parameters2, null);
        Canvas parent2 = addToChildren;
        Thickness margin3 = new Thickness(0.0, 38.0, 0.0, 0.0);
        double width3 = this.Width;
        double height3 = double.NaN;
        string url = link.url;
        VirtualizableItemBase.TextBlockParam parameters3 = new VirtualizableItemBase.TextBlockParam();
        parameters3.Style = "PhoneTextAccentStyle";
        // ISSUE: variable of the null type
        this.CreateTextBlockAddToChildren(parent2, margin3, width3, height3, url, parameters3, null);
        // ISSUE: explicit reference operation
        // ISSUE: variable of a reference type
        double num = thickness.Top + (60.0 + this._marginBetweenLinks);
        thickness.Top = num;
      }
    }

    private void linkCanvas_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      string tag = ((FrameworkElement) (sender as Canvas)).Tag as string;
      if (tag == null)
        return;
      Navigator.Current.NavigateToWebUri(tag, false, false);
      e.Handled = true;
    }
  }
}
