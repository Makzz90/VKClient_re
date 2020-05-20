using System;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;

namespace VKClient.Common.Library
{
  public class SettingsPrivacyViewModel : ViewModelBase, ICollectionDataProvider<PrivacySettingsInfo, Group<EditPrivacyViewModel>>
  {
    private GenericCollectionViewModel<PrivacySettingsInfo, Group<EditPrivacyViewModel>> _privacyCollection;

    public GenericCollectionViewModel<PrivacySettingsInfo, Group<EditPrivacyViewModel>> PrivacyCollection
    {
      get
      {
        return this._privacyCollection;
      }
    }

    public Func<PrivacySettingsInfo, ListWithCount<Group<EditPrivacyViewModel>>> ConverterFunc
    {
      get
      {
        return (Func<PrivacySettingsInfo, ListWithCount<Group<EditPrivacyViewModel>>>) (psi => new ListWithCount<Group<EditPrivacyViewModel>>()
        {
          List = psi.settings.GroupBy<PrivacySetting, string>((Func<PrivacySetting, string>) (ps => ps.section)).Select<IGrouping<string, PrivacySetting>, Group<EditPrivacyViewModel>>((Func<IGrouping<string, PrivacySetting>, Group<EditPrivacyViewModel>>) (psg => new Group<EditPrivacyViewModel>(psi.sections.First<PrivacySection>((Func<PrivacySection, bool>) (s => s.name == psg.Key)).title.ToUpperInvariant(), psg.Select<PrivacySetting, EditPrivacyViewModel>((Func<PrivacySetting, EditPrivacyViewModel>) (privacySetting => new EditPrivacyViewModel(privacySetting.title, new PrivacyInfo(privacySetting.value), privacySetting.key, privacySetting.supported_values))), false))).ToList<Group<EditPrivacyViewModel>>()
        });
      }
    }

    public SettingsPrivacyViewModel()
    {
      this._privacyCollection = new GenericCollectionViewModel<PrivacySettingsInfo, Group<EditPrivacyViewModel>>((ICollectionDataProvider<PrivacySettingsInfo, Group<EditPrivacyViewModel>>) this);
    }

    public void GetData(GenericCollectionViewModel<PrivacySettingsInfo, Group<EditPrivacyViewModel>> caller, int offset, int count, Action<BackendResult<PrivacySettingsInfo, ResultCode>> callback)
    {
      AccountService.Instance.GetPrivacySettings(callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<PrivacySettingsInfo, Group<EditPrivacyViewModel>> caller, int count)
    {
      return "";
    }

    internal void UpdatePrivacy(EditPrivacyViewModel vm, PrivacyInfo pi)
    {
      AccountService.Instance.SetPrivacy(vm.Key, pi.ToString(), (Action<BackendResult<ResponseWithId, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (res.ResultCode != ResultCode.Succeeded)
          GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", (VKRequestsDispatcher.Error) null);
        else
          vm.ReadFromPrivacyInfo(pi);
      }))));
    }
  }
}
