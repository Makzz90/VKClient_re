namespace VKClient.Common.Utils
{
  public class ThumbAttachment
  {
    public double Width { get; set; }

    public double Height { get; set; }

    public double CalcWidth { get; set; }

    public double CalcHeight { get; set; }

    public bool LastColumn { get; set; }

    public bool LastRow { get; set; }

    internal double getRatio()
    {
      return this.Width / this.Height;
    }

    internal void SetViewSize(double width, double height, bool lastColumn, bool lastRow)
    {
      this.CalcWidth = width;
      this.CalcHeight = height;
      this.LastColumn = lastColumn;
      this.LastRow = lastRow;
    }
  }
}
