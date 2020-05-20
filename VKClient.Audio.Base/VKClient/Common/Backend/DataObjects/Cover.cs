using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Common.Utils;

namespace VKClient.Common.Backend.DataObjects
{
  public class Cover
  {
    private List<CoverImage> _images;

    public int enabled { get; set; }

    public List<CoverImage> images
    {
      get
      {
        return this._images;
      }
      set
      {
        this._images = value;
        this.CurrentImage = this.GetImage(480.0);
      }
    }

    public CoverImage CurrentImage { get; private set; }

    private CoverImage GetImage(double width)
    {
      if (this.images == null || this.images.Count == 0)
        return  null;
      width *= (double) ScaleFactor.GetRealScaleFactor() / 100.0;
      foreach (CoverImage coverImage in (IEnumerable<CoverImage>) this.images.OrderBy<CoverImage, int>((Func<CoverImage, int>) (i => i.width)))
      {
        if ((double) coverImage.width >= width)
          return coverImage;
      }
      return this.images.LastOrDefault<CoverImage>();
    }
  }
}
