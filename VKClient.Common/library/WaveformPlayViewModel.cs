using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class WaveformPlayViewModel : ViewModelBase
  {
    private readonly Waveform _recordedWaveform;
    private int _itemsCount;

    public Waveform Waveform = new Waveform();

    public int ItemsCount
    {
      get
      {
        return this._itemsCount;
      }
      set
      {
        this._itemsCount = value;
        this.GenerateWaveform();
      }
    }

    private void GenerateWaveform()
    {
      this.Waveform.Clear();
      int itemsCount = this._itemsCount;
      for (int index = 0; index < itemsCount; ++index)
        this.Waveform.Add(new WaveformItemViewModel(100));
    }
  }
}
