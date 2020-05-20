using System;
using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.AudioCache
{
  public class InProgressDownloadInfo : IBinarySerializable
  {
    public RemoteFileInfo RemoteFile { get; set; }

    public long DownloadedToByte { get; set; }

    public string LocalFileId { get; set; }

    public long Length { get; set; }

    public DownloadResult LastDownloadResult { get; set; }

    public InProgressDownloadInfo()
    {
      this.LastDownloadResult = DownloadResult.OK;
      this.DownloadedToByte = -1L;
      this.Length = -1L;
    }

    public InProgressDownloadInfo(RemoteFileInfo rfi)
      : this()
    {
      this.RemoteFile = rfi;
      this.LocalFileId = this.RemoteFile.UniqueId + "_" + Guid.NewGuid().ToString();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write<RemoteFileInfo>(this.RemoteFile, false);
      writer.Write(this.DownloadedToByte);
      writer.WriteString(this.LocalFileId);
      writer.Write(this.Length);
      writer.Write((int) this.LastDownloadResult);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.RemoteFile = reader.ReadGeneric<RemoteFileInfo>();
      this.DownloadedToByte = reader.ReadInt64();
      this.LocalFileId = reader.ReadString();
      this.Length = reader.ReadInt64();
      this.LastDownloadResult = (DownloadResult) reader.ReadInt32();
    }

    public override string ToString()
    {
      return string.Format("{{InProgressDownload id={0}, uri={1}, downloadedToByte={2}, length={3}}} ", this.RemoteFile.UniqueId, this.RemoteFile.UriStr, this.DownloadedToByte, this.Length);
    }
  }
}
