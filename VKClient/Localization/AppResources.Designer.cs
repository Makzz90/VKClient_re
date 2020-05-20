using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace VKClient.Localization
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  public class AppResources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static ResourceManager ResourceManager
    {
      get
      {
        if (AppResources.resourceMan == null)
          AppResources.resourceMan = new ResourceManager("VKClient.Localization.AppResources", typeof (AppResources).Assembly);
        return AppResources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static CultureInfo Culture
    {
      get
      {
        return AppResources.resourceCulture;
      }
      set
      {
        AppResources.resourceCulture = value;
      }
    }

    public static string About_DesignedByMichael
    {
      get
      {
        return AppResources.ResourceManager.GetString("About_DesignedByMichael", AppResources.resourceCulture);
      }
    }

    public static string About_DevelopedByMe
    {
      get
      {
        return AppResources.ResourceManager.GetString("About_DevelopedByMe", AppResources.resourceCulture);
      }
    }

    public static string FailedToConnect
    {
      get
      {
        return AppResources.ResourceManager.GetString("FailedToConnect", AppResources.resourceCulture);
      }
    }

    public static string Login_Error_Header
    {
      get
      {
        return AppResources.ResourceManager.GetString("Login_Error_Header", AppResources.resourceCulture);
      }
    }

    public static string Login_Error_InvalidCredential
    {
      get
      {
        return AppResources.ResourceManager.GetString("Login_Error_InvalidCredential", AppResources.resourceCulture);
      }
    }

    public static string Login_Id
    {
      get
      {
        return AppResources.ResourceManager.GetString("Login_Id", AppResources.resourceCulture);
      }
    }

    public static string Login_Info_LoggingIn
    {
      get
      {
        return AppResources.ResourceManager.GetString("Login_Info_LoggingIn", AppResources.resourceCulture);
      }
    }

    public static string Login_LogIn
    {
      get
      {
        return AppResources.ResourceManager.GetString("Login_LogIn", AppResources.resourceCulture);
      }
    }

    public static string Login_Password
    {
      get
      {
        return AppResources.ResourceManager.GetString("Login_Password", AppResources.resourceCulture);
      }
    }

    public static string Login_SignUp_Line1
    {
      get
      {
        return AppResources.ResourceManager.GetString("Login_SignUp_Line1", AppResources.resourceCulture);
      }
    }

    public static string Login_SignUp_Title
    {
      get
      {
        return AppResources.ResourceManager.GetString("Login_SignUp_Title", AppResources.resourceCulture);
      }
    }

    public static string MainPage_Conversations
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_Conversations", AppResources.resourceCulture);
      }
    }

    public static string MainPage_Menu
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_Menu", AppResources.resourceCulture);
      }
    }

    public static string MainPage_Menu_AppBar_Settings
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_Menu_AppBar_Settings", AppResources.resourceCulture);
      }
    }

    public static string MainPage_Menu_Audio
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_Menu_Audio", AppResources.resourceCulture);
      }
    }

    public static string MainPage_Menu_Favorites
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_Menu_Favorites", AppResources.resourceCulture);
      }
    }

    public static string MainPage_Menu_Feedback
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_Menu_Feedback", AppResources.resourceCulture);
      }
    }

    public static string MainPage_Menu_Friends
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_Menu_Friends", AppResources.resourceCulture);
      }
    }

    public static string MainPage_Menu_Groups
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_Menu_Groups", AppResources.resourceCulture);
      }
    }

    public static string MainPage_Menu_Messages
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_Menu_Messages", AppResources.resourceCulture);
      }
    }

    public static string MainPage_Menu_News
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_Menu_News", AppResources.resourceCulture);
      }
    }

    public static string MainPage_Menu_Photos
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_Menu_Photos", AppResources.resourceCulture);
      }
    }

    public static string MainPage_Menu_Video
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_Menu_Video", AppResources.resourceCulture);
      }
    }

    public static string MainPage_News
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_News", AppResources.resourceCulture);
      }
    }

    public static string MainPage_News_AddNews
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_News_AddNews", AppResources.resourceCulture);
      }
    }

    public static string MainPage_News_AddPhoto
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_News_AddPhoto", AppResources.resourceCulture);
      }
    }

    public static string MainPage_News_Refresh
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_News_Refresh", AppResources.resourceCulture);
      }
    }

    public static string MainPage_News_Search
    {
      get
      {
        return AppResources.ResourceManager.GetString("MainPage_News_Search", AppResources.resourceCulture);
      }
    }

    public static string Settings_AppliedAfterRestart
    {
      get
      {
        return AppResources.ResourceManager.GetString("Settings_AppliedAfterRestart", AppResources.resourceCulture);
      }
    }

    internal AppResources()
    {
    }
  }
}
