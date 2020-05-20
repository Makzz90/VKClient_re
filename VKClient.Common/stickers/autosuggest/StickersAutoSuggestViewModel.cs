using System.Collections.Generic;
using System.Collections.ObjectModel;
using VKClient.Common.Framework;

namespace VKClient.Common.Stickers.AutoSuggest
{
    public class StickersAutoSuggestViewModel : ViewModelBase
    {
        public ObservableCollection<StickersAutoSuggestItem> AutoSuggestCollection { get; private set; }

        public string Keyword { get; set; }

        public void SetItems(IEnumerable<StickersAutoSuggestItem> items)
        {
            this.AutoSuggestCollection.Clear();
            foreach (StickersAutoSuggestItem stickersAutoSuggestItem in items)
                this.AutoSuggestCollection.Add(stickersAutoSuggestItem);
        }

        public StickersAutoSuggestViewModel()
        {
            this.AutoSuggestCollection = new ObservableCollection<StickersAutoSuggestItem>();
        }
    }
}
