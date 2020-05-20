namespace VKClient.Localization
{
    public class LocalizedStrings
    {
        private static AppResources _localizedResources = new AppResources();

        public AppResources LocalizedResources
        {
            get
            {
                return LocalizedStrings._localizedResources;
            }
        }
    }
}
