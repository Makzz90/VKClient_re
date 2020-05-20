using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using Windows.ApplicationModel;
using Windows.Phone.Management.Deployment;

namespace VKClient.Common.Library.Games
{
    public class InstalledPackagesFinder : IHandle<ApplicationDeactivatedEvent>, IHandle, IHandle<RootFrameNavigatedEvent>
    {
        private static InstalledPackagesFinder _instance;
        private bool _isLoaded;
        private bool _isLoading;
        private List<string> _installedPackageIds;

        public static InstalledPackagesFinder Instance
        {
            get
            {
                if (InstalledPackagesFinder._instance == null)
                    InstalledPackagesFinder._instance = new InstalledPackagesFinder();
                return InstalledPackagesFinder._instance;
            }
        }

        public void Initialize()
        {
            EventAggregator.Current.Subscribe(this);
        }

        public void EnsurePackageInfoIsLoadedAsync()
        {
            if (this._isLoaded || this._isLoading)
                return;
            this._isLoading = true;
            new Task((Action)(() =>
            {
                try
                {
                    IEnumerable<Package> packages = InstallationManager.FindPackages();
                    List<string> stringList = new List<string>();
                    foreach (Package package in packages)
                    {
                        string str = package.Id.ProductId.Replace("{", "").Replace("}", "");
                        stringList.Add(str.ToUpperInvariant());
                    }
                    this._isLoaded = true;
                    this._isLoading = false;
                    this._installedPackageIds = stringList;
                }
                catch (Exception ex)
                {
                    ServiceLocator.Resolve<IAppStateInfo>().ReportException(ex);
                }
            })).Start();
        }

        public bool IsPackageInstalled(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                return false;
            packageId = packageId.ToUpperInvariant();
            if (this._installedPackageIds == null)
                return false;
            return this._installedPackageIds.Contains(packageId);
        }

        public void Handle(RootFrameNavigatedEvent message)
        {
            if (!message.Uri.OriginalString.Contains("Games"))
                return;
            this.EnsurePackageInfoIsLoadedAsync();
        }

        public void Handle(ApplicationDeactivatedEvent message)
        {
            this._isLoaded = false;
        }
    }
}
