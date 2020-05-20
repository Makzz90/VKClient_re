using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;

namespace VKClient.Common.Emoji
{
    public class SpriteItem : VirtualizableItemBase, ISupportOrientationChange
    {
        private SpriteDescription _vertSpriteDescription;
        private SpriteDescription _horSpriteDescription;
        private bool _isHorizontal;
        private double _verticalWidth;
        private double _horizontalWidth;
        private double _verticalHeight;
        private double _horizontalHeight;
        //private string _verticalSpritePath;
        //private string _horizontalSpritePath;

        public bool IsHorizontal
        {
            get
            {
                return this._isHorizontal;
            }
            set
            {
                this._isHorizontal = value;
                this.Width = this._isHorizontal ? this._horizontalWidth : this._verticalWidth;
            }
        }

        private SpriteDescription CurrentDesc
        {
            get
            {
                if (!this._isHorizontal)
                    return this._vertSpriteDescription;
                return this._horSpriteDescription;
            }
        }

        public override double FixedHeight
        {
            get
            {
                if (!this._isHorizontal)
                    return this._verticalHeight;
                return this._horizontalHeight;
            }
        }

        public SpriteItem(SpriteDescription vertSpriteDesc, SpriteDescription horSpriteDesc, bool isHorizontal)
            : base(0.0)
        {
            this._vertSpriteDescription = vertSpriteDesc;
            this._horSpriteDescription = horSpriteDesc;
            this._verticalWidth = (double)vertSpriteDesc.WidthInPixels * 100.0 / (double)ScaleFactor.GetRealScaleFactor();
            this._horizontalWidth = (double)horSpriteDesc.WidthInPixels * 100.0 / (double)ScaleFactor.GetRealScaleFactor();
            this._verticalHeight = (double)vertSpriteDesc.HeightInPixels * 100.0 / (double)ScaleFactor.GetRealScaleFactor();
            this._horizontalHeight = (double)horSpriteDesc.HeightInPixels * 100.0 / (double)ScaleFactor.GetRealScaleFactor();
            this._isHorizontal = isHorizontal;
            this.Width = this._isHorizontal ? this._horizontalWidth : this._verticalWidth;
        }

        protected override void GenerateChildren()
        {
            base.GenerateChildren();
            Image image1 = new Image();
            double width = this.Width;
            image1.Width = width;
            double fixedHeight = this.FixedHeight;
            image1.Height = fixedHeight;
            Image image2 = image1;
            image2.Tap += new EventHandler<GestureEventArgs>(this.image_Tap);
            IsoStoreImageSource.SetIsoStoreFileName((UIElement)image2, this._isHorizontal ? this._horSpriteDescription.SpritePath : this._vertSpriteDescription.SpritePath);
            this.Children.Add((FrameworkElement)image2);
        }

        private void image_Tap(object sender, GestureEventArgs e)
        {
            Point position = e.GetPosition(sender as UIElement);
            SpriteElementData elementByRelativePoint = this.CurrentDesc.GetElementByRelativePoint(new Point(position.X / this.Width, position.Y / this.FixedHeight));
            if (elementByRelativePoint == null)
                return;
            EventAggregator.Current.Publish((object)new SpriteElementTapEvent()
            {
                Data = elementByRelativePoint
            });
        }

        public void SetIsHorizontal(bool isHorizontal)
        {
            this.IsHorizontal = isHorizontal;
        }
    }
}
