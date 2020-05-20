using Microsoft.Phone.BackgroundAudio;
using System;
using VKClient.Audio.Base;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.Events
{
    public class AudioEventTranslator
    {
        private static AudioTrack _previousTrack;
        private static PlayState _previousPlayState;
        private static bool _havePreviousState;

        public static void Initialize()
        {
            BGAudioPlayerWrapper.Instance.PlayStateChanged += new EventHandler(AudioEventTranslator.Instance_PlayStateChanged);
        }

        private static void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            AudioTrack track = BGAudioPlayerWrapper.Instance.Track;
            PlayState playerState = BGAudioPlayerWrapper.Instance.PlayerState;
            if (AudioEventTranslator._havePreviousState && AudioEventTranslator._previousPlayState == playerState && !AudioEventTranslator.AreDifferentTracks(track, AudioEventTranslator._previousTrack))
                return;
            AudioEventTranslator._havePreviousState = true;
            AudioEventTranslator._previousTrack = track;
            AudioEventTranslator._previousPlayState = playerState;
            EventAggregator.Current.Publish(new AudioPlayerStateChanged(playerState));
        }

        private static bool AreDifferentTracks(AudioTrack track, AudioTrack previousTrack)
        {
            return (track != null || previousTrack != null) && (track != null && previousTrack == null || track == null && previousTrack != null || !(track.GetTagId() == previousTrack.GetTagId()));
        }
    }
}
