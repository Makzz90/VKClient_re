namespace Microsoft.Phone.Applications.Common
{
  public class DeviceOrientationInfo
  {
    public double AngleOnXYPlan { get; set; }

    public int HorizontalAxisPolarity { get; set; }

    public Simple3DVector NormalGravityVector { get; set; }

    public DeviceOrientationInfo(double angle, int horizontalScreenAxisPolarity, Simple3DVector typicalGravityVector)
    {
      this.AngleOnXYPlan = angle;
      this.HorizontalAxisPolarity = horizontalScreenAxisPolarity;
      this.NormalGravityVector = typicalGravityVector;
    }
  }
}
