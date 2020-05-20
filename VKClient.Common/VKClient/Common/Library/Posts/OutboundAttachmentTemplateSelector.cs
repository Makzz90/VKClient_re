using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.Posts
{
  public class OutboundAttachmentTemplateSelector : DataTemplateSelector
  {
    public DataTemplate PhotoTemplate { get; set; }

    public DataTemplate GeoTemplate { get; set; }

    public DataTemplate VideoTemplate { get; set; }

    public DataTemplate AudioTemplate { get; set; }

    public DataTemplate DocumentTemplate { get; set; }

    public DataTemplate GenericThumbTemplate { get; set; }

    public DataTemplate AddAttachmentTemplate { get; set; }

    public DataTemplate WallPostTemplate { get; set; }

    public DataTemplate GenericIconTemplate { get; set; }

    public DataTemplate ForwardedMessageTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (item is OutboundPhotoAttachment)
        return this.PhotoTemplate;
      if (item is OutboundVideoAttachment || item is OutboundUploadVideoAttachment)
        return this.VideoTemplate;
      OutboundDocumentAttachment documentAttachment = item as OutboundDocumentAttachment;
      if (item is OutboundAlbumAttachment || item is OutboundProductAttachment || (item is OutboundMarketAlbumAttachment || item is OutboundUploadDocumentAttachment) || !string.IsNullOrEmpty(documentAttachment != null ? documentAttachment.Thumb : null))
        return this.GenericThumbTemplate;
      if (item is OutboundAddAttachment)
        return this.AddAttachmentTemplate;
      if (item is OutboundForwardedMessages)
        return this.ForwardedMessageTemplate;
      return this.GenericIconTemplate;
    }
  }
}
