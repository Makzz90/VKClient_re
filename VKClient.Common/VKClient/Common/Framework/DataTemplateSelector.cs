using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Framework
{
  public abstract class DataTemplateSelector : ContentControl
  {
    public DataTemplateSelector()
    {
      this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
    }

    public virtual DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      return (DataTemplate) null;
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
      base.OnContentChanged(oldContent, newContent);
      this.ContentTemplate = this.SelectTemplate(newContent, (DependencyObject) this);
    }
  }
}
