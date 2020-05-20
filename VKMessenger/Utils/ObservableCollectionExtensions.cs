using System;
using System.Collections.ObjectModel;

namespace VKMessenger.Utils
{
  public static class ObservableCollectionExtensions
  {
    public static void AddOrdered<T>(this ObservableCollection<T> observableCollection, T item, Func<T, T, int> comparisonFunction, bool startFromTheEndHint)
    {
      if (observableCollection.Count == 0)
        observableCollection.Add(item);
      else if (startFromTheEndHint)
      {
        for (int count = observableCollection.Count; count >= 0; --count)
        {
          if (ObservableCollectionExtensions.ShouldInsertAtCurrentIndex<T>(observableCollection, item, comparisonFunction, count))
          {
            observableCollection.Insert(count, item);
            break;
          }
        }
      }
      else
      {
        for (int index = 0; index <= observableCollection.Count; ++index)
        {
          if (ObservableCollectionExtensions.ShouldInsertAtCurrentIndex<T>(observableCollection, item, comparisonFunction, index))
          {
            observableCollection.Insert(index, item);
            break;
          }
        }
      }
    }

    private static bool ShouldInsertAtCurrentIndex<T>(ObservableCollection<T> observableCollection, T item, Func<T, T, int> comparisonFunction, int i)
    {
      bool flag = false;
      if (i == observableCollection.Count && comparisonFunction(observableCollection[i - 1], item) >= 0)
        flag = true;
      else if (i > 0 && i < observableCollection.Count && (comparisonFunction(item, observableCollection[i]) >= 0 && comparisonFunction(observableCollection[i - 1], item) >= 0))
        flag = true;
      else if (i == 0 && comparisonFunction(item, observableCollection[0]) >= 0)
        flag = true;
      return flag;
    }
  }
}
