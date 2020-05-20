namespace XamlAnimatedGif.Decoding
{
  internal struct GifColor
  {
      public byte R;// { get; private set; }

      public byte G;// { get; private set; }

      public byte B;// { get; private set; }

    internal GifColor(byte r, byte g, byte b)
    {
      this.R = r;
      this.G = g;
      this.B = b;
    }

    public override string ToString()
    {
      return string.Format("#{0:x2}{1:x2}{2:x2}", this.R, this.G, this.B);
    }
  }
}
