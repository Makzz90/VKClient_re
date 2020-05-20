using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class CommunitySettings
  {
    public string title { get; set; }

    public string description { get; set; }

    public string address { get; set; }

    public string website { get; set; }

    public long subject { get; set; }

    public long public_category { get; set; }

    public long public_subcategory { get; set; }

    public List<Section> subject_list { get; set; }

    public List<Section> public_category_list { get; set; }

    public string type { get; set; }

    public GroupType Type
    {
      get
      {
        if (this.type == "group")
          return GroupType.Group;
        return !(this.type == "public") && !(this.type == "page") ? GroupType.Event : GroupType.PublicPage;
      }
    }

    public int access { get; set; }

    public Place place { get; set; }

    public int age_limits { get; set; }

    public string public_date { get; set; }

    public string public_date_label { get; set; }

    public long event_group_id { get; set; }

    public string email { get; set; }

    public string phone { get; set; }

    public User event_creator { get; set; }

    public List<Group> event_available_organizers { get; set; }

    public int? start_date { get; set; }

    public int? finish_date { get; set; }

    public int wall { get; set; }

    public int photos { get; set; }

    public int video { get; set; }

    public int audio { get; set; }

    public int docs { get; set; }

    public int topics { get; set; }

    public int links { get; set; }

    public int events { get; set; }

    public int contacts { get; set; }

    public int obscene_filter { get; set; }

    public int obscene_stopwords { get; set; }

    public string[] obscene_words { get; set; }
  }
}
