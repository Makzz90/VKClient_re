using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;

namespace VKClient.Audio.ViewModels
{
    public class AudioAlbumHeader : ViewModelBase
    {
        private ObservableCollection<MenuItemData> _menuItems = new ObservableCollection<MenuItemData>();
        private AudioAlbum _album;

        public AudioAlbum Album
        {
            get
            {
                return this._album;
            }
        }

        public ObservableCollection<MenuItemData> MenuItems
        {
            get
            {
                return this._menuItems;
            }
        }

        public bool IsMenuEnabled
        {
            get
            {
                return ((Collection<MenuItemData>)this._menuItems).Count > 0;
            }
        }

        public string Title
        {
            get
            {
                return this._album.title;
            }
            set
            {
                this._album.title = value;
                this.NotifyPropertyChanged<string>(() => this.Title);
            }
        }

        public AudioAlbumHeader(AudioAlbum album)
        {
            this._album = album;
            this.CreateMenuItems();
        }

        private void CreateMenuItems()
        {
            if (this._album.album_id == AllAlbumsViewModel.RECOMMENDED_ALBUM_ID || this._album.album_id == AllAlbumsViewModel.POPULAR_ALBUM_ID || this._album.owner_id != AppGlobalStateManager.Current.LoggedInUserId)
                return;
            ((Collection<MenuItemData>)this._menuItems).Add(new MenuItemData()
            {
                Title = CommonResources.Edit,
                Tag = "edit"
            });
            ((Collection<MenuItemData>)this._menuItems).Add(new MenuItemData()
            {
                Title = CommonResources.Delete,
                Tag = "delete"
            });
        }
    }
}
