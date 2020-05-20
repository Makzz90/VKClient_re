using System;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.Events
{
  public sealed class CommunityInformationChanged
  {
    public long Id { get; set; }

    public string Name { get; set; }

    public string CategoryName { get; set; }

    public string SubcategoryName { get; set; }

    public GroupPrivacy Privacy { get; set; }

    public DateTime EventStartDate { get; set; }
  }
}
