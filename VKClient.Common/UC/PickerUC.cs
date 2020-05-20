using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using Windows.Storage.Pickers;

namespace VKClient.Common.UC
{
  public class PickerUC : UserControl, INotifyPropertyChanged
  {
    private ObservableCollection<PickableItem> _items;
    private bool _handleAsAttTypes;
    private Action<PickableItem> _itemSelectedCallback;
    private Action _mainButtonTapCallback;
    private Action<PickableItem> _toolButtonTapCallback;
    private Action _beforeNavigateCallback;
    private Action _showPhotoPicker;
    private PickableItem _itemToSelect;
    private string _title;
    private DialogService _ds;
    internal Grid LayoutRoot;
    internal TextBlock ApplicationTitle;
    internal ExtendedLongListSelector listPicker;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this._title))
          return this._title;
        return CommonResources.CHOOSE;
      }
    }

    public ObservableCollection<PickableItem> Items
    {
      get
      {
        return this._items;
      }
      set
      {
        this._items = value;
        if (this.PropertyChanged == null)
          return;
        this.PropertyChanged(this, new PropertyChangedEventArgs("Items"));
      }
    }

    public Visibility MainToolImageVisibility
    {
      get
      {
        if (this._mainButtonTapCallback == null)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public bool IsShown
    {
      get
      {
        if (this._ds != null)
          return this._ds.IsOpen;
        return false;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public PickerUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.PickerUC_Loaded));
    }

    private void _items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      this.EnsureItemIsSelected();
    }

    public static PickerUC PickAttachmentTypeAndNavigate(List<NamedAttachmentType> attTypes, Action beforeNavigateCallback = null, Action showPhotoPicker = null)
    {
      PickerUC pickerUc = new PickerUC();
      pickerUc._handleAsAttTypes = true;
      ObservableCollection<PickableItem> observableCollection = new ObservableCollection<PickableItem>((IEnumerable<PickableItem>)Enumerable.Select<NamedAttachmentType, PickableItem>(attTypes, (Func<NamedAttachmentType, PickableItem>)(a => new PickableItem()
      {
        ID = (long) a.AttachmentType,
        Name = a.Name
      })));
      pickerUc.Items = observableCollection;
      Action action1 = beforeNavigateCallback;
      pickerUc._beforeNavigateCallback = action1;
      Action action2 = showPhotoPicker;
      pickerUc._showPhotoPicker = action2;
      pickerUc.ShowPopup();
      return pickerUc;
    }

    public static void ShowPickerFor(ObservableCollection<PickableItem> items, PickableItem selectedItem, Action<PickableItem> itemSelectedCallback, Action<PickableItem> toolButtonTapCallback = null, Action mainToolButtonTapCallback = null, string title = null)
    {
      new PickerUC()
      {
        _itemSelectedCallback = itemSelectedCallback,
        _toolButtonTapCallback = toolButtonTapCallback,
        _mainButtonTapCallback = mainToolButtonTapCallback,
        Items = items,
        _itemToSelect = selectedItem,
        _title = title
      }.ShowPopup();
    }

    public static void ShowPickerForReportReasons(Action<ReportReason> choosenReasonCallback)
    {
      PickerUC.ShowPickerFor(ReportContentHelper.GetPredefinedReportReasons(),  null,  (pi => choosenReasonCallback((ReportReason) pi.ID)),  null,  null, "");
    }

    private void PickerUC_Loaded(object sender, RoutedEventArgs e)
    {
      this.EnsureItemIsSelected();
      this._items.CollectionChanged += new NotifyCollectionChangedEventHandler(this._items_CollectionChanged);
    }

    private void EnsureItemIsSelected()
    {
      if (this._itemToSelect == null)
        return;
      this.listPicker.SelectedItem = (Enumerable.FirstOrDefault<PickableItem>(this.Items, (Func<PickableItem, bool>)(it => it.ID == this._itemToSelect.ID)));
    }

    public void ShowPopup()
    {
      base.DataContext = this;
      this._ds = new DialogService();
      this._ds.Child = (FrameworkElement) this;
      this._ds.HideOnNavigation = true;
      this._ds.KeepAppBar = false;
      this._ds.Show( null);
      ((Control) this).Focus();
    }

    private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!(sender is FrameworkElement))
        return;
      object dataContext = (sender as FrameworkElement).DataContext;
      if (!(dataContext is PickableItem))
        return;
      this.HandleItemSelection(dataContext as PickableItem);
    }

    private void HandleItemSelection(PickableItem pickableItem)
    {
      if (this._handleAsAttTypes)
      {
        if (this._beforeNavigateCallback != null)
          this._beforeNavigateCallback();
        PickableItem pickableItem1 = pickableItem;
        long id = pickableItem1.ID;
        if (pickableItem1.ID == 1L)
          Navigator.Current.NavigateToPhotoAlbums(true, 0, false, 0);
        if (id == 2L)
          Navigator.Current.NavigateToVideo(true, 0, false, false);
        if (id == 3L)
          Navigator.Current.NavigateToAudio(1, 0, false, 0, 0, "");
        if (id == 4L)
          Navigator.Current.NavigateToDocumentsPicker(1);
        if (id == 7L)
          Navigator.Current.NavigateToMap(true, 0.0, 0.0);
        if (id == 8L && this._showPhotoPicker != null)
        {
          this._ds.Hide();
          this._showPhotoPicker();
        }
        if (id != 9L)
          return;
        this._ds.Hide();
        FileOpenPicker fileOpenPicker = new FileOpenPicker();
        List<string>.Enumerator enumerator = VKConstants.SupportedVideoExtensions.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            string current = enumerator.Current;
            fileOpenPicker.FileTypeFilter.Add(current);
          }
        }
        finally
        {
          enumerator.Dispose();
        }
        ((IDictionary<string, object>) fileOpenPicker.ContinuationData)["Operation"] = "VideoFromPhone";
        fileOpenPicker.PickSingleFileAndContinue();
      }
      else
      {
        this._ds.Hide();
        this._itemSelectedCallback(pickableItem);
      }
    }

    private void MainToolButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this._mainButtonTapCallback == null)
        return;
      this._mainButtonTapCallback();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/PickerUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ApplicationTitle = (TextBlock) base.FindName("ApplicationTitle");
      this.listPicker = (ExtendedLongListSelector) base.FindName("listPicker");
    }
  }
}
