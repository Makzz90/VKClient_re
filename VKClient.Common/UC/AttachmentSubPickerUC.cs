using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.UC
{
  public class AttachmentSubpickerUC : UserControl
  {
    internal ItemsControl itemsControl;
    private bool _contentLoaded;

    public event AttachmentSubItemSelectedEventHandler ItemSelected;

    public AttachmentSubpickerUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void Item_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      AttachmentPickerItemViewModel dataContext = ((FrameworkElement) sender).DataContext as AttachmentPickerItemViewModel;
      if (dataContext == null)
        return;
      // ISSUE: reference to a compiler-generated field
      AttachmentSubItemSelectedEventHandler itemSelected = this.ItemSelected;
      if (itemSelected == null)
        return;
      AttachmentPickerItemViewModel picketItem = dataContext;
      itemSelected(picketItem);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/AttachmentSubpickerUC.xaml", UriKind.Relative));
      this.itemsControl = (ItemsControl) base.FindName("itemsControl");
    }
  }
}
