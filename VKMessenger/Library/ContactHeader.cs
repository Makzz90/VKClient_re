using Microsoft.Phone.UserData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKMessenger.Library
{
    public class ContactHeader : ViewModelBase, IBinarySerializable
    {
        private BitmapImage _image;
        private string _contactName;
        private string _vKName;
        private User _assignedUser;

        public BitmapImage Image
        {
            get
            {
                return this._image;
            }
            set
            {
                this._image = value;
                this.NotifyPropertyChanged<BitmapImage>((Expression<Func<BitmapImage>>)(() => this.Image));
            }
        }

        public string ContactName
        {
            get
            {
                return this._contactName;
            }
            set
            {
                this._contactName = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.ContactName));
            }
        }

        public string VKName
        {
            get
            {
                return this._vKName;
            }
            set
            {
                this._vKName = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.VKName));
            }
        }

        public long VKUserId { get; set; }

        public Visibility IsOnline
        {
            get
            {
                if (this.AssignedUser != null && this.AssignedUser.online != 0)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public User AssignedUser
        {
            get
            {
                return this._assignedUser;
            }
            set
            {
                this._assignedUser = value;
                this.NotifyPropertyChanged<User>((Expression<Func<User>>)(() => this.AssignedUser));
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.PhotoMedium));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.IsOnline));
            }
        }

        public string PhotoMedium
        {
            get
            {
                if (this.AssignedUser == null)
                    return string.Empty;
                return this.AssignedUser.photo_max;
            }
        }

        public bool IsDefaultImage { get; set; }

        public List<string> ContactPhoneNumbers { get; set; }

        public ContactHeader()
        {
        }

        public ContactHeader(Contact contact)
        {
            ContactHeader contactHeader = this;
            this.ContactPhoneNumbers = new List<string>();
            if (contact.PhoneNumbers != null && contact.PhoneNumbers.Count<ContactPhoneNumber>() > 0)
                this.ContactPhoneNumbers.AddRange(contact.PhoneNumbers.Select<ContactPhoneNumber, string>((Func<ContactPhoneNumber, string>)(n => n.PhoneNumber)));
            this.ContactName = contact.DisplayName;
            if (((DependencyObject)Deployment.Current).Dispatcher.CheckAccess())
                this.SetImage(contact);
            else
                ((DependencyObject)Deployment.Current).Dispatcher.BeginInvoke((Action)(() => contactHeader.SetImage(contact)));
        }

        private void SetImage(Contact contact)
        {
            if (this.VKUserId == 0L && string.IsNullOrEmpty(this.VKName))
            {
                this.Image = new BitmapImage();
                Stream picture = contact.GetPicture();
                if (picture != null)
                {
                    ((BitmapSource)this.Image).SetSource(picture);
                    this.IsDefaultImage = false;
                }
                else
                    this.SetImageToDefaultPlaceholder();
            }
            else
                this.UpdateImageForVK();
        }

        public void UpdateImageForVK()
        {
            if (this.PhotoMedium.EndsWith("gif"))
            {
                this.SetImageToDefaultPlaceholder();
            }
            else
            {
                this.Image = new BitmapImage(new Uri(this.PhotoMedium, UriKind.RelativeOrAbsolute));
                this.IsDefaultImage = false;
            }
        }

        private void SetImageToDefaultPlaceholder()
        {
            StreamResourceInfo resourceStream = Application.GetResourceStream(new Uri("/VKMessenger;component/Resources/Photo_Placeholder.png", UriKind.RelativeOrAbsolute));
            this.Image = new BitmapImage();
            ((BitmapSource)this.Image).SetSource(resourceStream.Stream);
            this.IsDefaultImage = true;
        }

        public bool Matches(IList<string> searchStrings)
        {
            bool flag1 = true;
            foreach (string searchString in (IEnumerable<string>)searchStrings)
            {
                string searchTerm = searchString;
                int num;
                if (!string.IsNullOrEmpty(this.ContactName))
                {
                    if (((IEnumerable<string>)this.ContactName.Split(' ')).Any<string>((Func<string, bool>)(name => name.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        num = 1;
                        goto label_6;
                    }
                }
                num = string.IsNullOrEmpty(this.VKName) ? 0 : (this.VKName.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase) ? 1 : 0);
            label_6:
                bool flag2 = num != 0;
                flag1 &= flag2;
                if (!flag1)
                    break;
            }
            return flag1;
        }

        public bool HasNumber(string phoneNumber)
        {
            return this.ContactPhoneNumbers.Any<string>((Func<string, bool>)(s => string.Compare(Regex.Replace(s, "[^0-9]", ""), Regex.Replace(phoneNumber, "[^0-9]", "")) == 0));
        }

        internal void SetIsOnline(bool p)
        {
            if (this.AssignedUser == null)
                return;
            this.AssignedUser.online = p ? 1 : 0;
            this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.IsOnline));
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((int)this.VKUserId);
            writer.WriteString(this.ContactName);
            writer.WriteString(this.VKName);
            writer.WriteList(this.ContactPhoneNumbers ?? new List<string>());
            writer.Write<User>(this.AssignedUser ?? new User(), false);
            writer.Write(this.IsDefaultImage);
            if (this.IsDefaultImage)
                return;
            byte[] byteArray = ImageUtil.TryImageToByteArray((BitmapSource)this.Image);
            int length = byteArray.Length;
            writer.Write(length);
            if (length <= 0)
                return;
            writer.Write(byteArray);
        }

        public void Read(BinaryReader reader)
        {
            this.VKUserId = (long)reader.ReadInt32();
            this.ContactName = reader.ReadString();
            this.VKName = reader.ReadString();
            this.ContactPhoneNumbers = reader.ReadList();
            this.AssignedUser = reader.ReadGeneric<User>();
            this.IsDefaultImage = reader.ReadBoolean();
            if (!this.IsDefaultImage)
            {
                int count = reader.ReadInt32();
                if (count <= 0)
                    return;
                this.Image = ImageUtil.ByteArrayToImage(reader.ReadBytes(count));
            }
            else
                this.SetImageToDefaultPlaceholder();
        }
    }
}
