using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Utils;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Gifts.ViewModels
{
  public class GiftHeader
  {
    private static readonly ThemeHelper _themeHelper = new ThemeHelper();
    private static readonly CultureName _currentCulture = CultureHelper.GetCurrent();

    public bool IsCurrentUser { get; private set; }

    public long Id { get; private set; }

    public long FromId { get; private set; }

    public string GiftHash { get; private set; }

    public string Name { get; private set; }

    public string Photo { get; private set; }

    public string Date { get; private set; }

    public string GiftImage { get; private set; }

    public string Message { get; private set; }

    public string FirstName { get; private set; }

    public string FirstNameGen { get; private set; }

    public string UsersGiftsStr { get; private set; }

    public double NameTilt { get; private set; }

    public bool CanSeeGifts { get; private set; }

    public bool IsMoreActionsVisible { get; private set; }

    public Visibility MessageVisiblity { get; private set; }

    public Visibility PrivacyDescriptionVisibility { get; private set; }

    public Visibility MessageOrPrivacyDescriptionVisibility { get; private set; }

    public Visibility SendBackVisibility { get; private set; }

    public Visibility MoreActionsVisibility { get; private set; }

    public Visibility LightThemeVisibility { get; private set; }

    public Visibility DarkThemeVisibility { get; private set; }

    public GiftHeader(GiftItemData giftItem, IProfile profile, bool isCurrentUser)
    {
      this.IsCurrentUser = isCurrentUser;
      this.Id = giftItem.id;
      this.FromId = giftItem.from_id;
      this.GiftHash = giftItem.gift_hash;
      this.Name = (profile != null ? profile.name :  null) ?? CommonResources.AnonymousGift;
      this.Photo = profile != null ? profile.photo_max :  null;
      this.Date = UIStringFormatterHelper.FormatDateTimeForUI(giftItem.date);
      Gift gift = giftItem.gift;
      this.GiftImage = gift != null ? gift.thumb_256 :  null;
      this.Message = giftItem.message;
      this.FirstName = profile != null ? profile.first_name :  null;
      this.FirstNameGen = profile != null ? profile.first_name_gen :  null;
      string str = GiftHeader._currentCulture == CultureName.KZ ? this.FirstName : this.FirstNameGen;
      this.UsersGiftsStr = string.IsNullOrWhiteSpace(str) ? "" : string.Format(CommonResources.UsersGiftsFrm, str);
      this.NameTilt = this.FromId != 0L ? 1.5 : 0.0;
      this.CanSeeGifts = profile != null && profile.can_see_gifts == 1;
      this.IsMoreActionsVisible = this.IsCurrentUser || this.CanSeeGifts;
      this.MessageVisiblity = (!string.IsNullOrEmpty(this.Message)).ToVisiblity();
      this.PrivacyDescriptionVisibility = (this.IsCurrentUser && giftItem.Privacy == GiftPrivacy.VisibleToRecipient).ToVisiblity();
      this.MessageOrPrivacyDescriptionVisibility = (this.MessageVisiblity == System.Windows.Visibility.Visible || this.PrivacyDescriptionVisibility == 0).ToVisiblity();
      this.SendBackVisibility = (this.IsCurrentUser && giftItem.from_id > 0L).ToVisiblity();
      this.MoreActionsVisibility = this.IsMoreActionsVisible.ToVisiblity();
      this.LightThemeVisibility = GiftHeader._themeHelper.PhoneLightThemeVisibility;
      this.DarkThemeVisibility = GiftHeader._themeHelper.PhoneDarkThemeVisibility;
    }
  }
}
