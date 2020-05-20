namespace Mp3MediaStreamSource.Phone
{
  public interface IReportFileChunkDownloaded
  {
    void ReportDownloadedFileChunk(string fileId, string filePath, long byteRangeBeginning, long byteRangeEnd, bool isWholeFile);
  }
}
