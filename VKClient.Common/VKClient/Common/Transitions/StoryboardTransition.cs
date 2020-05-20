using Microsoft.Phone.Controls;
using System;
using System.Windows.Media.Animation;

namespace VKClient.Common.Transitions
{
  public abstract class StoryboardTransition : ITransition
  {
    private readonly Storyboard _storyboard;

    public event EventHandler Completed
    {
      add
      {
        this._storyboard.Completed += value;
      }
      remove
      {
        this._storyboard.Completed -= value;
      }
    }

    public StoryboardTransition()
    {
      this._storyboard = this.CreateStoryboard();
    }

    protected abstract Storyboard CreateStoryboard();

    public ClockState GetCurrentState()
    {
      return this._storyboard.GetCurrentState();
    }

    public TimeSpan GetCurrentTime()
    {
      return this._storyboard.GetCurrentTime();
    }

    public void Pause()
    {
      this._storyboard.Pause();
    }

    public void Resume()
    {
      this._storyboard.Resume();
    }

    public void Seek(TimeSpan offset)
    {
      this._storyboard.Seek(offset);
    }

    public void SeekAlignedToLastTick(TimeSpan offset)
    {
      this._storyboard.SeekAlignedToLastTick(offset);
    }

    public void SkipToFill()
    {
      this._storyboard.SkipToFill();
    }

    public void Begin()
    {
      this._storyboard.Begin();
    }

    public void Stop()
    {
      this._storyboard.Stop();
    }
  }
}
