using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
    public class MyVirtualizingStackPanel : StackPanel
    {
        public MyVirtualizingStackPanel()
        {
            base.SizeChanged += (delegate(object sender, SizeChangedEventArgs args)
            {
                this.FindMyVirtPanel(this);
            });
        }

        private void FindMyVirtPanel(UIElement parentElement)
        {
            MyVirtualizingPanel2 panel1 = parentElement as MyVirtualizingPanel2;
            if (panel1 != null)
            {
                this.PrintMyVirtPanel(panel1);
            }
            else
            {
                Panel panel2 = parentElement as Panel;
                if (panel2 == null)
                    return;
                using (IEnumerator<UIElement> enumerator = ((PresentationFrameworkCollection<UIElement>)panel2.Children).GetEnumerator())
                {
                    while (((IEnumerator)enumerator).MoveNext())
                    {
                        UIElement current = enumerator.Current;
                        if ((current).GetType() == typeof(MyVirtualizingPanel2))
                            this.PrintMyVirtPanel(current as MyVirtualizingPanel2);
                        else
                            this.FindMyVirtPanel(current);
                    }
                }
            }
        }

        private void PrintMyVirtPanel(MyVirtualizingPanel2 panel)
        {
            double y = panel.TransformToVisual(this).Transform(new Point(0.0, 0.0)).Y;
            panel.OffsetY = y;
        }
    }
}
