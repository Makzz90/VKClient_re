using System;
using System.Collections.Generic;
using System.Linq;

namespace VKClient.Common.Utils
{
  public class ThumbnailLayoutManager
  {
    public static void ProcessThumbnails(double maxW, double maxH, List<ThumbAttachment> thumbs, double marginBetween)
    {
      string str = "";
      int[] numArray = new int[3];
      List<double> doubleList1 = new List<double>();
      int count = thumbs.Count;
      bool flag = false;
      foreach (ThumbAttachment thumb in thumbs)
      {
        double ratio = thumb.getRatio();
        if (ratio == -1.0)
          flag = true;
        char orient = ratio > 1.2 ? 'w' : (ratio < 0.8 ? 'n' : 'q');
        str += orient.ToString();
        ++numArray[ThumbnailLayoutManager.oi(orient)];
        doubleList1.Add(ratio);
      }
      if (flag)
      {
        ThumbnailLayoutManager.Log("BAD!");
      }
      else
      {
        double num1 = doubleList1.Count > 0 ? Enumerable.Sum((IEnumerable<double>) doubleList1) / (double) doubleList1.Count : 1.0;
        double margin = marginBetween;
        double num2 = marginBetween;
        double width1;
        double num3;
        if (maxW > 0.0)
        {
          width1 = maxW;
          num3 = maxH;
        }
        else
        {
          width1 = 320.0;
          num3 = 210.0;
        }
        double num4 = width1 / num3;
        if (count == 1)
        {
          if (doubleList1[0] > 0.8)
            thumbs[0].SetViewSize(width1, width1 / doubleList1[0], false, false);
          else
            thumbs[0].SetViewSize(num3 * doubleList1[0], num3, false, false);
        }
        else if (count == 2)
        {
          if (str == "ww" && num1 > 1.4 * num4 && doubleList1[1] - doubleList1[0] < 0.2)
          {
            double width2 = width1;
            double height = Math.Min(width2 / doubleList1[0], Math.Min(width2 / doubleList1[1], (num3 - num2) / 2.0));
            thumbs[0].SetViewSize(width2, height, true, false);
            thumbs[1].SetViewSize(width2, height, false, false);
          }
          else if (str == "ww" || str == "qq")
          {
            double width2 = (width1 - margin) / 2.0;
            double height = Math.Min(width2 / doubleList1[0], Math.Min(width2 / doubleList1[1], num3));
            thumbs[0].SetViewSize(width2, height, false, false);
            thumbs[1].SetViewSize(width2, height, false, false);
          }
          else
          {
            double width2 = (width1 - margin) / doubleList1[1] / (1.0 / doubleList1[0] + 1.0 / doubleList1[1]);
            double width3 = width1 - width2 - margin;
            double height = Math.Min(num3, Math.Min(width2 / doubleList1[0], width3 / doubleList1[1]));
            thumbs[0].SetViewSize(width2, height, false, false);
            thumbs[1].SetViewSize(width3, height, false, false);
          }
        }
        else if (count == 3)
        {
          if (str == "www")
          {
            double width2 = width1;
            double height1 = Math.Min(width2 / doubleList1[0], (num3 - num2) * 0.66);
            thumbs[0].SetViewSize(width2, height1, true, false);
            double width3 = (width1 - margin) / 2.0;
            double height2 = Math.Min(num3 - height1 - num2, Math.Min(width3 / doubleList1[1], width3 / doubleList1[2]));
            thumbs[1].SetViewSize(width3, height2, false, false);
            thumbs[2].SetViewSize(width3, height2, false, false);
          }
          else
          {
            double height1 = num3;
            double width2 = Math.Min(height1 * doubleList1[0], (width1 - margin) * 0.75);
            thumbs[0].SetViewSize(width2, height1, false, false);
            double height2 = doubleList1[1] * (num3 - num2) / (doubleList1[2] + doubleList1[1]);
            double height3 = num3 - height2 - num2;
            double width3 = Math.Min(width1 - width2 - margin, Math.Min(height2 * doubleList1[2], height3 * doubleList1[1]));
            thumbs[1].SetViewSize(width3, height3, false, true);
            thumbs[2].SetViewSize(width3, height2, false, true);
          }
        }
        else if (count == 4)
        {
          if (str == "wwww")
          {
            double width2 = width1;
            double height1 = Math.Min(width2 / doubleList1[0], (num3 - num2) * 0.66);
            thumbs[0].SetViewSize(width2, height1, true, false);
            double val2 = (width1 - 2.0 * margin) / (doubleList1[1] + doubleList1[2] + doubleList1[3]);
            double width3 = val2 * doubleList1[1];
            double width4 = val2 * doubleList1[2];
            double width5 = val2 * doubleList1[3];
            double height2 = Math.Min(num3 - height1 - num2, val2);
            thumbs[1].SetViewSize(width3, height2, false, false);
            thumbs[2].SetViewSize(width4, height2, false, false);
            thumbs[3].SetViewSize(width5, height2, false, false);
          }
          else
          {
            double height1 = num3;
            double width2 = Math.Min(height1 * doubleList1[0], (width1 - margin) * 0.66);
            thumbs[0].SetViewSize(width2, height1, false, false);
            double val2 = (num3 - 2.0 * num2) / (1.0 / doubleList1[1] + 1.0 / doubleList1[2] + 1.0 / doubleList1[3]);
            double height2 = val2 / doubleList1[1];
            double height3 = val2 / doubleList1[2];
            double height4 = val2 / doubleList1[3];
            double width3 = Math.Min(width1 - width2 - margin, val2);
            thumbs[1].SetViewSize(width3, height2, false, true);
            thumbs[2].SetViewSize(width3, height3, false, true);
            thumbs[3].SetViewSize(width3, height4, false, true);
          }
        }
        else
        {
          List<double> doubleList2 = new List<double>();
          if (num1 > 1.1)
          {
            foreach (double val2 in doubleList1)
              doubleList2.Add(Math.Max(1.0, val2));
          }
          else
          {
            foreach (double val2 in doubleList1)
              doubleList2.Add(Math.Min(1.0, val2));
          }
          Dictionary<string, List<double>> dictionary = new Dictionary<string, List<double>>();
          int num5;
          dictionary[string.Concat((num5 = count))] = new List<double>()
          {
            ThumbnailLayoutManager.calculateMultiThumbsHeight(doubleList2, width1, margin)
          };
          for (int index = 1; index <= count - 1; ++index)
          {
            int num6;
            dictionary[index.ToString() + "," + (num6 = count - index)] = new List<double>()
            {
              ThumbnailLayoutManager.calculateMultiThumbsHeight(doubleList2.Sublist<double>(0, index), width1, margin),
              ThumbnailLayoutManager.calculateMultiThumbsHeight(doubleList2.Sublist<double>(index, doubleList2.Count), width1, margin)
            };
          }
          for (int index1 = 1; index1 <= count - 2; ++index1)
          {
            for (int index2 = 1; index2 <= count - index1 - 1; ++index2)
            {
              int num6;
              dictionary[index1.ToString() + "," + index2 + "," + (num6 = count - index1 - index2)] = new List<double>()
              {
                ThumbnailLayoutManager.calculateMultiThumbsHeight(doubleList2.Sublist<double>(0, index1), width1, margin),
                ThumbnailLayoutManager.calculateMultiThumbsHeight(doubleList2.Sublist<double>(index1, index1 + index2), width1, margin),
                ThumbnailLayoutManager.calculateMultiThumbsHeight(doubleList2.Sublist<double>(index1 + index2, doubleList2.Count), width1, margin)
              };
            }
          }
          string index3 =  null;
          double num7 = 0.0;
          foreach (string key in dictionary.Keys)
          {
            List<double> doubleList3 = dictionary[key];
            double num6 = num2 * (double) (doubleList3.Count - 1);
            foreach (double num8 in doubleList3)
              num6 += num8;
            double num9 = Math.Abs(num6 - num3);
            if (key.IndexOf(",") != -1)
            {
              string[] strArray = key.Split(',');
              if (int.Parse(strArray[0]) > int.Parse(strArray[1]) || strArray.Length > 2 && int.Parse(strArray[1]) > int.Parse(strArray[2]))
                num9 *= 1.1;
            }
            if (index3 == null || num9 < num7)
            {
              index3 = key;
              num7 = num9;
            }
          }
          List<ThumbAttachment> thumbAttachmentList1 = new List<ThumbAttachment>((IEnumerable<ThumbAttachment>) thumbs);
          List<double> doubleList4 = new List<double>((IEnumerable<double>) doubleList2);
          string[] strArray1 = index3.Split(',');
          List<double> doubleList5 = dictionary[index3];
          int length = strArray1.Length;
          int index4 = 0;
          for (int index1 = 0; index1 < strArray1.Length; ++index1)
          {
            int num6 = int.Parse(strArray1[index1]);
            List<ThumbAttachment> thumbAttachmentList2 = new List<ThumbAttachment>();
            for (int index2 = 0; index2 < num6; ++index2)
            {
              thumbAttachmentList2.Add(thumbAttachmentList1[0]);
              thumbAttachmentList1.RemoveAt(0);
            }
            double num8 = doubleList5[index4];
            ++index4;
            int num9 = thumbAttachmentList2.Count - 1;
            for (int index2 = 0; index2 < thumbAttachmentList2.Count; ++index2)
            {
              ThumbAttachment thumbAttachment = thumbAttachmentList2[index2];
              double num10 = doubleList4[0];
              doubleList4.RemoveAt(0);
              double width2 = num10 * num8;
              double height = num8;
              int num11 = index2 == num9 ? 1 : 0;
              int num12 = 0;
              thumbAttachment.SetViewSize(width2, height, num11 != 0, num12 != 0);
            }
          }
        }
      }
    }

    private static double calculateMultiThumbsHeight(List<double> ratios, double width, double margin)
    {
      return (width - (double) (ratios.Count - 1) * margin) / Enumerable.Sum((IEnumerable<double>) ratios);
    }

    private static void Log(string p)
    {
    }

    private static int oi(char orient)
    {
      if ((int) orient == 110)
        return 1;
      if ((int) orient == 113)
        return 2;
      return 0;
    }
  }
}
