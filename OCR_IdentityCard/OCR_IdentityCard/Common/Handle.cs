using Emgu.CV;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MsiFindPic;
using System.Drawing;

namespace OCR_IdentityCard.Common
{
    public class Handle
    {
       static public Point ContrastPoint(Bitmap b1, Bitmap b2,int value)
        {
            var p = FindPic.GetImageContains(b1,b2,value);
            NLogerHelper.Info(p.X+"-"+p.Y);
            return p;
        }
        static public string ContrastString(string b1, string b2, int value)
        {
            var p = FindPic.GetImageContainsStr(b1, b2, value);
            return p;
        }
    }
}