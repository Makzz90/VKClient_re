using System.Collections.Generic;
using System.Linq;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public class SearchParamsVideoViewModel : ViewModelBase
  {
    private readonly List<string> _durationTypes = new List<string>() { CommonResources.VideoSearch_Filter_Any, CommonResources.VideoSearch_Filter_Short, CommonResources.VideoSearch_Filter_Long };
    private readonly List<string> _sortTypes = new List<string>() { CommonResources.VideoSearch_Sort_ByRelevance, CommonResources.VideoSearch_Sort_ByDate, CommonResources.VideoSearch_Sort_ByDuration };

    public List<string> DurationTypes
    {
      get
      {
        return this._durationTypes;
      }
    }

    public string DurationType
    {
      get
      {
        if (this.Parameters.ContainsKey("filters"))
        {
          string parameter = this.Parameters["filters"];
          if (parameter == "short")
            return this._durationTypes[1];
          if (parameter == "long")
            return this._durationTypes[2];
        }
        return (string) Enumerable.First<string>(this._durationTypes);
      }
      set
      {
        if (string.IsNullOrEmpty(value) || value == this._durationTypes[0])
        {
          if (!this.Parameters.ContainsKey("filters"))
            return;
          this.Parameters.Remove("filters");
        }
        else
        {
          if (value == this._durationTypes[1])
            this.Parameters["filters"] = "short";
          else if (value == this._durationTypes[2])
            this.Parameters["filters"] = "long";
          this.NotifyPropertyChanged("DurationType");
        }
      }
    }

    public List<string> SortTypes
    {
      get
      {
        return this._sortTypes;
      }
    }

    public string SortType
    {
      get
      {
        if (this.Parameters.ContainsKey("sort"))
        {
          string parameter = this.Parameters["sort"];
          if (parameter == "0")
            return this._sortTypes[1];
          if (parameter == "1")
            return this._sortTypes[2];
        }
        return (string) Enumerable.First<string>(this._sortTypes);
      }
      set
      {
        if (string.IsNullOrEmpty(value) || value == this._sortTypes[0])
        {
          if (!this.Parameters.ContainsKey("sort"))
            return;
          this.Parameters.Remove("sort");
        }
        else
        {
          if (value == this._sortTypes[1])
            this.Parameters["sort"] = "0";
          else if (value == this._sortTypes[2])
            this.Parameters["sort"] = "1";
          this.NotifyPropertyChanged("SortType");
        }
      }
    }

    public bool IsHD
    {
      get
      {
        if (this.Parameters.ContainsKey("hd"))
          return this.Parameters["hd"] == "1";
        return false;
      }
      set
      {
        this.Parameters["hd"] = value ? "1" : "0";
        this.NotifyPropertyChanged("IsHD");
      }
    }

    public bool IsSafeSearch
    {
      get
      {
        if (this.Parameters.ContainsKey("adult"))
          return this.Parameters["adult"] == "0";
        return true;
      }
      set
      {
        this.Parameters["adult"] = value ? "0" : "1";
        this.NotifyPropertyChanged("IsSafeSearch");
      }
    }

    public Dictionary<string, string> Parameters { get; private set; }

    public SearchParamsVideoViewModel(Dictionary<string, string> parameters)
    {
      this.Parameters = parameters ?? new Dictionary<string, string>();
    }
  }
}
