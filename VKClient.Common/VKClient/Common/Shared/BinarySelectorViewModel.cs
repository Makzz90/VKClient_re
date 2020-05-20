using System.ComponentModel;
using VKClient.Common.Framework;

namespace VKClient.Common.Shared
{
  public class BinarySelectorViewModel : ViewModelBase
  {
    private SelectorOption _option1;
    private SelectorOption _option2;

    public SelectorOption Option1
    {
      get
      {
        return this._option1;
      }
    }

    public SelectorOption Option2
    {
      get
      {
        return this._option2;
      }
    }

    public BinarySelectorViewModel(SelectorOption option1, SelectorOption option2)
    {
      this._option1 = option1;
      this._option1.PropertyChanged += new PropertyChangedEventHandler(this._option_PropertyChanged);
      this._option2 = option2;
      this._option2.PropertyChanged += new PropertyChangedEventHandler(this._option_PropertyChanged);
    }

    private void _option_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "IsSelected"))
        return;
      SelectorOption selectorOption = sender as SelectorOption;
      if (!selectorOption.IsSelected)
        return;
      if (this.Option1 != selectorOption)
        this.Option1.IsSelected = false;
      if (this.Option2 == selectorOption)
        return;
      this.Option2.IsSelected = false;
    }
  }
}
