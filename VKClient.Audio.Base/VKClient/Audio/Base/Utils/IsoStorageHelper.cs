using System;
using System.IO.IsolatedStorage;
using VKClient.Common.Utils;

namespace VKClient.Audio.Base.Utils
{
  public class IsoStorageHelper
  {
    public static bool TryMoveFile(string fromPath, string toFolder, string toName)
    {
      bool flag = true;
      try
      {
        using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
        {
          if (!storeForApplication.DirectoryExists(toFolder))
            storeForApplication.CreateDirectory(toFolder);
          string str = toFolder + "\\" + toName;
          if (storeForApplication.FileExists(str))
            storeForApplication.DeleteFile(str);
          storeForApplication.MoveFile(fromPath, str);
        }
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("FAILED IsoStorageHelper.TryMoveFile", ex);
        flag = false;
      }
      return flag;
    }
  }
}
