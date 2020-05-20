using Microsoft.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Windows;
using System.Windows.Resources;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKMessenger.Library
{
  public class DeviceManager
  {
    private static SoundEffect soundEffect;

    public static void Vibrate()
    {
      VibrateController.Default.Start(TimeSpan.FromMilliseconds(100.0));
    }

    public static void PlaySound()
    {
      if (!AppGlobalStateManager.Current.GlobalState.SoundEnabled)
        return;
      DeviceManager.LoadSound("Resources/New/chat_sound.wav", out DeviceManager.soundEffect);
      if (DeviceManager.soundEffect == null)
        return;
      FrameworkDispatcher.Update();
      DeviceManager.soundEffect.Play();
    }

    private static void LoadSound(string SoundFilePath, out SoundEffect Sound)
    {
      Sound =  null;
      try
      {
        StreamResourceInfo resourceStream = Application.GetResourceStream(new Uri(SoundFilePath, UriKind.Relative));
        Sound = SoundEffect.FromStream(resourceStream.Stream);
      }
      catch (NullReferenceException ex)
      {
        Logger.Instance.Error("Failed to load sound", (Exception) ex);
      }
    }
  }
}
