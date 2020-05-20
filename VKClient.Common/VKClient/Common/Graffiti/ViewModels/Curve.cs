using System.Collections.Generic;
using System.Windows.Shapes;

namespace VKClient.Common.Graffiti.ViewModels
{
  public class Curve : List<Path>
  {
    public Curve()
    {
    }

    public Curve(IEnumerable<Path> curvePaths)
    {
      this.AddRange(curvePaths);
    }
  }
}
