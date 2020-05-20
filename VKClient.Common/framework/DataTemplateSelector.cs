using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Framework
{
  public abstract class DataTemplateSelector : ContentControl
  {
    public DataTemplateSelector()
    {
      //base.\u002Ector();
      ((Control) this).HorizontalContentAlignment=((HorizontalAlignment) 3);
    }

    public virtual DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      return  null;
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
      base.OnContentChanged(oldContent, newContent);
      this.ContentTemplate=(this.SelectTemplate(newContent, (DependencyObject) this));
    }
  }
}
