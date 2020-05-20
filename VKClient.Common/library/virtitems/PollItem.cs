using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;

namespace VKClient.Common.Library.VirtItems
{
  public class PollItem : VirtualizableItemBase
  {
    private readonly Poll _poll;
    private readonly long _topicId;
    private double _fixedHeight;
    private PollUC _pollUC;
    private TextItem _pollDescriptionTextItem;
    private PollViewModel _pvm;

    public override double FixedHeight
    {
      get
      {
        return this._fixedHeight;
      }
    }

    public PollItem(double width, Thickness margin, Poll poll, long topicId = 0)
      : base(width, margin, new Thickness())
    {
      this._poll = poll;
      this._topicId = topicId;
      this.CreateLayout();
    }

    private void CreateLayout()
    {
      this._pvm = new PollViewModel(this._poll, this._topicId);
      this._pvm.PropertyChanged += new PropertyChangedEventHandler(this.pvm_PropertyChanged);
      TextItem textItem = new TextItem(this.Width, new Thickness(16.0, 0.0, 16.0, 0.0), this._pvm.Question, true, 20.0, "Segoe WP Semibold", VKConstants.LineHeight,  null, true,  null);
      this.VirtualizableChildren.Add((IVirtualizable) textItem);
      this._pollDescriptionTextItem = new TextItem(this.Width, new Thickness(16.0, textItem.FixedHeight, 16.0, 0.0), string.Format("{0}, {1}", this._pvm.PollTypeStr, this._pvm.VotedCountStr), false, 20.0, "Segoe WP", 0.0, Application.Current.Resources["PhoneVKSubtleBrush"] as SolidColorBrush, true,  null);
      this.VirtualizableChildren.Add((IVirtualizable) this._pollDescriptionTextItem);
      PollUC pollUc = new PollUC();
      double width = this.Width;
      ((FrameworkElement) pollUc).Width = width;
      double num1 = 0.0;
      Thickness margin1 = this._pollDescriptionTextItem.Margin;
      // ISSUE: explicit reference operation
      double num2 = margin1.Top + this._pollDescriptionTextItem.FixedHeight + 2.0;
      double num3 = 0.0;
      double num4 = 0.0;
      Thickness thickness = new Thickness(num1, num2, num3, num4);
      ((FrameworkElement) pollUc).Margin = thickness;
      this._pollUC = pollUc;
      this._pollUC.Initialize(this._poll, this._topicId);
      Thickness margin2 = ((FrameworkElement) this._pollUC).Margin;
      // ISSUE: explicit reference operation
      this._fixedHeight = ((Thickness) @margin2).Top + this._pollUC.CalculateTotalHeight() + 4.0;
    }

    private void pvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      this._pollDescriptionTextItem.UpdateText(this._pvm.VotedCountStr);
    }

    protected override void GenerateChildren()
    {
      this.Children.Add((FrameworkElement) this._pollUC);
    }
  }
}
