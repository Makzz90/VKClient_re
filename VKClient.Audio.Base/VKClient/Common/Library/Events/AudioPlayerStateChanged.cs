using Microsoft.Phone.BackgroundAudio;

namespace VKClient.Common.Library.Events
{
    public class AudioPlayerStateChanged
    {
        public PlayState PlayState { get; private set; }

        public AudioPlayerStateChanged(PlayState state)
        {
            this.PlayState = state;
        }
    }
}
