using Microsoft.Phone.Shell;
using System;
using System.Linq;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
    public class TileManager : IHandle<CountersChanged>, IHandle
    {
        private static TileManager _instance = new TileManager();
        private object _lockObj = new object();

        public static TileManager Instance
        {
            get
            {
                return TileManager._instance;
            }
        }

        public void Initialize()
        {
            EventAggregator.Current.Subscribe(this);
        }

        public void Handle(CountersChanged message)
        {
            this.UpdateTileWithCount(message.Counters.messages, (Action)(() => { }));
        }

        public void ResetContent()
        {
            IconicTileData data = new IconicTileData();
            this.SetColor(data);
            data.WideContent1 = ("");
            data.WideContent2 = ("");
            data.WideContent3 = ("");
            this.UpdatePrimaryTile(data);
        }

        public void SetContentAndCount(string content1, string content2, string content3, int count, Action callback)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                IconicTileData data = new IconicTileData();
                data.Count = (new int?(count));
                data.WideContent1 = content1;
                data.WideContent2 = content2;
                data.WideContent3 = content3;
                this.SetColor(data);
                this.UpdatePrimaryTile(data);
                callback();
            }));
        }

        private void UpdatePrimaryTile(IconicTileData data)
        {
            try
            {
                ShellTile.ActiveTiles.First<ShellTile>().Update((ShellTileData)data);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Failed to update primary tile", ex);
            }
        }

        public void UpdateTileWithCount(int count, Action callback)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                IconicTileData data = new IconicTileData();
                data.Count = (new int?(count));
                this.SetColor(data);
                this.UpdatePrimaryTile(data);
                callback();
            }));
        }

        public void UpdateTileColor()
        {
            IconicTileData data = new IconicTileData();
            this.SetColor(data);
            this.UpdatePrimaryTile(data);
        }

        private void SetColor(IconicTileData data)
        {
            if (ThemeSettingsManager.GetThemeSettings().TileSettings == 1)
            {
                Color backgroundColor = default(Color);
                backgroundColor.A = (255);
                backgroundColor.R = (62);
                backgroundColor.G = (114);
                backgroundColor.B = (173);
                data.BackgroundColor=(backgroundColor);
            }
        }
    }
}
