using System;
using System.Linq;
using VKClient.Audio.Base.Localization;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend.DataObjects;
using VKMessenger.Backend;

namespace VKClient.Audio.Base
{
  public class MessageHeaderFormatterHelper
  {
    public static void FormatForTileIntoThreeStrings(Message message, User user, out string content1, out string content2, out string content3)
    {
      content1 = "";
      content2 = "";
      content3 = "";
      if (message == null || user == null)
        return;
      content1 = message.chat_id != 0 ? string.Format("{0} ({1})", user.Name, BaseResources.InChat) : user.Name;
      if (!string.IsNullOrWhiteSpace(message.body))
      {
        content2 = message.body.Replace(Environment.NewLine, " ");
        content3 = MessageHeaderFormatterHelper.GetAttachmentsDesc(message);
      }
      else
        content2 = MessageHeaderFormatterHelper.GetAttachmentsDesc(message);
    }

    private static string GetAttachmentsDesc(Message message)
    {
      string str = "";
      if (message.attachments != null && message.attachments.Count > 0)
      {
        Attachment firstAttachment = message.attachments.First<Attachment>();
        int number = message.attachments.Count<Attachment>((Func<Attachment, bool>) (a => a.type == firstAttachment.type));
        string lowerInvariant = firstAttachment.type.ToLowerInvariant();
        if (!(lowerInvariant == "photo"))
        {
          if (!(lowerInvariant == "video"))
          {
            if (!(lowerInvariant == "audio"))
            {
              if (!(lowerInvariant == "doc"))
              {
                if (lowerInvariant == "wall")
                  str = BaseResources.WallPost;
              }
              else
                str = BaseFormatterHelper.FormatNumberOfSomething(number, BaseResources.OneDocFrm, BaseResources.TwoFourDocsFrm, BaseResources.FiveDocsFrm, true,  null, false);
            }
            else
              str = BaseFormatterHelper.FormatNumberOfSomething(number, BaseResources.OneAudioFrm, BaseResources.TwoFourAudiosFrm, BaseResources.FiveAudiosFrm, true,  null, false);
          }
          else
            str = BaseFormatterHelper.FormatNumberOfSomething(number, BaseResources.OneVideoFrm, BaseResources.TwoFourVideosFrm, BaseResources.FiveVideosFrm, true,  null, false);
        }
        else
          str = BaseFormatterHelper.FormatNumberOfSomething(number, BaseResources.OnePhotoFrm, BaseResources.TwoFourPhotosFrm, BaseResources.FivePhotosFrm, true,  null, false);
      }
      return str;
    }
  }
}
