using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace VKClient.Common.Library
{
  public class Waveform : ObservableCollection<WaveformItemViewModel>
  {
    private int? _maxItemsCount;

    public int? MaxItemsCount
    {
      set
      {
        this._maxItemsCount = value;
        this.RemoveExcessItems();
      }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
      base.OnCollectionChanged(e);
      if (e.Action != NotifyCollectionChangedAction.Add)
        return;
      this.RemoveExcessItems();
    }

    private void RemoveExcessItems()
    {
      if (!this._maxItemsCount.HasValue)
        return;
      while (this.Count > this._maxItemsCount.Value)
        this.RemoveAt(0);
    }
  }
}
