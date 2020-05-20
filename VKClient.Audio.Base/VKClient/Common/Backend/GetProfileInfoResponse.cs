using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class GetProfileInfoResponse
  {
    public User User { get; set; }

    public ProfileInfo ProfileInfo { get; set; }

    public User partner { get; set; }

    public List<User> RelationRequests { get; set; }
  }
}
