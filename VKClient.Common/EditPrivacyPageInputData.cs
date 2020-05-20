using System;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Library;

namespace VKClient.Common
{
  public class EditPrivacyPageInputData
  {
    public EditPrivacyViewModel PrivacyForEdit { get; set; }

    public Action<PrivacyInfo> UpdatePrivacyCallback { get; set; }
  }
}
