namespace VKClient.Audio.Base
{
  public class GetDialogsRequest
  {
    public int Offset { get; private set; }

    public int Count { get; private set; }

    public int PreviewLength { get; private set; }

    public GetDialogsRequest(int offset, int count, int previewLength)
    {
      this.Offset = offset;
      this.Count = count;
      this.PreviewLength = previewLength;
    }
  }
}
