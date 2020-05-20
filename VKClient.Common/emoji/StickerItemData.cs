using System.IO;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Emoji
{
    public class StickerItemData : IBinarySerializable
    {
        public bool CanSendSticker { get; set; }

        public string LocalPath { get; set; }

        public string LocalPathBig { get; set; }

        public string LocalPathExtraBig { get; set; }

        public int StickerSetId { get; set; }

        public int StickerId { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(3);
            writer.WriteString(this.LocalPath);
            writer.WriteString(this.LocalPathBig);
            writer.Write(this.StickerSetId);
            writer.Write(this.StickerId);
            writer.WriteString(this.LocalPathExtraBig);
            writer.Write(this.CanSendSticker);
        }

        public void Read(BinaryReader reader)
        {
            int num1 = reader.ReadInt32();
            this.LocalPath = reader.ReadString();
            this.LocalPathBig = reader.ReadString();
            this.StickerSetId = reader.ReadInt32();
            this.StickerId = reader.ReadInt32();
            this.LocalPathExtraBig = reader.ReadString();
            int num2 = 3;
            if (num1 < num2)
                return;
            this.CanSendSticker = reader.ReadBoolean();
        }

        public Attachment CreateAttachment()
        {
            Attachment attachment = new Attachment();
            attachment.type = "sticker";
            Sticker sticker = new Sticker();
            sticker.height = 256;
            sticker.width = 256;
            long stickerId = (long)this.StickerId;
            sticker.id = stickerId;
            int stickerSetId = this.StickerSetId;
            sticker.product_id = stickerSetId;
            string localPathBig = this.LocalPathBig;
            sticker.photo_256 = localPathBig;
            attachment.sticker = sticker;
            return attachment;
        }

        public StickerItemData()
        {
            this.CanSendSticker = true;
        }
    }
}
