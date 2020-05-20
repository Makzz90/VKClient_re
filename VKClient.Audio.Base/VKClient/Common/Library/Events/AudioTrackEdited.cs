namespace VKClient.Common.Library.Events
{
    public sealed class AudioTrackEdited
    {
        public long OwnerId { get; set; }

        public long Id { get; set; }

        public string Artist { get; set; }

        public string Title { get; set; }
    }
}
