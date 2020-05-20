using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.VideoCatalog
{
    public partial class CategoryMoreFooterUC : UserControlVirtualizable
    {

        private CategoryMoreFooter VM
        {
            get
            {
                return this.DataContext as CategoryMoreFooter;
            }
        }

        public CategoryMoreFooterUC()
        {
            this.InitializeComponent();
        }

        private void OnTapped(object sender, GestureEventArgs e)
        {
            this.VM.HandleTap();
        }

    }
}
