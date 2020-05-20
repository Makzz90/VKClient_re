using System.Collections.Generic;
using VKClient.Audio.Base.Library;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend.DataObjects;
using VKMessenger.Library;
using VKMessenger.Views;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class AudioMediaListItemViewModel : MediaListItemViewModelBase
  {
    private readonly AudioObj _audio;
    private readonly List<AudioObj> _audios;
    private readonly AttachmentViewModel _audioViewModel;

    public AttachmentViewModel AudioViewModel
    {
      get
      {
        return this._audioViewModel;
      }
    }

    public override string Id
    {
      get
      {
        return this._audio.ToString();
      }
    }

    public AudioMediaListItemViewModel(AudioObj audio, List<AudioObj> audios)
      : base(ProfileMediaListItemType.Audio)
    {
        this._audio = audio;
        this._audios = audios;
        this._audioViewModel = new AttachmentViewModel(new Attachment
        {
            type = "audio",
            audio = audio
        }/*, null*/);
    }

    private void OnPlayerStartedPlaying(AudioAttachmentUC control)
    {
      PlaylistManager.SetAudioAgentPlaylist(this._audios, CurrentMediaSource.AudioSource);
    }
  }
}
