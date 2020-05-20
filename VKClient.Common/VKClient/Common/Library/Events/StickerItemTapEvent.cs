using VKClient.Common.Emoji;

namespace VKClient.Common.Library.Events
{
    public class StickerItemTapEvent
    {
        public string Referrer { get; set; }

        public StickerItemData StickerItem { get; set; }

        public StickerItemTapEvent()
        {
            this.Referrer = "keyboard";
        }
    }
}
