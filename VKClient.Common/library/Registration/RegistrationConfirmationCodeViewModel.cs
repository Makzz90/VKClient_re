using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.Registration
{
  public class RegistrationConfirmationCodeViewModel : ViewModelBase, ICompleteable, IBinarySerializable
  {
    private static readonly int _waitingTimeBeforeSecondAttempt = 60;
    private int _currentStep = 1;
    private Action<Action<bool>> _requestVoiceCallAction;
    private string _confirmationCode;
    private string _phoneNumber;
    private string _phonePrefix;
    private DispatcherTimer _localTimer;
    private DateTime _createdDateTime;
    private bool _isRequestingCall;

    public Action<Action<bool>> RequestVoiceCallAction
    {
      get
      {
        return this._requestVoiceCallAction;
      }
      set
      {
        this._requestVoiceCallAction = value;
      }
    }

    private int CurrentStep
    {
      get
      {
        return this._currentStep;
      }
      set
      {
        this._currentStep = value;
        base.NotifyPropertyChanged<Visibility>(() => this.FirstAttemptVisibility);
        base.NotifyPropertyChanged<Visibility>(() => this.SecondAttemptVisibility);
        base.NotifyPropertyChanged<Visibility>(() => this.ThirdAttemptVisibility);
      }
    }

    public Visibility FirstAttemptVisibility
    {
      get
      {
        if (this._currentStep != 1)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility SecondAttemptVisibility
    {
      get
      {
        if (this._currentStep != 2)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility ThirdAttemptVisibility
    {
      get
      {
        if (this._currentStep != 3)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public int TotalSecondsFromCreatedTime
    {
      get
      {
        return (int) (DateTime.Now- this._createdDateTime).TotalSeconds;
      }
    }

    public string CountdownStr
    {
      get
      {
        return TimeSpan.FromSeconds((double) Math.Max(0, RegistrationConfirmationCodeViewModel._waitingTimeBeforeSecondAttempt - this.TotalSecondsFromCreatedTime)).ToString("mm\\:ss");
      }
    }

    public string ConfirmationCode
    {
      get
      {
        return this._confirmationCode;
      }
      set
      {
        this._confirmationCode = value;
        base.NotifyPropertyChanged<string>(() => this.ConfirmationCode);
        base.NotifyPropertyChanged<bool>(() => this.IsCompleted);
      }
    }

    public string PhoneNumberFormatted
    {
      get
      {
        return string.Concat("+", this._phonePrefix, this._phoneNumber);
      }
    }

    public bool IsCompleted
    {
      get
      {
        return !string.IsNullOrWhiteSpace(this.ConfirmationCode);
      }
    }

    public RegistrationConfirmationCodeViewModel()
    {
      this.InitTimer();
    }

    public RegistrationConfirmationCodeViewModel(string phonePrefix, string phoneNumber, Action<Action<bool>> requestVoiceCallAction)
    {
      this._phonePrefix = phonePrefix;
      this._phoneNumber = phoneNumber;
      this._requestVoiceCallAction = requestVoiceCallAction;
      this._createdDateTime = DateTime.Now;
      this.InitTimer();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this._confirmationCode);
      writer.WriteString(this._phonePrefix);
      writer.WriteString(this._phoneNumber);
      BinarySerializerExtensions.Write(writer, this._createdDateTime);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._confirmationCode = reader.ReadString();
      this._phonePrefix = reader.ReadString();
      this._phoneNumber = reader.ReadString();
      this._createdDateTime = BinarySerializerExtensions.ReadDateTime(reader);
    }

    private void InitTimer()
    {
      this._localTimer = new DispatcherTimer();
      this._localTimer.Interval=(TimeSpan.FromSeconds(0.5));
      this._localTimer.Tick+=(new EventHandler(this._localTimer_Tick));
      this._localTimer.Start();
    }

    public void RequestCall()
    {
        if (this._isRequestingCall)
        {
            return;
        }
        this._isRequestingCall = true;
        this._requestVoiceCallAction.Invoke(delegate(bool res)
        {
            Execute.ExecuteOnUIThread(delegate
            {
                this._isRequestingCall = false;
                if (res)
                {
                    this.CurrentStep = 3;
                }
            });
        });
    }


    private void _localTimer_Tick(object sender, EventArgs e)
    {
        base.NotifyPropertyChanged<string>(() => this.CountdownStr);
        if (this.TotalSecondsFromCreatedTime >= RegistrationConfirmationCodeViewModel._waitingTimeBeforeSecondAttempt)
        {
            this._localTimer.Stop();
            this.CurrentStep = 2;
        }
    }
  }
}
