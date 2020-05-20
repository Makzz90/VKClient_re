using System.Windows;

namespace VKClient.Common.Framework
{
  public interface IVirtualizable
  {
    VirtualizableState CurrentState { get; }

    double FixedHeight { get; }

    FrameworkElement View { get; }

    Thickness Margin { get; set; }

    Thickness ViewMargin { get; set; }

    IMyVirtualizingPanel Parent { get; set; }

    void ChangeState(VirtualizableState newState);

    void IsOnScreen();
  }
}
