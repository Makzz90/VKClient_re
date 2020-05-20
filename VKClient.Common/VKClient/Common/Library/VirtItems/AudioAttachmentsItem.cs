using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKMessenger.Library;
using VKMessenger.Views;

namespace VKClient.Common.Library.VirtItems
{
  public class AudioAttachmentsItem : VirtualizableItemBase
  {
    private readonly List<AudioObj> _audioList;
    private const double ITEM_HEIGHT = 72.0;
    private const double MARGIN_LEFT_RIGHT = 16.0;
    private readonly bool _isFromComment;
    private readonly bool _isMessage;
    private bool _isHorizontal;
    private readonly double _horizontalWidth;
    private readonly double _verticalWidth;

    public bool IsHorizontal
    {
      get
      {
        return this._isHorizontal;
      }
      set
      {
        if (this._isHorizontal == value)
          return;
        this._isHorizontal = value;
        this.Width = this._isHorizontal ? this._horizontalWidth : this._verticalWidth;
        this.UpdateLayout();
      }
    }

    public override double FixedHeight
    {
      get
      {
        return 72.0 * (double) this._audioList.Count;
      }
    }

    public AudioAttachmentsItem(double width, Thickness margin, List<AudioObj> audioList, bool isFromComment = false, bool isMessage = false, double horizontalWidth = 0.0, bool isHorizontal = false)
      : base(width, margin, new Thickness())
    {
      this._audioList = audioList;
      this._isFromComment = isFromComment;
      this._isMessage = isMessage;
      this._horizontalWidth = horizontalWidth;
      this._verticalWidth = width;
      this._isHorizontal = isHorizontal;
      if (this._isHorizontal)
        this.Width = this._horizontalWidth;
      this.CreateControls();
    }

    private List<AudioAttachmentUC> CreateControls()
    {
      return this._audioList.Select<AudioObj, AudioAttachmentUC>((Func<AudioObj, AudioAttachmentUC>) (audio =>
      {
        return new AudioAttachmentUC()
        {
          Width = this.Width,
          DataContext = (object) new AttachmentViewModel(new Attachment()
          {
            type = "audio",
            audio = audio
          }),
          NotifyStartedPlayingCallback = new Action<AudioAttachmentUC>(this.OnPlayerStartedPlaying)
        };
      })).ToList<AudioAttachmentUC>();
    }

    private void OnPlayerStartedPlaying(AudioAttachmentUC sender)
    {
      PlaylistManager.SetAudioAgentPlaylist(this._audioList, this._isFromComment ? StatisticsActionSource.comments : CurrentMediaSource.AudioSource);
    }

    protected override void GenerateChildren()
    {
      List<AudioAttachmentUC> controls = this.CreateControls();
      double top = 0.0;
      double num = 0.0;
      foreach (AudioAttachmentUC audioAttachmentUc in controls)
      {
        audioAttachmentUc.Margin = new Thickness(num, top, 0.0, num);
        this.Children.Add((FrameworkElement) audioAttachmentUc);
        top += 72.0;
      }
    }
  }
}
