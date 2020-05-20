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
    private string _parentPostId;
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

    public AudioAttachmentsItem(double width, Thickness margin, List<AudioObj> audioList, bool isFromComment = false, bool isMessage = false, double horizontalWidth = 0.0, bool isHorizontal = false, string parentPostId = null)
        : base(width, margin, new Thickness())
    {
      this._audioList = audioList;
      this._isFromComment = isFromComment;
      this._isMessage = isMessage;
      this._horizontalWidth = horizontalWidth;
      this._verticalWidth = width;
      this._isHorizontal = isHorizontal;
      this._parentPostId = parentPostId;
      if (this._isHorizontal)
        this.Width = this._horizontalWidth;
      this.CreateControls();
    }

    private List<AudioAttachmentUC> CreateControls()
    {
        return (List<AudioAttachmentUC>)Enumerable.ToList<AudioAttachmentUC>(Enumerable.Select<AudioObj, AudioAttachmentUC>(this._audioList, (Func<AudioObj, AudioAttachmentUC>)(audio =>
      {
        AudioAttachmentUC audioAttachmentUc = new AudioAttachmentUC();
        double width = this.Width;
        ((FrameworkElement) audioAttachmentUc).Width = width;
        Attachment attachment = new Attachment();
        attachment.type = "audio";
        attachment.audio = audio;
        string parentPostId = this._parentPostId;
        AttachmentViewModel attachmentViewModel = new AttachmentViewModel(attachment, parentPostId);
        ((FrameworkElement) audioAttachmentUc).DataContext = attachmentViewModel;
        Action<AudioAttachmentUC> action = new Action<AudioAttachmentUC>(this.OnPlayerStartedPlaying);
        audioAttachmentUc.NotifyStartedPlayingCallback = action;
        return audioAttachmentUc;
      })));
    }

    private void OnPlayerStartedPlaying(AudioAttachmentUC sender)
    {
      PlaylistManager.SetAudioAgentPlaylist(this._audioList, this._isFromComment ? StatisticsActionSource.comments : CurrentMediaSource.AudioSource);
    }

    protected override void GenerateChildren()
    {
      List<AudioAttachmentUC> controls = this.CreateControls();
      double num1 = 0.0;
      double num2 = 0.0;
      List<AudioAttachmentUC>.Enumerator enumerator = controls.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          AudioAttachmentUC current = enumerator.Current;
          ((FrameworkElement) current).Margin=(new Thickness(num2, num1, 0.0, num2));
          this.Children.Add((FrameworkElement) current);
          num1 += 72.0;
        }
      }
      finally
      {
        enumerator.Dispose();
      }
    }
  }
}
