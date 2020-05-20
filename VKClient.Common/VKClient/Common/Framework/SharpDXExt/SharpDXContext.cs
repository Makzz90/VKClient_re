using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Windows.Controls;
using Windows.Foundation;

namespace VKClient.Common.Framework.SharpDXExt
{
  public class SharpDXContext : SharpDXContextBase
  {
    private readonly object lockObject;

    public SharpDXContext(object lockObject)
    {
      this.lockObject = lockObject;
    }

    public void BindToControl(DrawingSurface drawingSurface)
    {
      this.ThrowIfDisposed();
      this.ThrowIfBound();
      DrawingSurfaceContentProvider surfaceContentProvider = new DrawingSurfaceContentProvider(this, this.lockObject);
      drawingSurface.SetContentProvider((object) surfaceContentProvider);
      this.IsBound = true;
    }

    internal override void RecreateBackBuffer(Size backBufferSize)
    {
      base.RecreateBackBuffer(backBufferSize);
      this.BackBuffer = new Texture2D(this.D3DDevice, new Texture2DDescription()
      {
        Format = Format.B8G8R8A8_UNorm,
        Width = (int) backBufferSize.Width,
        Height = (int) backBufferSize.Height,
        ArraySize = 1,
        MipLevels = 1,
        BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
        Usage = ResourceUsage.Default,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.SharedKeyedmutex | ResourceOptionFlags.SharedNthandle,
        SampleDescription = new SampleDescription(1, 0)
      });
      this.BackBufferView = new RenderTargetView(this.D3DDevice, (SharpDX.Direct3D11.Resource) this.BackBuffer);
    }
  }
}
