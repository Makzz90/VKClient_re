using System.Collections.Generic;
using VKClient.Common.Framework;

namespace VKClient.Common.Graffiti.ViewModels
{
  public class BrushThicknessPickerViewModel : ViewModelBase
  {
      public IEnumerable<BrushThicknessViewModel> ThicknessItems { get; private set; }

    public BrushThicknessPickerViewModel()
    {
      List<BrushThicknessViewModel> thicknessViewModelList = new List<BrushThicknessViewModel>();
      thicknessViewModelList.Add(new BrushThicknessViewModel(100, 48));
      thicknessViewModelList.Add(new BrushThicknessViewModel(70, 40));
      thicknessViewModelList.Add(new BrushThicknessViewModel(40, 32)
      {
        IsSelected = true
      });
      BrushThicknessViewModel thicknessViewModel1 = new BrushThicknessViewModel(20, 24);
      thicknessViewModelList.Add(thicknessViewModel1);
      BrushThicknessViewModel thicknessViewModel2 = new BrushThicknessViewModel(10, 16);
      thicknessViewModelList.Add(thicknessViewModel2);
      this.ThicknessItems = (IEnumerable<BrushThicknessViewModel>) thicknessViewModelList;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
    }
  }
}
