using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library.Events
{
    public class AudioTrackAddedRemoved
    {
        public bool Added { get; set; }

        public AudioObj Audio { get; set; }

        public bool IsSavedAudiosAlbum { get; set; }//mod
    }
}
