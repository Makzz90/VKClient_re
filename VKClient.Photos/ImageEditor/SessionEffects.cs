using System;
using System.Collections.Generic;
using System.Linq;

namespace VKClient.Photos.ImageEditor
{
    public class SessionEffects
    {
        private List<ImageEffectsInfo> _effectsList = new List<ImageEffectsInfo>();

        public ImageEffectsInfo GetImageEffectsInfo(string albumId, int seqNo)
        {
            ImageEffectsInfo imageEffectsInfo = this._effectsList.FirstOrDefault<ImageEffectsInfo>((Func<ImageEffectsInfo, bool>)(e =>
            {
                if (e.AlbumId == albumId)
                    return e.SeqNo == seqNo;
                return false;
            }));
            if (imageEffectsInfo == null)
            {
                imageEffectsInfo = new ImageEffectsInfo()
                {
                    AlbumId = albumId,
                    SeqNo = seqNo
                };
                this._effectsList.Add(imageEffectsInfo);
            }
            return imageEffectsInfo;
        }

        public List<ImageEffectsInfo> GetApplied()
        {
            return this._effectsList.Where<ImageEffectsInfo>((Func<ImageEffectsInfo, bool>)(e => e.AppliedAny)).ToList<ImageEffectsInfo>();
        }
    }
}
