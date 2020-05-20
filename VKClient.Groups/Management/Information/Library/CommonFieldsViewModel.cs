using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Information.Library
{
  public sealed class CommonFieldsViewModel : ViewModelBase
  {
    private string _name = "";
    private string _description = "";
    private string _domain = "";
    private string _site = "";
    private List<Section> _publicPageCategories;
    private CustomListPickerItem _category;
    private List<CustomListPickerItem> _availableCategories;
    private CustomListPickerItem _subcategory;
    private List<CustomListPickerItem> _availableSubcategories;
    private string _categoryTitle;
    private string _categoryPlaceholder;

    public InformationViewModel ParentViewModel { get; private set; }

    public string CurrentDomain { get; set; }

    public string Name
    {
      get
      {
        return this._name;
      }
      set
      {
        this._name = value;
        this.NotifyPropertyChanged<string>((() => this.Name));
        this.NotifyPropertyChanged<bool>((() => this.ParentViewModel.IsFormCompleted));
      }
    }

    public string Description
    {
      get
      {
        return this._description;
      }
      set
      {
        this._description = value;
        this.NotifyPropertyChanged<string>((() => this.Description));
      }
    }

    public string Domain
    {
      get
      {
        return this._domain;
      }
      set
      {
        this._domain = value;
        this.NotifyPropertyChanged<string>((() => this.Domain));
        this.NotifyPropertyChanged<bool>((() => this.ParentViewModel.IsFormCompleted));
      }
    }

    public string Site
    {
      get
      {
        return this._site;
      }
      set
      {
        this._site = value;
        this.NotifyPropertyChanged<string>((() => this.Site));
      }
    }

    public CustomListPickerItem Category
    {
        get
        {
            return this._category;
        }
        set
        {
            this._category = value;
            if (this._publicPageCategories != null && value != null)
            {
                List<Section> subtypesList = this._publicPageCategories.First<Section>((Func<Section, bool>)(category => category.id == value.Id)).subtypes_list;
                this.AvailableSubcategories = subtypesList != null ? subtypesList.Select<Section, CustomListPickerItem>((Func<Section, CustomListPickerItem>)(c =>
                {
                    CustomListPickerItem customListPickerItem = new CustomListPickerItem();
                    customListPickerItem.Name = c.name;
                    customListPickerItem.Id = c.id;
                    int num = c.id == 0L ? 1 : 0;
                    customListPickerItem.IsUnknown = num != 0;
                    return customListPickerItem;
                })).ToList<CustomListPickerItem>() : (List<CustomListPickerItem>)null;
                List<CustomListPickerItem> availableSubcategories = this.AvailableSubcategories;
                this.Subcategory = availableSubcategories != null ? availableSubcategories.First<CustomListPickerItem>() : (CustomListPickerItem)null;
            }
            this.NotifyPropertyChanged<CustomListPickerItem>((Expression<Func<CustomListPickerItem>>)(() => this.Category));
            this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.SubcategoryVisibility));
        }
    }

    public List<CustomListPickerItem> AvailableCategories
    {
      get
      {
        return this._availableCategories;
      }
      set
      {
        this._availableCategories = value;
        this.NotifyPropertyChanged<List<CustomListPickerItem>>((Expression<Func<List<CustomListPickerItem>>>) (() => this.AvailableCategories));
      }
    }

    public CustomListPickerItem Subcategory
    {
      get
      {
        return this._subcategory;
      }
      set
      {
        this._subcategory = value;
        this.NotifyPropertyChanged<CustomListPickerItem>((() => this.Subcategory));
      }
    }

    public List<CustomListPickerItem> AvailableSubcategories
    {
      get
      {
        return this._availableSubcategories;
      }
      set
      {
        this._availableSubcategories = value;
        this.NotifyPropertyChanged<List<CustomListPickerItem>>((Expression<Func<List<CustomListPickerItem>>>) (() => this.AvailableSubcategories));
        this.NotifyPropertyChanged<Visibility>((() => this.SubcategoryVisibility));
      }
    }

    public Visibility SubcategoryVisibility
    {
      get
      {
        int num;
        if (this._publicPageCategories != null)
        {
          CustomListPickerItem category = this.Category;
          if ((category != null ? ((ulong) category.Id > 0UL ? 1 : 0) : 1) != 0)
          {
            num = this.AvailableSubcategories != null ? 1 : 0;
            goto label_4;
          }
        }
        num = 0;
label_4:
        return (num != 0).ToVisiblity();
      }
    }

    public string CategoryTitle
    {
      get
      {
        return this._categoryTitle;
      }
      set
      {
        this._categoryTitle = value;
        this.NotifyPropertyChanged<string>((() => this.CategoryTitle));
      }
    }

    public string CategoryPlaceholder
    {
      get
      {
        return this._categoryPlaceholder;
      }
      set
      {
        this._categoryPlaceholder = value;
        this.NotifyPropertyChanged<string>((() => this.CategoryPlaceholder));
      }
    }

    public CommonFieldsViewModel(InformationViewModel parentViewModel)
    {
      this.ParentViewModel = parentViewModel;
    }

    public void Read(CommunitySettings information)
    {
        this.Name = Extensions.ForUI(information.title);
        this.Description = Extensions.ForUI(information.description);
        this.Domain = this.CurrentDomain = information.address;
        this.Site = Extensions.ForUI(information.website);
        if (information.Type != GroupType.PublicPage)
        {
            this.CategoryTitle = CommonResources.CommunitySubject;
            this.CategoryPlaceholder = CommonResources.SelectSubject;
            information.subject_list.Insert(0, new Section()
            {
                id = 0,
                name = CommonResources.NoneSelected
            });
            this.AvailableCategories = information.subject_list.Select<Section, CustomListPickerItem>((Func<Section, CustomListPickerItem>)(c =>
            {
                CustomListPickerItem customListPickerItem = new CustomListPickerItem();
                customListPickerItem.Name = c.name;
                customListPickerItem.Id = c.id;
                int num = c.id == 0L ? 1 : 0;
                customListPickerItem.IsUnknown = num != 0;
                return customListPickerItem;
            })).ToList<CustomListPickerItem>();
            this.Category = this.AvailableCategories.First<CustomListPickerItem>((Func<CustomListPickerItem, bool>)(c => c.Id == information.subject));
        }
        else
        {
            this.CategoryTitle = CommonResources.PublicPageCategory;
            this.CategoryPlaceholder = CommonResources.SelectCategory;
            for (int index = 1; index < information.public_category_list.Count; ++index)
            {
                List<Section> subtypesList = information.public_category_list[index].subtypes_list;
                if (subtypesList != null && subtypesList.Any<Section>())
                    subtypesList.First<Section>().name = CommonResources.NoneSelected;
            }
            information.public_category_list.First<Section>().name = CommonResources.NoneSelected;
            this._publicPageCategories = information.public_category_list;
            this.AvailableCategories = information.public_category_list.Select<Section, CustomListPickerItem>((Func<Section, CustomListPickerItem>)(c =>
            {
                CustomListPickerItem customListPickerItem = new CustomListPickerItem();
                customListPickerItem.Name = c.name;
                customListPickerItem.Id = c.id;
                int num = c.id == 0L ? 1 : 0;
                customListPickerItem.IsUnknown = num != 0;
                return customListPickerItem;
            })).ToList<CustomListPickerItem>();
            this.Category = this.AvailableCategories.First<CustomListPickerItem>((Func<CustomListPickerItem, bool>)(c => c.Id == information.public_category));
            if (this.Category.Id == 0L)
                return;
            List<CustomListPickerItem> availableSubcategories = this.AvailableSubcategories;
            CustomListPickerItem customListPickerItem1;
            if (availableSubcategories == null)
            {
                customListPickerItem1 = (CustomListPickerItem)null;
            }
            else
            {
                Func<CustomListPickerItem, bool> predicate = (Func<CustomListPickerItem, bool>)(s => s.Id == information.public_subcategory);
                customListPickerItem1 = availableSubcategories.First<CustomListPickerItem>(predicate);
            }
            this.Subcategory = customListPickerItem1;
        }
    }
  }
}
