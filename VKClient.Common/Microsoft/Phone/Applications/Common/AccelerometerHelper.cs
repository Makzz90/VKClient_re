using Microsoft.Devices.Sensors;
using System;

namespace Microsoft.Phone.Applications.Common
{
  public sealed class AccelerometerHelper : IDisposable
  {
    private static object _syncRoot = new object();
    private static double _maximumCalibrationOffset = Math.Sin(Math.PI / 9.0);
    private static double _maximumStabilityDeltaOffset = Math.Sin(Math.PI / 360.0);
    private Simple3DVector[] _sampleBuffer = new Simple3DVector[25];
    private Simple3DVector _sampleSum = new Simple3DVector(0.0, 0.0, -25.0);
    private static volatile AccelerometerHelper _singletonInstance;
    private Accelerometer _sensor;
    private const double MaximumCalibrationTiltAngle = 0.349065850398866;
    private const double MaximumStabilityTiltDeltaAngle = 0.00872664625997165;
    private int _deviceStableCount;
    private const int SamplesCount = 25;
    private const double LowPassFilterCoef = 0.1;
    private const double NoiseMaxAmplitude = 0.05;
    private bool _initialized;
    private Simple3DVector _previousLowPassOutput;
    private Simple3DVector _previousOptimalFilterOutput;
    private int _sampleIndex;
    private Simple3DVector _averageAcceleration;
    private const string AccelerometerCalibrationKeyName = "AccelerometerCalibration";
    private bool _active;

    public static AccelerometerHelper Instance
    {
      get
      {
        if (AccelerometerHelper._singletonInstance == null)
        {
          lock (AccelerometerHelper._syncRoot)
          {
            if (AccelerometerHelper._singletonInstance == null)
              AccelerometerHelper._singletonInstance = new AccelerometerHelper();
          }
        }
        return AccelerometerHelper._singletonInstance;
      }
    }

    public bool IsDeviceStable
    {
      get
      {
        return this._deviceStableCount >= 25;
      }
    }

    private static Simple3DVector AccelerometerCalibrationPersisted
    {
      get
      {
        return new Simple3DVector(ApplicationSettingHelper.TryGetValueWithDefault<double>("AccelerometerCalibrationX", 0.0), ApplicationSettingHelper.TryGetValueWithDefault<double>("AccelerometerCalibrationY", 0.0), 0.0);
      }
      set
      {
        if (!(ApplicationSettingHelper.AddOrUpdateValue("AccelerometerCalibrationX", value.X) | ApplicationSettingHelper.AddOrUpdateValue("AccelerometerCalibrationY", value.Y)))
          return;
        ApplicationSettingHelper.Save();
      }
    }

    public Simple3DVector ZeroAccelerationCalibrationOffset { get; private set; }

    public bool NoAccelerometer { get; private set; }

    public bool IsActive
    {
      get
      {
        return this._active;
      }
      set
      {
        if (this.NoAccelerometer)
          return;
        if (value)
        {
          if (this._active)
            return;
          this.StartAccelerometer();
        }
        else
        {
          if (!this._active)
            return;
          this.StopAccelerometer();
        }
      }
    }

    public event EventHandler<AccelerometerHelperReadingEventArgs> ReadingChanged;

    private AccelerometerHelper()
    {
      this._sensor = new Accelerometer();
      this.NoAccelerometer = this._sensor == null || this._sensor.State == 0;
      this._sensor =  null;
      this._sampleIndex = 0;
      this.ZeroAccelerationCalibrationOffset = AccelerometerHelper.AccelerometerCalibrationPersisted;
    }

    public void Dispose()
    {
      if (this._sensor == null)
        return;
      this._sensor.Dispose();
    }

    public bool CanCalibrate(bool xAxis, bool yAxis)
    {
      bool flag = false;
      lock (this._sampleBuffer)
      {
        if (this.IsDeviceStable)
        {
          double d = 0.0;
          if (xAxis)
            d += this._averageAcceleration.X * this._averageAcceleration.X;
          if (yAxis)
            d += this._averageAcceleration.Y * this._averageAcceleration.Y;
          if (Math.Sqrt(d) <= AccelerometerHelper._maximumCalibrationOffset)
            flag = true;
        }
      }
      return flag;
    }

    public bool Calibrate(bool xAxis, bool yAxis)
    {
      bool flag = false;
      lock (this._sampleBuffer)
      {
        if (this.CanCalibrate(xAxis, yAxis))
        {
          this.ZeroAccelerationCalibrationOffset = new Simple3DVector(xAxis ? -this._averageAcceleration.X : this.ZeroAccelerationCalibrationOffset.X, yAxis ? -this._averageAcceleration.Y : this.ZeroAccelerationCalibrationOffset.Y, 0.0);
          AccelerometerHelper.AccelerometerCalibrationPersisted = this.ZeroAccelerationCalibrationOffset;
          flag = true;
        }
      }
      return flag;
    }

    private void StartAccelerometer()
    {
      try
      {
        this._sensor = new Accelerometer();
        if (this._sensor != null)
        {
          this._sensor.ReadingChanged += (new EventHandler<AccelerometerReadingEventArgs>(this.sensor_ReadingChanged));
          this._sensor.Start();
          this._active = true;
          this.NoAccelerometer = false;
        }
        else
        {
          this._active = false;
          this.NoAccelerometer = true;
        }
      }
      catch (Exception )
      {
        this._active = false;
        this.NoAccelerometer = true;
      }
    }

    private void StopAccelerometer()
    {
      try
      {
        if (this._sensor == null)
          return;
        this._sensor.ReadingChanged -= (new EventHandler<AccelerometerReadingEventArgs>(this.sensor_ReadingChanged));
        this._sensor.Stop();
        this._sensor =  null;
        this._active = false;
        this._initialized = false;
      }
      catch (Exception )
      {
        this._active = false;
        this.NoAccelerometer = true;
      }
    }

    private static double LowPassFilter(double newInputValue, double priorOutputValue)
    {
      return priorOutputValue + 0.1 * (newInputValue - priorOutputValue);
    }

    private static double FastLowAmplitudeNoiseFilter(double newInputValue, double priorOutputValue)
    {
      double num = newInputValue;
      if (Math.Abs(newInputValue - priorOutputValue) <= 0.05)
        num = priorOutputValue + 0.1 * (newInputValue - priorOutputValue);
      return num;
    }

    private void sensor_ReadingChanged(object sender, AccelerometerReadingEventArgs e)
    {
      Simple3DVector simple3Dvector1 = new Simple3DVector(e.X, e.Y, e.Z);
      Simple3DVector simple3Dvector2;
      Simple3DVector simple3Dvector3;
      Simple3DVector simple3Dvector4;
      lock (this._sampleBuffer)
      {
        if (!this._initialized)
        {
          this._sampleSum = simple3Dvector1 * 25.0;
          this._averageAcceleration = simple3Dvector1;
          for (int index = 0; index < 25; ++index)
            this._sampleBuffer[index] = this._averageAcceleration;
          this._previousLowPassOutput = this._averageAcceleration;
          this._previousOptimalFilterOutput = this._averageAcceleration;
          this._initialized = true;
        }
        simple3Dvector2 = new Simple3DVector(AccelerometerHelper.LowPassFilter(simple3Dvector1.X, this._previousLowPassOutput.X), AccelerometerHelper.LowPassFilter(simple3Dvector1.Y, this._previousLowPassOutput.Y), AccelerometerHelper.LowPassFilter(simple3Dvector1.Z, this._previousLowPassOutput.Z));
        this._previousLowPassOutput = simple3Dvector2;
        simple3Dvector3 = new Simple3DVector(AccelerometerHelper.FastLowAmplitudeNoiseFilter(simple3Dvector1.X, this._previousOptimalFilterOutput.X), AccelerometerHelper.FastLowAmplitudeNoiseFilter(simple3Dvector1.Y, this._previousOptimalFilterOutput.Y), AccelerometerHelper.FastLowAmplitudeNoiseFilter(simple3Dvector1.Z, this._previousOptimalFilterOutput.Z));
        this._previousOptimalFilterOutput = simple3Dvector3;
        this._sampleIndex = this._sampleIndex + 1;
        if (this._sampleIndex >= 25)
          this._sampleIndex = 0;
        Simple3DVector simple3Dvector5 = simple3Dvector3;
        this._sampleSum = this._sampleSum + simple3Dvector5;
        this._sampleSum = this._sampleSum - this._sampleBuffer[this._sampleIndex];
        this._sampleBuffer[this._sampleIndex] = simple3Dvector5;
        simple3Dvector4 = this._sampleSum / 25.0;
        this._averageAcceleration = simple3Dvector4;
        Simple3DVector simple3Dvector6 = simple3Dvector4 - simple3Dvector3;
        if (Math.Abs(simple3Dvector6.X) > AccelerometerHelper._maximumStabilityDeltaOffset || Math.Abs(simple3Dvector6.Y) > AccelerometerHelper._maximumStabilityDeltaOffset || Math.Abs(simple3Dvector6.Z) > AccelerometerHelper._maximumStabilityDeltaOffset)
          this._deviceStableCount = 0;
        else if (this._deviceStableCount < 25)
          this._deviceStableCount = this._deviceStableCount + 1;
        simple3Dvector1 += this.ZeroAccelerationCalibrationOffset;
        simple3Dvector2 += this.ZeroAccelerationCalibrationOffset;
        simple3Dvector3 += this.ZeroAccelerationCalibrationOffset;
        simple3Dvector4 += this.ZeroAccelerationCalibrationOffset;
      }
      // ISSUE: reference to a compiler-generated field
      if (this.ReadingChanged == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.ReadingChanged(this, new AccelerometerHelperReadingEventArgs()
      {
        RawAcceleration = simple3Dvector1,
        LowPassFilteredAcceleration = simple3Dvector2,
        OptimallyFilteredAcceleration = simple3Dvector3,
        AverageAcceleration = simple3Dvector4
      });
    }
  }
}
