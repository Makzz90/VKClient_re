namespace VKClient.Common.Utils
{
  public class Segment
  {
    public int LowerBound { get; private set; }

    public int UpperBound { get; private set; }

    public bool IsEmpty
    {
      get
      {
        return this.UpperBound < this.LowerBound;
      }
    }

    public Segment(int lowerBound, int upperBound)
    {
      this.LowerBound = lowerBound;
      this.UpperBound = upperBound;
    }

    public Segment()
      : this(0, -1)
    {
    }

    public override string ToString()
    {
      if (this.IsEmpty)
        return "[]";
      return string.Format("[{0},{1}]", (object) this.LowerBound, (object) this.UpperBound);
    }

    public void CompareToSegment(Segment otherSegment, out Segment thisMinusOther1, out Segment thisMinusOther2, out Segment intersection, out Segment otherMinusThis1, out Segment otherMinusThis2)
    {
      thisMinusOther1 = new Segment();
      thisMinusOther2 = new Segment();
      intersection = new Segment();
      otherMinusThis1 = new Segment();
      otherMinusThis2 = new Segment();
      if (this.IsEmpty)
        otherMinusThis1 = otherSegment;
      else if (otherSegment.IsEmpty)
        thisMinusOther1 = this;
      else if (this.UpperBound < otherSegment.LowerBound)
      {
        thisMinusOther1 = this;
        otherMinusThis1 = otherSegment;
      }
      else if (this.LowerBound < otherSegment.LowerBound && this.UpperBound >= otherSegment.LowerBound && this.UpperBound <= otherSegment.UpperBound)
      {
        thisMinusOther1 = new Segment(this.LowerBound, otherSegment.LowerBound - 1);
        intersection = new Segment(otherSegment.LowerBound, this.UpperBound);
        otherMinusThis1 = new Segment(this.UpperBound + 1, otherSegment.UpperBound);
      }
      else if (this.LowerBound >= otherSegment.LowerBound && this.UpperBound <= otherSegment.UpperBound)
      {
        intersection = this;
        otherMinusThis1 = new Segment(otherSegment.LowerBound, this.LowerBound - 1);
        otherMinusThis2 = new Segment(this.UpperBound + 1, otherSegment.UpperBound);
      }
      else
        otherSegment.CompareToSegment(this, out otherMinusThis1, out otherMinusThis2, out intersection, out thisMinusOther1, out thisMinusOther2);
    }
  }
}
