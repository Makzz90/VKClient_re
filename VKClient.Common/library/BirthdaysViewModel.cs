using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Audio.Base.Extensions;
namespace VKClient.Common.Library
{
  public class BirthdaysViewModel : ViewModelBase, ICollectionDataProvider<List<User>, Group<Birthday>>
  {
      public GenericCollectionViewModel<List<User>, Group<Birthday>> BithdaysGroupsViewModel { get; private set; }

    public string Title
    {
      get
      {
        return CommonResources.Birthdays_Title;
      }
    }

    public Func<List<User>, ListWithCount<Group<Birthday>>> ConverterFunc
    {
      get
      {
        return (Func<List<User>, ListWithCount<Group<Birthday>>>) (list =>
        {
          ListWithCount<Group<Birthday>> listWithCount = new ListWithCount<Group<Birthday>>();
          Dictionary<string, Group<Birthday>> dictionary = new Dictionary<string, Group<Birthday>>();
          DateTime dateTime = DateTime.Now;
          string index1 = CommonResources.Today.Capitalize();
          string tomorrow = CommonResources.Tomorrow;
          IEnumerator<User> enumerator1 = ((IEnumerable<User>)Enumerable.OrderBy<User, int>(Enumerable.Where<User>(list, (Func<User, bool>)(u =>
          {
            if (u.BirthDate.Day.HasValue)
              return u.BirthDate.Month.HasValue;
            return false;
          })), (Func<User, int>) (u =>
          {
            DateTime baseDate = dateTime;
            int? nullable = u.BirthDate.Month;
            int month = nullable.Value;
            nullable = u.BirthDate.Day;
            int day = nullable.Value;
            return BirthdaysViewModel.GetDiffFromDate(baseDate, month, day);
          }))).GetEnumerator();
          try
          {
            while (((IEnumerator) enumerator1).MoveNext())
            {
              User current = enumerator1.Current;
              BirthDate birthDate = current.BirthDate;
              int? nullable = birthDate.Day;
              if (nullable.HasValue)
              {
                nullable = birthDate.Month;
                if (nullable.HasValue)
                {
                  if (birthDate.IsToday)
                  {
                    if (!dictionary.ContainsKey(index1))
                      dictionary.Add(index1, new Group<Birthday>(index1, false));
                    nullable = birthDate.Year;
                    string subtitle;
                    if (nullable.HasValue)
                    {
                      int year = dateTime.Year;
                      nullable = birthDate.Year;
                      int num = nullable.Value;
                      subtitle = ( UIStringFormatterHelper.FormatNumberOfSomething(year - num, CommonResources.TurnsOneYearTodayFrm, CommonResources.TurnsTwoFourYearsTodayFrm, CommonResources.TurnsFiveYearsTodayFrm, true,  null, false)).ToLowerInvariant();
                    }
                    else
                      subtitle = CommonResources.HasABirthdayToday;
                    ((Collection<Birthday>) dictionary[index1]).Add(new Birthday(current, subtitle, true));
                  }
                  else if (birthDate.IsTomorrow)
                  {
                    if (!dictionary.ContainsKey(tomorrow))
                      dictionary.Add(tomorrow, new Group<Birthday>(tomorrow, false));
                    ((Collection<Birthday>) dictionary[tomorrow]).Add(new Birthday(current, birthDate.GetDateString(), false));
                  }
                  else
                  {
                    nullable = birthDate.Month;
                    int month = nullable.Value;
                    nullable = birthDate.Day;
                    int num = nullable.Value;
                    string index2 = month != dateTime.Month || num >= dateTime.Day ? CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) : string.Concat(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month), " ");
                    if (!dictionary.ContainsKey(index2))
                      dictionary.Add(index2, new Group<Birthday>(index2, false));
                    ((Collection<Birthday>) dictionary[index2]).Add(new Birthday(current, birthDate.GetDateString(), false));
                  }
                }
              }
            }
          }
          finally
          {
            if (enumerator1 != null)
              ((IDisposable) enumerator1).Dispose();
          }
          if (dictionary.Count > 0)
            dictionary[((KeyValuePair<string, Group<Birthday>>) Enumerable.First<KeyValuePair<string, Group<Birthday>>>(dictionary)).Key].SeparatorVisibility = Visibility.Collapsed;
          Dictionary<string, Group<Birthday>>.ValueCollection.Enumerator enumerator2 = dictionary.Values.GetEnumerator();
          try
          {
            while (enumerator2.MoveNext())
            {
              Group<Birthday> current = enumerator2.Current;
              listWithCount.List.Add(current);
            }
          }
          finally
          {
            enumerator2.Dispose();
          }
          return listWithCount;
        });
      }
    }

    public BirthdaysViewModel()
    {
      this.BithdaysGroupsViewModel = new GenericCollectionViewModel<List<User>, Group<Birthday>>((ICollectionDataProvider<List<User>, Group<Birthday>>) this)
      {
        NeedCollectionCountBeforeFullyLoading = true
      };
    }

    public async void GetData(GenericCollectionViewModel<List<User>, Group<Birthday>> caller, int offset, int count, Action<BackendResult<List<User>, ResultCode>> callback)
    {
      if (offset != 0)
        return;
      SavedContacts friends = await FriendsCache.Instance.GetFriends();
      callback(new BackendResult<List<User>, ResultCode>(ResultCode.Succeeded, friends.SavedUsers));
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<List<User>, Group<Birthday>> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoFriends;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneFriendFrm, CommonResources.TwoFourFriendsFrm, CommonResources.FiveFriendsFrm, true,  null, false);
    }

    private static int GetDiffFromDate(DateTime baseDate, int month, int day)
    {
      int num1 = baseDate.Month * 100 + baseDate.Day;
      int num2 = month * 100 + day;
      if (num2 >= num1)
        return num2 - num1;
      return num2 - num1 + 100000;
    }
  }
}
