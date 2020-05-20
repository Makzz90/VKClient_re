using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VKClient.Common.Emoji
{
    internal class SpriteCreatorHelper
    {
        public static SpriteDescription TryCreateSprite(int maxElementsCount, List<string> originalUris, List<string> elementCodes, string saveResultUri1, int columnsCount, int widthOfOneItemInPixels, int heightOfOneItemInPixels, int widthInPixels, int heightInPixels, SolidColorBrush backgroundColor1)
        {
            SpriteDescription spriteDescription = new SpriteDescription();
            try
            {
                spriteDescription.WidthInPixels = widthInPixels;
                spriteDescription.HeightInPixels = heightInPixels;
                spriteDescription.SpritePath = saveResultUri1;
                spriteDescription.Elements = new List<SpriteElementData>();
                WriteableBitmap writeableBitmap1 = new WriteableBitmap(widthInPixels, heightInPixels);
                WriteableBitmap writeableBitmap2 = writeableBitmap1;
                Canvas canvas = new Canvas();
                SolidColorBrush solidColorBrush = backgroundColor1;
                canvas.Background = (Brush)solidColorBrush;
                double num1 = (double)widthInPixels;
                canvas.Width = num1;
                double num2 = (double)heightInPixels;
                canvas.Height = num2;
                writeableBitmap2.Render((UIElement)canvas, null);
                writeableBitmap1.Invalidate();
                int num3 = (int)Math.Ceiling((double)maxElementsCount / (double)columnsCount);
                int num4 = widthInPixels / columnsCount;
                int num5 = heightInPixels / num3;
                int num6 = (num4 - widthOfOneItemInPixels) / 2;
                int num7 = (num5 - heightOfOneItemInPixels) / 2;
                int height = 0;
                for (int index = 0; index < originalUris.Count; ++index)
                {
                    int num8 = index % columnsCount * num4;
                    int num9 = index / columnsCount * num5;
                    string smallImageUri = originalUris[index];
                    SpriteCreatorHelper.WriteImageToBitmap(writeableBitmap1, smallImageUri, num8 + num6, num9 + num7, widthOfOneItemInPixels, heightOfOneItemInPixels);
                    height = num9 + num5;
                    SpriteElementData spriteElementData = new SpriteElementData()
                    {
                        Position = new Rect((double)num8, (double)num9, (double)num4, (double)num5),
                        ElementCode = elementCodes[index]
                    };
                    spriteDescription.Elements.Add(spriteElementData);
                }
                if (height < writeableBitmap1.PixelHeight - 1)
                {
                    writeableBitmap1 = writeableBitmap1.Crop(0, 0, writeableBitmap1.PixelWidth, height);
                    spriteDescription.HeightInPixels = height;
                }
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(saveResultUri1, FileMode.OpenOrCreate, FileAccess.Write))
                        writeableBitmap1.SaveJpeg((Stream)storageFileStream, spriteDescription.WidthInPixels, spriteDescription.HeightInPixels, 0, 90);
                }
            }
            catch
            {
                return null;
            }
            return spriteDescription;
        }

        private static void WriteImageToBitmap(WriteableBitmap resultWb1, string smallImageUri, int currentXOffset, int currentYOffset, int widthOfOneItemInPixels, int heightOfOneItemInPixels)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.CreateOptions = BitmapCreateOptions.None;
            bitmapImage.DecodePixelType = DecodePixelType.Physical;
            bitmapImage.DecodePixelWidth = widthOfOneItemInPixels;
            bitmapImage.DecodePixelHeight = heightOfOneItemInPixels;

            try
            {
                Stream stream = Application.GetResourceStream(new Uri(smallImageUri, UriKind.Relative)).Stream;
                bitmapImage.SetSource(stream);
                WriteableBitmap source = new WriteableBitmap((BitmapSource)bitmapImage);
                resultWb1.Blit(new Rect()
                {
                    X = (double)currentXOffset,
                    Y = (double)currentYOffset,
                    Width = (double)widthOfOneItemInPixels,
                    Height = (double)heightOfOneItemInPixels
                }, source, new Rect()
                {
                    Width = (double)widthOfOneItemInPixels,
                    Height = (double)heightOfOneItemInPixels
                });
            }
            catch
            {
            }
        }
    }
}
