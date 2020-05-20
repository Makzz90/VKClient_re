using System.Collections.Generic;

namespace VKClient.Photos.ImageEditor
{
  public static class AvailableFilters
  {
    public static List<FilterViewModel> _filters = new List<FilterViewModel>()
    {
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/normal.jpg",
        FilterName = "Normal"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/pro.jpg",
        FilterName = "Pro"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/horus.jpg",
        FilterName = "Horus"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/latona.jpg",
        FilterName = "Latona"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/aurora.jpg",
        FilterName = "Aurora"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/liber.jpg",
        FilterName = "Liber"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/zaria.jpg",
        FilterName = "Zaria"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/vesta.jpg",
        FilterName = "Vesta"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/fortuna.jpg",
        FilterName = "Fortuna"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/nox.jpg",
        FilterName = "Nox"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/veles.jpg",
        FilterName = "Veles"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/minerva.jpg",
        FilterName = "Minerva"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/luna.jpg",
        FilterName = "Luna"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/iris.jpg",
        FilterName = "Iris"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/terra.jpg",
        FilterName = "Terra"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/genius.jpg",
        FilterName = "Genius"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/vesper.jpg",
        FilterName = "Vesper"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/mitra.jpg",
        FilterName = "Mitra"
      },
      new FilterViewModel()
      {
        FilterImage = "/Resources/FilterPreviews/diana.jpg",
        FilterName = "Diana"
      }
    };

    public static List<FilterViewModel> Filters
    {
      get
      {
        return AvailableFilters._filters;
      }
    }
  }
}
