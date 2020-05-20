using System.Collections.Generic;
using VKClient.Common.Framework;

namespace VKClient.Common.Graffiti.ViewModels
{
    public abstract class BrushColorPickerViewModel : ViewModelBase
    {
        public IEnumerable<ColorViewModel> Colors { get; set; }

        protected BrushColorPickerViewModel()
        {
            List<ColorViewModel> colorViewModelList = new List<ColorViewModel>();
            colorViewModelList.Add(new ColorViewModel("#ffe64646")
            {
                IsSelected = true
            });
            ColorViewModel colorViewModel1 = new ColorViewModel("#ffff9300");
            colorViewModelList.Add(colorViewModel1);
            ColorViewModel colorViewModel2 = new ColorViewModel("#ffffcb00");
            colorViewModelList.Add(colorViewModel2);
            ColorViewModel colorViewModel3 = new ColorViewModel("#ff62da37");
            colorViewModelList.Add(colorViewModel3);
            ColorViewModel colorViewModel4 = new ColorViewModel("#ff00aef9");
            colorViewModelList.Add(colorViewModel4);
            ColorViewModel colorViewModel5 = new ColorViewModel("#ffcc74e1");
            colorViewModelList.Add(colorViewModel5);
            ColorViewModel colorViewModel6 = new ColorViewModel("#ff000000");
            colorViewModelList.Add(colorViewModel6);
            colorViewModelList.Add(new ColorViewModel("#ffffffff")
            {
                HasExtraStroke = true
            });
            this.Colors = (IEnumerable<ColorViewModel>)colorViewModelList;
        }
    }
}
