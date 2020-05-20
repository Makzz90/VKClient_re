using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace XamlAnimatedGif
{
  internal class TimingManager
  {
    private readonly List<TimeSpan> _timeSpans = new List<TimeSpan>();
    private readonly Task _completedTask = (Task) Task.FromResult<int>(0);
    private readonly RepeatBehavior _repeatBehavior;
    private int _current;
    private int _count;
    private bool _isComplete;
    private TimeSpan _elapsed;
    private TaskCompletionSource<int> _pauseCompletionSource;

    public bool IsComplete
    {
      get
      {
        return this._isComplete;
      }
      private set
      {
        this._isComplete = value;
        if (!value)
          return;
        this.OnCompleted();
      }
    }

    public bool IsPaused { get; private set; }

    public event EventHandler Completed;

    public TimingManager(RepeatBehavior repeatBehavior)
    {
      this._repeatBehavior = repeatBehavior;
    }

    public void Add(TimeSpan timeSpan)
    {
      this._timeSpans.Add(timeSpan);
    }

    public async Task<bool> NextAsync(CancellationToken cancellationToken)
    {
      if (this.IsComplete)
        return false;
      await this.IsPausedAsync(cancellationToken);
      TimeSpan ts = this._timeSpans[this._current];
      await Task.Delay(ts, cancellationToken);
      this._current = this._current + 1;
      RepeatBehavior repeatBehavior = this._repeatBehavior;
      // ISSUE: explicit reference operation
      if (((RepeatBehavior) @repeatBehavior).HasDuration)
      {
        this._elapsed = this._elapsed + ts;
        TimeSpan elapsed = this._elapsed;
        repeatBehavior = this._repeatBehavior;
        // ISSUE: explicit reference operation
        TimeSpan duration = ((RepeatBehavior) @repeatBehavior).Duration;
        if (elapsed >= duration)
        {
          this.IsComplete = true;
          return false;
        }
      }
      if (this._current < this._timeSpans.Count)
        return true;
      repeatBehavior = this._repeatBehavior;
      // ISSUE: explicit reference operation
      if (((RepeatBehavior) @repeatBehavior).HasCount)
      {
        int num1 = this._count + 1;
        this._count = num1;
        double num2 = (double) num1;
        repeatBehavior = this._repeatBehavior;
        // ISSUE: explicit reference operation
        double count = ((RepeatBehavior) @repeatBehavior).Count;
        if (num2 < count)
        {
          this._current = 0;
          return true;
        }
        this.IsComplete = true;
        return false;
      }
      this._current = 0;
      return true;
    }

    public void Reset()
    {
      this._current = 0;
      this._count = 0;
      this._elapsed = TimeSpan.Zero;
      this.IsComplete = false;
    }

    protected virtual void OnCompleted()
    {
      // ISSUE: reference to a compiler-generated field
      EventHandler completed = this.Completed;
      if (completed == null)
        return;
      EventArgs empty = EventArgs.Empty;
      completed(this, empty);
    }

    public void Pause()
    {
      this.IsPaused = true;
      this._pauseCompletionSource = new TaskCompletionSource<int>();
    }

    public void Resume()
    {
      TaskCompletionSource<int> completionSource = this._pauseCompletionSource;
      if (completionSource != null)
      {
        int result = 0;
        completionSource.TrySetResult(result);
      }
      this._pauseCompletionSource =  null;
      this.IsPaused = false;
    }

    private Task IsPausedAsync(CancellationToken cancellationToken)
    {
      TaskCompletionSource<int> tcs = this._pauseCompletionSource;
      if (tcs == null)
        return this._completedTask;
      if (cancellationToken.CanBeCanceled)
        cancellationToken.Register((Action) (() => tcs.TrySetCanceled()));
      return (Task) tcs.Task;
    }
  }
}
