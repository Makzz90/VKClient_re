using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using Windows.Foundation;

namespace VKClient.Common.Framework.SharpDXExt
{
  internal class DrawingSurfaceContentProvider : DrawingSurfaceContentProviderNativeBase
  {
    private SharpDXContext sharpDXContext;
    private readonly object lockObject;
    private DrawingSurfaceRuntimeHost runtimeHost;
    private DrawingSurfaceSynchronizedTexture synchronizedTexture;

    public DrawingSurfaceContentProvider(SharpDXContext context, object lockObject)
    {
      this.sharpDXContext = context;
      this.lockObject = lockObject;
      using (Device device = new Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new FeatureLevel[5]
      {
        FeatureLevel.Level_11_1,
        FeatureLevel.Level_11_0,
        FeatureLevel.Level_10_1,
        FeatureLevel.Level_10_0,
        FeatureLevel.Level_9_3
      }))
      {
        Device newDevice = (Device) device.QueryInterface<Device1>();
        DeviceContext newContext = (DeviceContext) newDevice.ImmediateContext.QueryInterface<DeviceContext1>();
        this.sharpDXContext.OnDeviceReset(newDevice, newContext);
      }
    }

    public override void Connect(DrawingSurfaceRuntimeHost host)
    {
      this.runtimeHost = host;
    }

    public override void Disconnect()
    {
      this.runtimeHost.Dispose();
      this.runtimeHost = (DrawingSurfaceRuntimeHost) null;
      Utilities.Dispose<DrawingSurfaceSynchronizedTexture>(ref this.synchronizedTexture);
      if (this.sharpDXContext == null || !this.sharpDXContext.IsDisposed)
        return;
      this.sharpDXContext = (SharpDXContext) null;
    }

    public override void PrepareResources(DateTime presentTargetTime, out Bool isContentDirty)
    {
      isContentDirty = (Bool) true;
    }

    public override void GetTexture(Size2F surfaceSize, out DrawingSurfaceSynchronizedTexture synchronizedTexture, out RectangleF textureSubRectangle)
    {
      lock (this.lockObject)
      {
        if (this.sharpDXContext == null || this.sharpDXContext.IsDisposed)
        {
          synchronizedTexture = this.synchronizedTexture;
          textureSubRectangle = new RectangleF(0.0f, 0.0f, surfaceSize.Width, surfaceSize.Height);
        }
        else
        {
          if (this.synchronizedTexture == null)
          {
            this.sharpDXContext.BackBufferSize = new Size((double) surfaceSize.Width, (double) surfaceSize.Height);
            this.sharpDXContext.RecreateBackBuffer(this.sharpDXContext.BackBufferSize);
            this.sharpDXContext.D3DContext.Rasterizer.SetViewport(new ViewportF(0.0f, 0.0f, surfaceSize.Width, surfaceSize.Height));
            this.synchronizedTexture = this.runtimeHost.CreateSynchronizedTexture(this.sharpDXContext.BackBuffer);
          }
          synchronizedTexture = this.synchronizedTexture;
          textureSubRectangle = new RectangleF(0.0f, 0.0f, surfaceSize.Width, surfaceSize.Height);
          this.synchronizedTexture.BeginDraw();
          this.sharpDXContext.OnRender();
          this.synchronizedTexture.EndDraw();
          this.runtimeHost.RequestAdditionalFrame();
        }
      }
    }
  }
}
