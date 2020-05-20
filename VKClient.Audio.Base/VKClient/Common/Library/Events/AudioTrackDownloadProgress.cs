namespace VKClient.Common.Library.Events
{
    public sealed class AudioTrackDownloadProgress
    {
        public string Id { get; set; }

        public float Progress { get; set; }
    }
}
