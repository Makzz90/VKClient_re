namespace VKClient.Localization
{
    using System;
    
    
    /// <summary>
    ///   Класс ресурса со строгой типизацией для поиска локализованных строк и т.д.
    /// </summary>
    // Этот класс создан автоматически классом StronglyTypedResourceBuilder
    // с помощью такого средства, как ResGen или Visual Studio.
    // Чтобы добавить или удалить член, измените файл .ResX и снова запустите ResGen
    // с параметром /str или перестройте свой проект VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class AppResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AppResources() {
        }
        
        /// <summary>
        ///   Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("VKClient.Resources.AppResources", typeof(AppResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Перезаписывает свойство CurrentUICulture текущего потока для всех
        ///   обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Designed by Michael Lihachyov.
        /// </summary>
        public static string About_DesignedByMichael {
            get {
                return ResourceManager.GetString("About_DesignedByMichael", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Developed by Alexander Sychev.
        /// </summary>
        public static string About_DevelopedByMe {
            get {
                return ResourceManager.GetString("About_DevelopedByMe", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Please, check internet availability and correctness of phone datetime settings..
        /// </summary>
        public static string FailedToConnect {
            get {
                return ResourceManager.GetString("FailedToConnect", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Unable to login.
        /// </summary>
        public static string Login_Error_Header {
            get {
                return ResourceManager.GetString("Login_Error_Header", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Login failed. Please check that user id and password are correct..
        /// </summary>
        public static string Login_Error_InvalidCredential {
            get {
                return ResourceManager.GetString("Login_Error_InvalidCredential", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Phone number or e-mail.
        /// </summary>
        public static string Login_Id {
            get {
                return ResourceManager.GetString("Login_Id", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на .
        /// </summary>
        public static string Login_Info_LoggingIn {
            get {
                return ResourceManager.GetString("Login_Info_LoggingIn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на log in.
        /// </summary>
        public static string Login_LogIn {
            get {
                return ResourceManager.GetString("Login_LogIn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Password.
        /// </summary>
        public static string Login_Password {
            get {
                return ResourceManager.GetString("Login_Password", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на you&apos;ll .
        /// </summary>
        public static string Login_SignUp_Line1 {
            get {
                return ResourceManager.GetString("Login_SignUp_Line1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на sign up.
        /// </summary>
        public static string Login_SignUp_Title {
            get {
                return ResourceManager.GetString("Login_SignUp_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на messages.
        /// </summary>
        public static string MainPage_Conversations {
            get {
                return ResourceManager.GetString("MainPage_Conversations", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на menu.
        /// </summary>
        public static string MainPage_Menu {
            get {
                return ResourceManager.GetString("MainPage_Menu", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на settings.
        /// </summary>
        public static string MainPage_Menu_AppBar_Settings {
            get {
                return ResourceManager.GetString("MainPage_Menu_AppBar_Settings", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на audio.
        /// </summary>
        public static string MainPage_Menu_Audio {
            get {
                return ResourceManager.GetString("MainPage_Menu_Audio", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на favorites.
        /// </summary>
        public static string MainPage_Menu_Favorites {
            get {
                return ResourceManager.GetString("MainPage_Menu_Favorites", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на feedback.
        /// </summary>
        public static string MainPage_Menu_Feedback {
            get {
                return ResourceManager.GetString("MainPage_Menu_Feedback", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на friends.
        /// </summary>
        public static string MainPage_Menu_Friends {
            get {
                return ResourceManager.GetString("MainPage_Menu_Friends", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на groups.
        /// </summary>
        public static string MainPage_Menu_Groups {
            get {
                return ResourceManager.GetString("MainPage_Menu_Groups", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на messages.
        /// </summary>
        public static string MainPage_Menu_Messages {
            get {
                return ResourceManager.GetString("MainPage_Menu_Messages", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на news.
        /// </summary>
        public static string MainPage_Menu_News {
            get {
                return ResourceManager.GetString("MainPage_Menu_News", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на photos.
        /// </summary>
        public static string MainPage_Menu_Photos {
            get {
                return ResourceManager.GetString("MainPage_Menu_Photos", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на video.
        /// </summary>
        public static string MainPage_Menu_Video {
            get {
                return ResourceManager.GetString("MainPage_Menu_Video", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на news.
        /// </summary>
        public static string MainPage_News {
            get {
                return ResourceManager.GetString("MainPage_News", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на write a post.
        /// </summary>
        public static string MainPage_News_AddNews {
            get {
                return ResourceManager.GetString("MainPage_News_AddNews", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на add photo.
        /// </summary>
        public static string MainPage_News_AddPhoto {
            get {
                return ResourceManager.GetString("MainPage_News_AddPhoto", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на refresh.
        /// </summary>
        public static string MainPage_News_Refresh {
            get {
                return ResourceManager.GetString("MainPage_News_Refresh", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на search.
        /// </summary>
        public static string MainPage_News_Search {
            get {
                return ResourceManager.GetString("MainPage_News_Search", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на New theme settings will be applied after application restart..
        /// </summary>
        public static string Settings_AppliedAfterRestart {
            get {
                return ResourceManager.GetString("Settings_AppliedAfterRestart", resourceCulture);
            }
        }
    }
}
