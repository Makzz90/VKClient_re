using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
//
using System.Windows.Shapes;

namespace VKClient.Common.UC
{
    public class MenuItemUC : UserControl
    {
        private bool _contentLoaded;
        //
        public Rectangle MenuCounters;

        public event EventHandler<System.Windows.Input.GestureEventArgs> ItemTap;
        public event EventHandler<System.Windows.Input.GestureEventArgs> ItemHold;
        public event EventHandler<System.Windows.Input.GestureEventArgs> CounterTap;
        public event EventHandler<System.Windows.Input.GestureEventArgs> CounterHold;

        public MenuItemUC()
        {
            //base.\u002Ector();
            this.InitializeComponent();
            //
            double px_per_tick = this.MenuCounters.Height / 10.0 / 2.0;
            this.MenuCounters.RadiusX = this.MenuCounters.RadiusY = VKClient.Common.Library.AppGlobalStateManager.Current.GlobalState.NotifyRadius * px_per_tick;
        }

        private void MenuItem_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // ISSUE: reference to a compiler-generated field
            EventHandler<System.Windows.Input.GestureEventArgs> itemTap = this.ItemTap;
            if (itemTap == null)
                return;
            itemTap(sender, e);
        }

        private void MenuItem_OnHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // ISSUE: reference to a compiler-generated field
            EventHandler<System.Windows.Input.GestureEventArgs> itemHold = this.ItemHold;
            if (itemHold == null)
                return;
            itemHold(sender, e);
        }

        private void Counter_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // ISSUE: reference to a compiler-generated field
            if (this.CounterTap != null)
            {
                // ISSUE: reference to a compiler-generated field
                this.CounterTap(sender, e);
            }
            else
            {
                // ISSUE: reference to a compiler-generated field
                EventHandler<System.Windows.Input.GestureEventArgs> itemTap = this.ItemTap;
                if (itemTap == null)
                    return;
                itemTap(sender, e);
            }
        }

        private void Counter_OnHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // ISSUE: reference to a compiler-generated field
            if (this.CounterHold != null)
            {
                // ISSUE: reference to a compiler-generated field
                this.CounterHold(sender, e);
            }
            else
            {
                // ISSUE: reference to a compiler-generated field
                EventHandler<System.Windows.Input.GestureEventArgs> itemHold = this.ItemHold;
                if (itemHold == null)
                    return;
                object sender1 = sender;
                GestureEventArgs e1 = e;
                itemHold(sender1, e1);
            }
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/MenuItemUC.xaml", UriKind.Relative));
            //
            this.MenuCounters = (Rectangle)base.FindName("MenuCounters");
        }
    }
}
