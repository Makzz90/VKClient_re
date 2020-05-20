using ExifLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Common.Framework
{
    public static class ImagePreprocessor
    {
        public static Rect GetThumbnailRect(double actualWidth, double actualHeight, Rect relativeThumbRect)
        {
            Size childSize = new Size(actualWidth, actualHeight);
            return RectangleUtils.CalculateRelative(RectangleUtils.GetSize(RectangleUtils.ResizeToFitIfNotContained(new Size((double)VKConstants.MAX_PHOTO_WIDTH, (double)VKConstants.MAX_PHOTO_HEIGHT), childSize)), relativeThumbRect);
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16384];
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count;
                while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                    memoryStream.Write(buffer, 0, count);
                return memoryStream.ToArray();
            }
        }

        public static void PreprocessImage(Stream stream, int desiredSizeInPx, bool closeStream, Action<ImagePreprocessResult> completionCallback)
        {
            Stopwatch.StartNew();
            try
            {
                MemoryStream exifStream;
                ImagePreprocessor.PatchAwayExif(stream, out exifStream);
                long position = stream.Position;
                bool keepExif = AppGlobalStateManager.Current.GlobalState.SaveLocationDataOnUpload;
                stream.Position = 0L;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(stream);
                WriteableBitmap wb = new WriteableBitmap((BitmapSource)bitmapImage);
                ExifReader reader = (ExifReader)null;
                stream.Position = 0L;
                reader = new ExifReader(stream);
                bool flag = false;
                switch (reader.info.Orientation)
                {
                    case ExifOrientation.BottomRight:
                        wb = wb.Rotate(180);
                        flag = true;
                        break;
                    case ExifOrientation.TopRight:
                        wb = wb.Rotate(90);
                        flag = true;
                        break;
                    case ExifOrientation.BottomLeft:
                        wb = wb.Rotate(270);
                        flag = true;
                        break;
                }
                int pixelWidth = wb.PixelWidth;
                int pixelHeight = wb.PixelHeight;
                int num1 = pixelWidth * pixelHeight;
                if (desiredSizeInPx > num1)
                    desiredSizeInPx = num1;
                if (num1 > desiredSizeInPx | flag)
                {
                    MemoryStream ms = new MemoryStream();
                    double num2 = Math.Sqrt((double)num1 / (double)desiredSizeInPx);
                    int resizedWidth = (int)Math.Round((double)pixelWidth / num2);
                    int resizedHeight = (int)Math.Round((double)pixelHeight / num2);
                    ThreadPool.QueueUserWorkItem((WaitCallback)(o =>
                    {
                        try
                        {
                            Stream resultStream;
                            wb.SaveJpeg((Stream)ms, resizedWidth, resizedHeight, 0, VKConstants.JPEGQUALITY);
                            ms.Position = 0;
                            if (keepExif)
                            {
                                exifStream = new MemoryStream(ImagePreprocessor.ResetOrientation(reader.info.OrientationOffset, exifStream.ToArray(), reader.info.LittleEndian));
                                resultStream = ImagePreprocessor.MergeExif((Stream)ms, exifStream);
                                Logger.Instance.Info("RESIZED JPEG SIZE: " + (object)resultStream.Length);
                                ms.Close();
                            }
                            else
                                resultStream = ms;
                            if (closeStream)
                                stream.Close();
                            exifStream.Close();
                            resultStream.Position = 0L;
                            GC.Collect();
                            completionCallback(new ImagePreprocessResult((Stream)resultStream, resizedWidth, resizedHeight));
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Error("Failed to resize image", ex);
                        }
                    }));
                }
                else
                {
                    MemoryStream resultStream = new MemoryStream();
                    if (keepExif)
                    {
                        stream.Position = 0;
                        stream.CopyTo((Stream)resultStream);
                        if (closeStream)
                            stream.Close();
                    }
                    else
                    {
                        stream.Position = 0;
                        resultStream.WriteByte((byte)stream.ReadByte());
                        resultStream.WriteByte((byte)stream.ReadByte());
                        stream.Position = position;
                        stream.CopyTo((Stream)resultStream);
                    }
                    if (closeStream)
                        stream.Close();
                    exifStream.Close();
                    resultStream.Position = 0;
                    GC.Collect();
                    completionCallback(new ImagePreprocessResult((Stream)resultStream, pixelWidth, pixelHeight));
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Failed to resize image", ex);
            }
        }

        public static byte[] ResetOrientation(long p, byte[] exifData, bool littleEndian)
        {
            byte[] array = new List<byte>((IEnumerable<byte>)exifData).ToArray();
            long num = p - 2L;
            if (num >= 0L && num < (long)(array.Length - 1))
                ExifIO.WriteUShort(array, (int)num, littleEndian, (ushort)1);
            return array;
        }

        public static MemoryStream MergeExif(Stream ms, MemoryStream exifStream)
        {
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.WriteByte((byte)ms.ReadByte());
            memoryStream.WriteByte((byte)ms.ReadByte());
            exifStream.WriteTo((Stream)memoryStream);
            ms.CopyTo((Stream)memoryStream);
            memoryStream.Position = 0L;
            return memoryStream;
        }

        public static void PatchAwayExif(Stream inStream, out MemoryStream exifStream)
        {
            exifStream = new MemoryStream();
            byte[] numArray = new byte[2]
      {
        (byte) inStream.ReadByte(),
        (byte) inStream.ReadByte()
      };
            if ((int)numArray[0] != (int)byte.MaxValue || (int)numArray[1] != 216)
                return;
            ImagePreprocessor.SkipAppHeaderSection(inStream, out exifStream);
        }

        private static byte[] SkipAppHeaderSection(Stream inStream, out MemoryStream exifStream)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            byte[] numArray = new byte[2]
      {
        (byte) inStream.ReadByte(),
        (byte) inStream.ReadByte()
      };
            exifStream = new MemoryStream();
            while ((int)numArray[0] == (int)byte.MaxValue && (int)numArray[1] >= 224 && (int)numArray[1] <= 239)
            {
                exifStream.WriteByte(numArray[0]);
                exifStream.WriteByte(numArray[1]);
                byte num1 = (byte)inStream.ReadByte();
                byte num2 = (byte)inStream.ReadByte();
                exifStream.WriteByte(num1);
                exifStream.WriteByte(num2);
                byte[] buffer = new byte[((int)num1 << 8 | (int)num2) - 2];
                inStream.Read(buffer, 0, buffer.Length);
                exifStream.Write(buffer, 0, buffer.Length);
                numArray[0] = (byte)inStream.ReadByte();
                numArray[1] = (byte)inStream.ReadByte();
            }
            inStream.Position -= 2L;
            exifStream.Position = 0L;
            stopwatch.Stop();
            return numArray;
        }
    }
}
