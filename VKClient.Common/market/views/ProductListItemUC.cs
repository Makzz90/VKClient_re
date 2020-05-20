using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Market.Views
{
  public class ProductListItemUC : UserControl
  {
      public static readonly DependencyProperty ProductProperty = DependencyProperty.Register("Product", typeof(Product), typeof(ProductListItemUC), new PropertyMetadata(new PropertyChangedCallback(ProductListItemUC.Product_OnChanged)));
    private long _ownerId;
    private long _productId;
    internal Image image;
    internal TextBlock textBlockTitle;
    internal TextBlock textBlockSubtitle;
    private bool _contentLoaded;

    public Product Product
    {
      get
      {
        return (Product) base.GetValue(ProductListItemUC.ProductProperty);
      }
      set
      {
        base.SetValue(ProductListItemUC.ProductProperty, value);
      }
    }

    public ProductListItemUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.textBlockTitle.Text = ("");
      this.textBlockSubtitle.Text = ("");
    }

    private static void Product_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((ProductListItemUC) d).UpdateData(e.NewValue as Product);
    }

    private void UpdateData(Product product)
    {
      if (product == null)
        return;
      this._ownerId = product.owner_id;
      this._productId = product.id;
      this.UpdateTitle(product.title);
      this.UpdateSubtitle(product.price);
      this.UpdateImage((IReadOnlyList<Photo>) product.photos);
    }

    private void UpdateTitle(string newTitle)
    {
      this.textBlockTitle.Text = newTitle;
    }

    private void UpdateSubtitle(Price price)
    {
      string str = "";
      if (price != null && !string.IsNullOrEmpty(price.text))
        str = price.text;
      this.textBlockSubtitle.Text = str;
    }

    private void UpdateImage(IReadOnlyList<Photo> photos)
    {
      if (photos != null && photos.Count > 0)
        ImageLoader.SetUriSource(this.image, photos[0].GetAppropriateForScaleFactor(((FrameworkElement) this.image).Height, 1));
      else
        ImageLoader.SetUriSource(this.image, "");
    }

    private void Product_OnTap(object sender, GestureEventArgs args)
    {
      if (this._ownerId == 0L || this._productId == 0L)
        return;
      Navigator.Current.NavigateToProduct(this._ownerId, this._productId);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Market/Views/ProductListItemUC.xaml", UriKind.Relative));
      this.image = (Image) base.FindName("image");
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
      this.textBlockSubtitle = (TextBlock) base.FindName("textBlockSubtitle");
    }
  }
}
