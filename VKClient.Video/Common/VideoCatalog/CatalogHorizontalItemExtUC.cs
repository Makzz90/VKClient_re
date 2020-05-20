using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library.VirtItems;
using VKClient.Video.Library;

namespace VKClient.Common.VideoCatalog
{
    public partial class CatalogHorizontalItemExtUC : UserControlVirtualizable
    {
        private CatalogItemViewModel VM
        {
            get
            {
                return this.DataContext as CatalogItemViewModel;
            }
        }

        private AlbumHeader AlbumHeaderVM
        {
            get
            {
                return this.DataContext as AlbumHeader;
            }
        }

        public CatalogHorizontalItemExtUC()
        {
            this.InitializeComponent();
        }

        private void LayoutRoot_Tap(object sender, GestureEventArgs e)
        {
            if (this.VM != null)
            {
                this.VM.HandleTap();
            }
            else
            {
                if (this.AlbumHeaderVM == null)
                    return;
                this.AlbumHeaderVM.HandleTap();
            }
        }

    }
}
