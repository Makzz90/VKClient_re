using System;
using System.IO;
using VKClient.Common.Backend;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.Utils
{
    public static class StreamUtils
    {
        public static void CopyStream(Stream input, Stream output, Action<double> progressCallback = null, Cancellation c = null, long inputLength = 0)
        {
            if (inputLength == 0L)
            {
                try
                {
                    inputLength = input.Length;
                }
                catch (Exception)
                {
                }
            }
            byte[] buffer = new byte[32768];
            int num = 0;
            int count;
            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (!output.CanWrite)
                    throw new Exception("failed to copy stream");
                if (c != null && c.IsSet)
                    throw new Exception("CopyStream cancelled");
                output.Write(buffer, 0, count);
                num += count;
                if (progressCallback != null && inputLength > 0L)
                    progressCallback((double)num * 100.0 / (double)inputLength);
            }
        }

        public static MemoryStream ReadFully(Stream input)
        {
            byte[] buffer = new byte[16384];
            MemoryStream memoryStream = new MemoryStream();
            int count;
            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                memoryStream.Write(buffer, 0, count);
            memoryStream.Position = 0L;
            return memoryStream;
        }

        public static byte[] ReadFullyToByteArray(Stream input)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                input.CopyTo((Stream)memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static T CloneThroughSerialize<T>(T bs) where T : IBinarySerializable, new()
        {
            T instance = Activator.CreateInstance<T>();
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter((Stream)memoryStream);
            bs.Write(writer);
            long num = 0;
            memoryStream.Position = num;
            BinaryReader reader = new BinaryReader((Stream)memoryStream);
            instance.Read(reader);
            return instance;
        }
    }
}
