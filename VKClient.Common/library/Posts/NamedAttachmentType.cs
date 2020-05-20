namespace VKClient.Common.Library.Posts
{
  public class NamedAttachmentType
  {
    public AttachmentType AttachmentType { get; set; }

    public string Name { get; set; }

    public override string ToString()
    {
      return this.Name;
    }
  }
}
