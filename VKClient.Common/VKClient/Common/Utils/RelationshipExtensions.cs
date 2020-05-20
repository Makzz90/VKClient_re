using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;

namespace VKClient.Common.Utils
{
  public static class RelationshipExtensions
  {
    public static string GetRelationshipStr(this RelationshipStatus status, int sex = 0)
    {
      bool flag = sex != 1;
      switch (status)
      {
        case RelationshipStatus.NotMarried:
          if (!flag)
            return CommonResources.Relationship_Single_Female;
          return CommonResources.Relationship_Single_Male;
        case RelationshipStatus.InARelationship:
          if (!flag)
            return CommonResources.Relationship_InARelationship_Female;
          return CommonResources.Relationship_InARelationship_Male;
        case RelationshipStatus.Engaged:
          if (!flag)
            return CommonResources.Relationship_Engaged_Female;
          return CommonResources.Relationship_Engaged_Male;
        case RelationshipStatus.Married:
          if (!flag)
            return CommonResources.Relationship_Married_Female;
          return CommonResources.Relationship_Married_Male;
        case RelationshipStatus.ItIsComplicated:
          return CommonResources.Relationship_ItIsComplicated;
        case RelationshipStatus.ActivelySearching:
          return CommonResources.Relationship_ActivelySearching;
        case RelationshipStatus.InLove:
          if (!flag)
            return CommonResources.Relationship_InLove_Female;
          return CommonResources.Relationship_InLove_Male;
        default:
          return "";
      }
    }
  }
}
