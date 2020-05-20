using System.Windows;
using VKClient.Common.Framework;

namespace VKMessenger.Library
{
  public class MessageTemplateSelector : DataTemplateSelector
  {
    public DataTemplate StandardTemplate { get; set; }

    public DataTemplate SimplifiedTemplate { get; set; }

    public DataTemplate StickerTemplate { get; set; }

    public DataTemplate GraffitiTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      MessageViewModel messageViewModel = item as MessageViewModel;
      if (messageViewModel == null)
        return  null;
      if (!messageViewModel.IsChat && (messageViewModel.Attachments == null || messageViewModel.Attachments.Count == 0) && (messageViewModel.ForwardedMessages == null || messageViewModel.ForwardedMessages.Count == 0))
        return this.SimplifiedTemplate;
      if (messageViewModel.IsSticker)
        return this.StickerTemplate;
      if (messageViewModel.IsGraffiti)
        return this.GraffitiTemplate;
      return this.StandardTemplate;
    }
  }
}
