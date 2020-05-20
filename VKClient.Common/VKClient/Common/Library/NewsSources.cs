using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common.Library
{
  public class NewsSources
  {
    public static PickableNewsfeedSourceItemViewModel NewsFeed { get{return new PickableNewsfeedSourceItemViewModel(new PickableItem()
    {
      ID = -10L,
      Name = CommonResources.NEWS.ToLowerInvariant(),
      ImageSource = new Uri("/Resources/NotUsed", UriKind.Relative)
    });
    }}
    public static PickableNewsfeedSourceItemViewModel Suggestions { get{return new PickableNewsfeedSourceItemViewModel(new PickableItem()
    {
      ID = -9L,
      Name = CommonResources.NewsFeedSuggestions,
      Alias = CommonResources.NewsFeedSuggestions
    });
        }}
    public static PickableNewsfeedSourceItemViewModel Photos { get{return new PickableNewsfeedSourceItemViewModel(new PickableItem()
    {
      ID = -101L,
      Name = CommonResources.MainPage_Menu_Photos,
      Alias = CommonResources.MainPage_Menu_Photos
    });
        }}
    public static PickableNewsfeedSourceItemViewModel Videos { get{return new PickableNewsfeedSourceItemViewModel(new PickableItem()
    {
      ID = -100L,
      Name = CommonResources.MainPage_Menu_Video,
      Alias = CommonResources.MainPage_Menu_Video
    });
        }}
    public static PickableNewsfeedSourceItemViewModel Friends { get{return new PickableNewsfeedSourceItemViewModel(new PickableItem()
    {
      ID = -8L,
      Name = CommonResources.FriendsLowCase,
      Alias = CommonResources.FriendsLowCase
    });
        }}
    public static PickableNewsfeedSourceItemViewModel Groups { get{return new PickableNewsfeedSourceItemViewModel(new PickableItem()
    {
      ID = -7L,
      Name = CommonResources.Groups.ToLowerInvariant(),
      Alias = CommonResources.Groups
    });
    }
    }
    public static ObservableCollection<PickableNewsfeedSourceItemViewModel> GetPredefinedNewsSources()
    {
      ObservableCollection<PickableNewsfeedSourceItemViewModel> observableCollection = new ObservableCollection<PickableNewsfeedSourceItemViewModel>();
      PickableNewsfeedSourceItemViewModel newsFeed = NewsSources.NewsFeed;
      observableCollection.Add(newsFeed);
      PickableNewsfeedSourceItemViewModel suggestions = NewsSources.Suggestions;
      observableCollection.Add(suggestions);
      return observableCollection;
    }

    public static ObservableCollection<PickableNewsfeedSourceItemViewModel> GetAllPredefinedNewsSources()
    {
      ObservableCollection<PickableNewsfeedSourceItemViewModel> observableCollection = new ObservableCollection<PickableNewsfeedSourceItemViewModel>();
      PickableNewsfeedSourceItemViewModel newsFeed = NewsSources.NewsFeed;
      observableCollection.Add(newsFeed);
      PickableNewsfeedSourceItemViewModel suggestions = NewsSources.Suggestions;
      observableCollection.Add(suggestions);
      PickableNewsfeedSourceItemViewModel friends = NewsSources.Friends;
      observableCollection.Add(friends);
      PickableNewsfeedSourceItemViewModel photos = NewsSources.Photos;
      observableCollection.Add(photos);
      PickableNewsfeedSourceItemViewModel videos = NewsSources.Videos;
      observableCollection.Add(videos);
      return observableCollection;
    }

    public static ObservableCollection<PickableNewsfeedSourceItemViewModel> GetNewsSources(NewsFeedSectionsAndLists sectionsAndLists, PickableItem currentNewsSource = null)
    {
      if (sectionsAndLists == null)
        return (ObservableCollection<PickableNewsfeedSourceItemViewModel>) null;
      ObservableCollection<PickableNewsfeedSourceItemViewModel> predefinedNewsSources = NewsSources.GetPredefinedNewsSources();
      if (currentNewsSource != null)
      {
        switch ((NewsSourcesPredefined) currentNewsSource.ID)
        {
          case NewsSourcesPredefined.Friends:
            predefinedNewsSources.Add(NewsSources.Friends);
            break;
          case NewsSourcesPredefined.Communities:
            predefinedNewsSources.Add(NewsSources.Groups);
            break;
          case NewsSourcesPredefined.Photo:
            predefinedNewsSources.Add(NewsSources.Photos);
            break;
          case NewsSourcesPredefined.Video:
            predefinedNewsSources.Add(NewsSources.Videos);
            break;
        }
      }
      List<NewsFeedSection> sections = sectionsAndLists.sections;
      if (sections != null)
      {
        foreach (NewsFeedSection newsFeedSection in sections)
        {
          if (newsFeedSection.enabled == 1)
          {
            string name = newsFeedSection.name;
            if (!(name == "photos"))
            {
              if (!(name == "videos"))
              {
                if (!(name == "friends"))
                {
                  if (name == "groups" && !predefinedNewsSources.Contains(NewsSources.Groups))
                    predefinedNewsSources.Insert(Math.Min(4, predefinedNewsSources.Count), NewsSources.Groups);
                }
                else if (!predefinedNewsSources.Contains(NewsSources.Friends))
                  predefinedNewsSources.Insert(Math.Min(4, predefinedNewsSources.Count), NewsSources.Friends);
              }
              else if (!predefinedNewsSources.Contains(NewsSources.Videos))
                predefinedNewsSources.Insert(Math.Min(3, predefinedNewsSources.Count), NewsSources.Videos);
            }
            else if (!predefinedNewsSources.Contains(NewsSources.Photos))
              predefinedNewsSources.Insert(Math.Min(2, predefinedNewsSources.Count), NewsSources.Photos);
          }
        }
      }
      List<NewsFeedList> lists = sectionsAndLists.lists;
      if (lists != null)
      {
        foreach (NewsFeedList newsFeedList in lists)
          predefinedNewsSources.Add(new PickableNewsfeedSourceItemViewModel(new PickableItem()
          {
            ID = newsFeedList.id,
            Name = newsFeedList.title,
            Alias = newsFeedList.title
          }));
      }
      return predefinedNewsSources;
    }
  }
}
