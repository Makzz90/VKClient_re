using SharpDX;
using SharpDX.Direct3D11;
using System;
using Windows.Foundation;

namespace VKClient.Common.Framework.SharpDXExt
{
  public abstract class SharpDXContextBase : IDisposable
  {
    private Device d3DDevice;
    private DeviceContext d3DContext;
    private RenderTargetView backBufferView;
    private DepthStencilView depthStencilView;
    private Texture2D backBuffer;
    private Size backBufferSize;

    public Device D3DDevice
    {
      get
      {
        this.ThrowIfDisposed();
        return this.d3DDevice;
      }
      private set
      {
        this.d3DDevice = value;
      }
    }

    public DeviceContext D3DContext
    {
      get
      {
        this.ThrowIfDisposed();
        return this.d3DContext;
      }
      private set
      {
        this.d3DContext = value;
      }
    }

    public RenderTargetView BackBufferView
    {
      get
      {
        this.ThrowIfDisposed();
        return this.backBufferView;
      }
      internal set
      {
        this.backBufferView = value;
      }
    }

    public Texture2D BackBuffer
    {
      get
      {
        this.ThrowIfDisposed();
        return this.backBuffer;
      }
      internal set
      {
        this.backBuffer = value;
      }
    }

    public Size BackBufferSize
    {
      get
      {
        this.ThrowIfDisposed();
        return this.backBufferSize;
      }
      internal set
      {
        this.backBufferSize = value;
      }
    }

    public bool IsDisposed { get; private set; }

    public bool IsBound { get; protected set; }

    public event EventHandler Render;

    protected SharpDXContextBase()
    {
      this.IsDisposed = false;
      this.IsBound = false;
    }

    protected void ReleaseDeviceDependentResources()
    {
      Utilities.Dispose<Device>(ref this.d3DDevice);
      Utilities.Dispose<DeviceContext>(ref this.d3DContext);
    }

    protected void ReleaseSizeDependentResources()
    {
      Utilities.Dispose<Texture2D>(ref this.backBuffer);
      Utilities.Dispose<RenderTargetView>(ref this.backBufferView);
      Utilities.Dispose<DepthStencilView>(ref this.depthStencilView);
    }

    internal virtual void RecreateBackBuffer(Size backBufferSize)
    {
      Utilities.Dispose<Texture2D>(ref this.backBuffer);
      Utilities.Dispose<RenderTargetView>(ref this.backBufferView);
    }

    internal void OnDeviceReset(Device newDevice, DeviceContext newContext)
    {
      this.D3DDevice = newDevice;
      this.D3DContext = newContext;
    }

    internal void OnRender()
    {
      if (this.Render == null)
        return;
      this.Render((object) this, EventArgs.Empty);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        this.ReleaseDeviceDependentResources();
        this.ReleaseSizeDependentResources();
      }
      this.IsBound = false;
      this.IsDisposed = true;
    }

    protected void ThrowIfDisposed()
    {
      if (this.IsDisposed)
        throw new ObjectDisposedException("The object has already been disposed.");
    }

    protected void ThrowIfBound()
    {
      if (this.IsBound)
        throw new InvalidOperationException("This instance has already been bound to a control.");
    }
  }
}
