using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class RelationshipItem : ProfileInfoItem
  {
    public RelationshipItem(UserData profileData)
      : base(ProfileInfoItemType.RichText)
    {
      this.Title = CommonResources.ProfilePage_Info_RelStatus;
      string str1 = "";
      User relationPartner = profileData.user.relation_partner;
      long num = relationPartner == null ? 0L : relationPartner.id;
      string str2 = "";
      RelationshipStatus relation = (RelationshipStatus) profileData.user.relation;
      NameCases partnerNameCases = profileData.relPartnerNameCases;
      bool flag = profileData.user.sex == 2;
      switch (relation)
      {
        case RelationshipStatus.NotMarried:
          str1 = flag ? CommonResources.ProfilePage_Info_NotMarriedMale : CommonResources.ProfilePage_Info_NotMarriedFemale;
          break;
        case RelationshipStatus.InARelationship:
          if (num > 0L)
          {
            str1 = flag ? CommonResources.ProfilePage_Info_InARelationshipMaleWith : CommonResources.ProfilePage_Info_InARelationshipFemaleWith;
            str2 = partnerNameCases.ins;
            break;
          }
          str1 = flag ? CommonResources.ProfilePage_Info_InARelationshipMale : CommonResources.ProfilePage_Info_InARelationshipFemale;
          break;
        case RelationshipStatus.Engaged:
          if (num > 0L)
          {
            str1 = flag ? CommonResources.ProfilePage_Info_EngagedMaleWith : CommonResources.ProfilePage_Info_EngagedFemaleWith;
            str2 = partnerNameCases.ins;
            break;
          }
          str1 = flag ? CommonResources.ProfilePage_Info_EngagedMale : CommonResources.ProfilePage_Info_EngagedFemale;
          break;
        case RelationshipStatus.Married:
          if (num > 0L)
          {
            str1 = flag ? CommonResources.ProfilePage_Info_MarriedMaleWith : CommonResources.ProfilePage_Info_MarriedFemaleWith;
            str2 = flag ? partnerNameCases.abl : partnerNameCases.ins;
            break;
          }
          str1 = flag ? CommonResources.ProfilePage_Info_MarriedMale : CommonResources.ProfilePage_Info_MarriedFemale;
          break;
        case RelationshipStatus.ItIsComplicated:
          if (num > 0L)
          {
            str1 = CommonResources.ProfilePage_Info_ItIsComplicatedWith;
            str2 = partnerNameCases.ins;
            break;
          }
          str1 = CommonResources.ProfilePage_Info_ItIsComplicated;
          break;
        case RelationshipStatus.ActivelySearching:
          str1 = CommonResources.ProfilePage_Info_ActivelySearching;
          break;
        case RelationshipStatus.InLove:
          if (num > 0L)
          {
            str1 = flag ? CommonResources.ProfilePage_Info_InLoveMaleWith : CommonResources.ProfilePage_Info_InLoveFemaleWith;
            str2 = partnerNameCases.acc;
            break;
          }
          str1 = flag ? CommonResources.ProfilePage_Info_InLoveMale : CommonResources.ProfilePage_Info_InLoveFemale;
          break;
        case RelationshipStatus.InCivilUnion:
          if (num > 0L)
          {
            str1 = CommonResources.InCivilUnionWith;
            str2 = partnerNameCases.ins;
            break;
          }
          str1 = CommonResources.InCivilUnion;
          break;
      }
      this.Data = num > 0L ? string.Format("{0} [id{1}|{2}]", str1, num, str2) : str1;
    }
  }
}
