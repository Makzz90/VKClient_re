using System;
using System.IO;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Backend.DataObjects
{
  public class Photo : IBinarySerializable
  {
    private string _text = "";
    private string _photo75;
    private string _photo130;
    private string _photo604;
    private string _photo807;
    private string _photo1280;
    private string _photo2560;

    public long id { get; set; }

    public long pid
    {
      get
      {
        return this.id;
      }
      set
      {
        this.id = value;
      }
    }

    public long album_id { get; set; }

    public long aid
    {
      get
      {
        return this.album_id;
      }
      set
      {
        this.album_id = value;
      }
    }

    public long owner_id { get; set; }

    public long user_id { get; set; }

    public string photo_75
    {
      get
      {
        if (!string.IsNullOrEmpty(this._photo75) && this._photo75.StartsWith("http"))
          return this._photo75 + this.WidthHeightInfo();
        return this._photo75;
      }
      set
      {
        this._photo75 = value;
      }
    }

    public string photo_130
    {
      get
      {
        if (!string.IsNullOrEmpty(this._photo130) && this._photo130.StartsWith("http"))
          return this._photo130 + this.WidthHeightInfo();
        return this._photo130;
      }
      set
      {
        this._photo130 = value;
      }
    }

    public string photo_604
    {
      get
      {
        if (!string.IsNullOrEmpty(this._photo604) && this._photo604.StartsWith("http"))
          return this._photo604 + this.WidthHeightInfo();
        return this._photo604;
      }
      set
      {
        this._photo604 = value;
      }
    }

    public string photo_807
    {
      get
      {
        if (!string.IsNullOrEmpty(this._photo807) && this._photo807.StartsWith("http"))
          return this._photo807 + this.WidthHeightInfo();
        return this._photo807;
      }
      set
      {
        this._photo807 = value;
      }
    }

    public string photo_1280
    {
      get
      {
        if (!string.IsNullOrEmpty(this._photo1280) && this._photo1280.StartsWith("http"))
          return this._photo1280 + this.WidthHeightInfo();
        return this._photo1280;
      }
      set
      {
        this._photo1280 = value;
      }
    }

    public string photo_2560
    {
      get
      {
        if (!string.IsNullOrEmpty(this._photo2560) && this._photo2560.StartsWith("http"))
          return this._photo2560 + this.WidthHeightInfo();
        return this._photo2560;
      }
      set
      {
        this._photo2560 = value;
      }
    }

    public string src
    {
      get
      {
        return this.photo_130;
      }
      set
      {
        this.photo_130 = value;
      }
    }

    public string src_big
    {
      get
      {
        return this.photo_604;
      }
      set
      {
        this.photo_604 = value;
      }
    }

    public string src_small
    {
      get
      {
        return this.photo_75;
      }
      set
      {
        this.photo_75 = value;
      }
    }

    public int width { get; set; }

    public int height { get; set; }

    public string text
    {
      get
      {
        return this._text;
      }
      set
      {
        this._text = (value ?? "").ForUI();
      }
    }

    public int created
    {
      get
      {
        return this.date;
      }
      set
      {
        this.date = value;
      }
    }

    public int date { get; set; }

    public string access_key { get; set; }

    public int real_offset { get; set; }

    public string src_xbig
    {
      get
      {
        return this.photo_807;
      }
      set
      {
        this.photo_807 = value;
      }
    }

    public string src_xxbig
    {
      get
      {
        return this.photo_1280;
      }
      set
      {
        this.photo_1280 = value;
      }
    }

    public Likes likes { get; set; }

    public Comments comments { get; set; }

    public int can_comment { get; set; }

    public Photo()
    {
      this.can_comment = 1;
      this.access_key = "";
    }

    private string WidthHeightInfo()
    {
      string str1 = "?wh=";
      int num = this.width;
      string str2 = num.ToString();
      string str3 = "_";
      num = this.height;
      string str4 = num.ToString();
      return str1 + str2 + str3 + str4;
    }

    public string GetAppropriateForScaleFactor(Size size)
    {
      int realScaleFactor = ScaleFactor.GetRealScaleFactor();
      // ISSUE: explicit reference operation
      double width = ((Size) @size).Width;
      // ISSUE: explicit reference operation
      double height = ((Size) @size).Height;
      double num1 = width * ((double) realScaleFactor / 100.0);
      double num2 = height * ((double) realScaleFactor / 100.0);
      double num3 = (double) this.width / (double) this.height;
      double val1 = num1 * num3;
      double val2 = num2 / num3;
      double num4 = num3 >= 1.0 ? (num3 <= 1.0 ? Math.Min(val1, val2) : (val2 > 0.0 ? val2 : val1)) : (val1 > 0.0 ? val1 : val2);
      if (num4 == 0.0)
        return "";
      string str = "";
      if (!string.IsNullOrEmpty(this.photo_75))
      {
        if (num4 <= 75.0)
          return this.photo_75;
        str = this.photo_75;
      }
      if (!string.IsNullOrEmpty(this.photo_130))
      {
        if (num4 <= 130.0)
          return this.photo_130;
        str = this.photo_130;
      }
      if (!string.IsNullOrEmpty(this.photo_604))
      {
        if (num4 <= 604.0)
          return this.photo_604;
        str = this.photo_604;
      }
      if (!string.IsNullOrEmpty(this.photo_807))
      {
        if (num4 <= 807.0)
          return this.photo_807;
        str = this.photo_807;
      }
      if (!string.IsNullOrEmpty(this.photo_1280))
      {
        if (num4 <= 1280.0)
          return this.photo_1280;
        str = this.photo_1280;
      }
      if (!string.IsNullOrEmpty(this.photo_2560))
      {
        if (num4 <= 2560.0)
          return this.photo_2560;
        str = this.photo_2560;
      }
      return str;
    }

    public string GetAppropriateForScaleFactor(double requiredHeight, int reduceSizeForLowMemoryDeviceFactor = 1)
    {
      int realScaleFactor = ScaleFactor.GetRealScaleFactor();
      double num1 = (double) this.width / (double) this.height;
      if (MemoryInfo.IsLowMemDevice && reduceSizeForLowMemoryDeviceFactor > 0)
        requiredHeight /= (double) reduceSizeForLowMemoryDeviceFactor;
      requiredHeight *= (double) realScaleFactor / 100.0;
      double val2 = requiredHeight * num1;
      double num2 = Math.Max(requiredHeight, val2);
      string str = "";
      if (!string.IsNullOrEmpty(this.photo_75))
      {
        if (num2 <= 75.0)
          return this.photo_75;
        str = this.photo_75;
      }
      if (!string.IsNullOrEmpty(this.photo_130))
      {
        if (num2 <= 130.0)
          return this.photo_130;
        str = this.photo_130;
      }
      if (!string.IsNullOrEmpty(this.photo_604))
      {
        if (num2 <= 604.0)
          return this.photo_604;
        str = this.photo_604;
      }
      if (!string.IsNullOrEmpty(this.photo_807))
      {
        if (num2 <= 807.0)
          return this.photo_807;
        str = this.photo_807;
      }
      if (!string.IsNullOrEmpty(this.photo_1280))
      {
        if (num2 <= 1280.0)
          return this.photo_1280;
        str = this.photo_1280;
      }
      if (!string.IsNullOrEmpty(this.photo_2560))
      {
        if (num2 <= 2560.0)
          return this.photo_2560;
        str = this.photo_2560;
      }
      return str;
    }

    public override string ToString()
    {
      return string.Format("photo{0}_{1}", this.owner_id, this.id);
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.pid);
      writer.Write(this.aid);
      writer.Write(this.owner_id);
      writer.WriteString(this.src);
      writer.WriteString(this.src_big);
      writer.WriteString(this.src_small);
      writer.Write(this.width);
      writer.Write(this.height);
      writer.WriteString(this.text);
      writer.Write(this.created);
      writer.WriteString(this.access_key);
      writer.Write(this.user_id);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.pid = reader.ReadInt64();
      this.aid = reader.ReadInt64();
      this.owner_id = reader.ReadInt64();
      this.src = reader.ReadString();
      this.src_big = reader.ReadString();
      this.src_small = reader.ReadString();
      this.width = reader.ReadInt32();
      this.height = reader.ReadInt32();
      this.text = reader.ReadString();
      this.created = reader.ReadInt32();
      this.access_key = reader.ReadString();
      this.owner_id = reader.ReadInt64();
    }
  }
}
