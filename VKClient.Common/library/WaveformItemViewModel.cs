using System;

namespace VKClient.Common.Library
{
  public class WaveformItemViewModel
  {
    private const int MIN_HEIGHT = 3;
    private const int MAX_HEIGHT = 36;

    public int Percentage { get; private set; }

    public int Height { get; private set; }

    public WaveformItemViewModel(int percentage)
    {
      this.Percentage = percentage;
      int num = Math.Max(3, Math.Min(36, (int) ((double) (36 * this.Percentage) / 100.0)));
      this.Height = num % 2 == 0 ? num : num + 1;
    }
  }
}
