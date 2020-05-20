using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class WaveformRecordViewModel : ViewModelBase
  {
    public Waveform Waveform = new Waveform();

    public Waveform CompleteWaveform = new Waveform();

    private void AddWaveformItem(int value)
    {
      this.Waveform.Add(new WaveformItemViewModel(value));
      this.CompleteWaveform.Add(new WaveformItemViewModel(value));
    }

    public void Add(int value)
    {
      this.AddWaveformItem(value);
    }
  }
}
