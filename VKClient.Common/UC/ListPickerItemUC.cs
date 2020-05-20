using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class ListPickerItemUC : UserControl
  {
    internal VisualStateGroup CommonStates;
    internal VisualState Normal;
    internal VisualState Selected;
    internal TextBlock textBlock;
    private bool _contentLoaded;

    public ListPickerItemUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/ListPickerItemUC.xaml", UriKind.Relative));
      this.CommonStates = (VisualStateGroup) base.FindName("CommonStates");
      this.Normal = (VisualState) base.FindName("Normal");
      this.Selected = (VisualState) base.FindName("Selected");
      this.textBlock = (TextBlock) base.FindName("textBlock");
    }
  }
}
