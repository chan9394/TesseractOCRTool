using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Text;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace OCR_IdentityCard.Common
{
    public class TesseractTools
    {
        //static Tesseract _ocr;//识别引擎对象
        static string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "tessdata\\";//语言包位置
        /// <summary> 实例化识别引擎对象
        /// 实例化识别引擎对象
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        //static bool InitOcr(string lang)
        //{
        //    try
        //    {
        //        if (_ocr != null)
        //        {
        //            _ocr.Dispose();
        //            _ocr = null;
        //        }
        //        _ocr = new Tesseract(path, lang, OcrEngineMode.Default);
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        _ocr = null;
        //        NLogerHelper.Error(e.Message, "Failed to initialize tesseract OCR engine");
        //        return false;
        //    }
        //}
        /// <summary>识别MAT对象文本
        /// 识别MAT对象文本
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        static public string OcrImage(Mat source,string lang)
        {
            try {
                var _ocr = new Tesseract(path, lang, OcrEngineMode.Default);
                Mat result = new Mat();
                string ocredText = OcrImage(_ocr, source, result);
                NLogerHelper.Info("ocredText:" + ocredText);//识别出来的文本
                //NLogerHelper.Info("hocrTextBox:" + _ocr.GetHOCRText());//xml
                return TxtUtil.ReadClearContent(ocredText);
            } catch (Exception ex)
            {
                NLogerHelper.Error(ex.Message, "Failed to initialize tesseract OCR engine");
                return string.Empty;
            }            
        }
        /// <summary> 识别MAT对象文本
        /// 识别MAT对象文本
        /// </summary>
        /// <param name="ocr"></param>
        /// <param name="image"></param>
        /// <param name="imageColor"></param>
        /// <returns></returns>
        static string OcrImage(Tesseract ocr, Mat image, Mat imageColor)
        {
            try
            {
                Bgr drawCharColor = new Bgr(Color.Red);

                if (image.NumberOfChannels == 1)
                    CvInvoke.CvtColor(image, imageColor, ColorConversion.Gray2Bgr);
                else
                    image.CopyTo(imageColor);

                #region Perform a full page OCR
                ocr.SetImage(imageColor);
                if (ocr.Recognize() != 0)
                    throw new Exception("Failed to recognizer image");
                Tesseract.Character[] characters = ocr.GetCharacters();               
                //if (characters.Length == 0)
                //{
                //    Mat imgGrey = new Mat();
                //    CvInvoke.CvtColor(image, imgGrey, ColorConversion.Bgr2Gray);
                //    Mat imgThresholded = new Mat();
                //    CvInvoke.Threshold(imgGrey, imgThresholded, 65, 255, ThresholdType.Binary);
                //    ocr.SetImage(imgThresholded);
                //    // 这里一直有异常
                //    characters = ocr.GetCharacters();
                //    imageColor = imgThresholded;
                //    if (characters.Length == 0)
                //    {
                //        CvInvoke.Threshold(image, imgThresholded, 190, 255, ThresholdType.Binary);
                //        ocr.SetImage(imgThresholded);
                //        characters = ocr.GetCharacters();
                //        imageColor = imgThresholded;
                //    }
                //}
                foreach (Tesseract.Character c in characters)
                {
                  
                      CvInvoke.Rectangle(imageColor, c.Region, drawCharColor.MCvScalar);
                }
                return ocr.GetUTF8Text();
                #endregion

                #region Detect the text region before applying OCR.
                //bool checkInvert = true;
                //Rectangle[] regions;

                //using (
                //   ERFilterNM1 er1 = new ERFilterNM1("trained_classifierNM1.xml", 8, 0.00025f, 0.13f, 0.4f, true, 0.1f))
                //using (ERFilterNM2 er2 = new ERFilterNM2("trained_classifierNM2.xml", 0.3f))
                //{
                //    int channelCount = image.NumberOfChannels;
                //    UMat[] channels = new UMat[checkInvert ? channelCount * 2 : channelCount];

                //    for (int i = 0; i < channelCount; i++)
                //    {
                //        UMat c = new UMat();
                //        CvInvoke.ExtractChannel(image, c, i);
                //        channels[i] = c;
                //    }

                //    if (checkInvert)
                //    {
                //        for (int i = 0; i < channelCount; i++)
                //        {
                //            UMat c = new UMat();
                //            CvInvoke.BitwiseNot(channels[i], c);
                //            channels[i + channelCount] = c;
                //        }
                //    }

                //    VectorOfERStat[] regionVecs = new VectorOfERStat[channels.Length];
                //    for (int i = 0; i < regionVecs.Length; i++)
                //        regionVecs[i] = new VectorOfERStat();

                //    try
                //    {
                //        for (int i = 0; i < channels.Length; i++)
                //        {
                //            er1.Run(channels[i], regionVecs[i]);
                //            er2.Run(channels[i], regionVecs[i]);
                //        }
                //        using (VectorOfUMat vm = new VectorOfUMat(channels))
                //        {
                //            regions = ERFilter.ERGrouping(image, vm, regionVecs, ERFilter.GroupingMethod.OrientationHoriz,
                //               "trained_classifier_erGrouping.xml", 0.5f);
                //        }
                //    }
                //    finally
                //    {
                //        foreach (UMat tmp in channels)
                //            if (tmp != null)
                //                tmp.Dispose();
                //        foreach (VectorOfERStat tmp in regionVecs)
                //            if (tmp != null)
                //                tmp.Dispose();
                //    }

                //    Rectangle imageRegion = new Rectangle(Point.Empty, imageColor.Size);
                //    for (int i = 0; i < regions.Length; i++)
                //    {
                //        Rectangle r = ScaleRectangle(regions[i], 1.1);

                //        r.Intersect(imageRegion);
                //        regions[i] = r;
                //    }

                //}
                //List<Tesseract.Character> allChars = new List<Tesseract.Character>();
                //String allText = String.Empty;
                //foreach (Rectangle rect in regions)
                //{
                //    using (Mat region = new Mat(image, rect))
                //    {
                //        ocr.SetImage(region);
                //        if (ocr.Recognize() != 0)
                //            throw new Exception("Failed to recognize image");
                //        Tesseract.Character[] characters = ocr.GetCharacters();

                //        //convert the coordinates from the local region to global
                //        for (int i = 0; i < characters.Length; i++)
                //        {
                //            Rectangle charRegion = characters[i].Region;
                //            charRegion.Offset(rect.Location);
                //            characters[i].Region = charRegion;

                //        }
                //        allChars.AddRange(characters);

                //        allText += ocr.GetUTF8Text() + Environment.NewLine;

                //    }
                //}
                //Bgr drawRegionColor = new Bgr(Color.Red);
                //foreach (Rectangle rect in regions)
                //{
                //    CvInvoke.Rectangle(imageColor, rect, drawRegionColor.MCvScalar);
                //}
                //foreach (Tesseract.Character c in allChars)
                //{
                //    CvInvoke.Rectangle(imageColor, c.Region, drawCharColor.MCvScalar);
                //}

                //return allText;
                #endregion
            }
            catch(Exception ex)
            {
                return string.Empty;
            }
            
        }

        static Rectangle ScaleRectangle(Rectangle r, double scale)
        {
            double centerX = r.Location.X + r.Width / 2.0;
            double centerY = r.Location.Y + r.Height / 2.0;
            double newWidth = Math.Round(r.Width * scale);
            double newHeight = Math.Round(r.Height * scale);
            return new Rectangle((int)Math.Round(centerX - newWidth / 2.0), (int)Math.Round(centerY - newHeight / 2.0),
               (int)newWidth, (int)newHeight);
        }
    }
}