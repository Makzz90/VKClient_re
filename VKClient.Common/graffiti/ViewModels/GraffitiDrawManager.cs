using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.Graffiti.ViewModels
{
  public class GraffitiDrawManager
  {
    private List<Point> _currentPoints = new List<Point>();
    private Brush _strokeBrush;
    private double _strokeThickness;

    public void HandlePoint(Point point, bool isLastPoint = false)
    {
    }

    public void SetStrokeBrush(Brush strokeBrush)
    {
      this._strokeBrush = strokeBrush;
    }

    public void SetStrokeThickness(double strokeThickness)
    {
      this._strokeThickness = strokeThickness;
    }

    public void Undo()
    {
    }

    public void UndoAll()
    {
    }

    public void Redo()
    {
    }
  }
}
