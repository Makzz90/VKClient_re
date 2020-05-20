using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VKMessenger.Library
{
  public class Group<T> : ObservableCollection<T>
  {
    public string Title { get; set; }

    public bool HasTitle
    {
      get
      {
        return !string.IsNullOrWhiteSpace(this.Title);
      }
    }

    public Group(string name, IEnumerable<T> items)
      : base(items)
    {
      this.Title = name;
    }

    public override bool Equals(object obj)
    {
      Group<T> group = obj as Group<T>;
      if (group != null)
        return this.Title.Equals(group.Title);
      return false;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
  }
}
