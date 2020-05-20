namespace VKClient.Audio.Base.AudioCache
{
  public class FileDownloadRequest
  {
    public string Uri { get; set; }

    public long FromByte { get; set; }

    public long ToByte { get; set; }

    public string LocalPath { get; set; }
  }
}
