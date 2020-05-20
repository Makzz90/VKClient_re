namespace VKClient.Common.UC.InplaceGifViewer
{
  public class GifStateChanged
  {
    public InplaceGifViewerViewModel.State NewState { get; set; }

    public long ID { get; set; }

    public int VMHashcode { get; set; }
  }
}
