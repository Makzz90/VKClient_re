using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Framework;

namespace VKMessenger.Library
{
  public class AttachmentTemplateSelector : DataTemplateSelector
  {
    public DataTemplate PhotoTemplate { get; set; }

    public DataTemplate VideoTemplate { get; set; }

    public DataTemplate AudioTemplate { get; set; }

    public DataTemplate GeoTemplate { get; set; }

    public DataTemplate DocumentTemplate { get; set; }

    public DataTemplate DocumentImageTemplate { get; set; }

    public DataTemplate DocumentGraffitiTemplate { get; set; }

    public DataTemplate WallPostTemplate { get; set; }

    public DataTemplate StickerTemplate { get; set; }

    public AttachmentTemplateSelector()
    {
      ((Control) this).HorizontalContentAlignment=((HorizontalAlignment) 3);
    }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      AttachmentViewModel attachmentViewModel = item as AttachmentViewModel;
      if (attachmentViewModel == null)
        return  null;
      switch (attachmentViewModel.AttachmentType)
      {
        case AttachmentType.Photo:
          return this.PhotoTemplate;
        case AttachmentType.Video:
          return this.VideoTemplate;
        case AttachmentType.Audio:
          return this.AudioTemplate;
        case AttachmentType.Geo:
          return this.GeoTemplate;
        case AttachmentType.Document:
          if (attachmentViewModel.IsDocumentImageAttachement)
            return this.DocumentImageTemplate;
          if (attachmentViewModel.IsDocumentGraffitiAttachment)
            return this.DocumentGraffitiTemplate;
          return this.DocumentTemplate;
        case AttachmentType.WallPost:
        case AttachmentType.WallReply:
          return this.WallPostTemplate;
        case AttachmentType.Sticker:
          return this.StickerTemplate;
        default:
          return  null;
      }
    }
  }
}
