using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Web;

namespace OCR_IdentityCard.Common
{
    public class ImageHandle
    {
        // 标准 灰度值
        static int grayNum = 55;
        static bool Debug = Convert.ToBoolean(ConfigurationManager.AppSettings["Debug"].ToString());
        static Size imgSize = new Size(Convert.ToInt32(ConfigurationManager.AppSettings["SizeW"].ToString()), Convert.ToInt32(ConfigurationManager.AppSettings["SizeH"].ToString()));
        #region  操作单张图片函数
        /// <summary>
        ///  操作单张图片函数
        /// </summary>
        /// <param name="imgPath">图片路径</param>
        /// <param name="operateImgName">存放文件夹</param>
        public static Info frontActionn(string imgPath, string operateImgName)
        {
            Info info = new Info();
            try
            {
                NLogerHelper.Info("a1");
                //原图图像对象，留给切割使用
                Image<Bgr, byte> IMG = new Image<Bgr, byte>(imgPath);
                NLogerHelper.Info("a2");
                IMG = ResizeImage(IMG.Bitmap, imgSize);
                NLogerHelper.Info("a3");
                //操作图像对象，做图片处理使用
                Image<Bgr, byte> img = new Image<Bgr, byte>(imgPath);
                NLogerHelper.Info("a4");
                var originalimg = ResizeImage(img.Bitmap, imgSize);
                NLogerHelper.Info("a5");
                //2.灰度化图片
                originalimg.Convert<Gray, Byte>().Convert<Gray, double>();
                NLogerHelper.Info("a6");
                //originalimg.Bitmap.Save("originalimg.png");
                //3.二值化图片
                var thersimg = BinImg(originalimg, operateImgName);
                NLogerHelper.Info("a7");
                //4.边缘检测+返回矩阵组
                var contours = GetContours(thersimg, operateImgName);
                NLogerHelper.Info("a8");
                var rrects = GetRects(contours, thersimg.Size, operateImgName);
                NLogerHelper.Info("a9");
                //5.遍历矩阵组，筛选身份证号码区域的矩阵
                var rects = IdRotatedRect(contours, thersimg.Size, operateImgName);
                NLogerHelper.Info("a10");
                NLogerHelper.Info("疑似身份证号码区域个数：" + rects.Count());

                RectAndAngle truerect = new RectAndAngle();
                foreach (var idrect in rects)
                {
                    NLogerHelper.Info("矩阵:" + idrect.center + "; " + idrect.width + "; " + idrect.height);
                    NLogerHelper.Info("偏移角度:" + idrect.angle);
                    //6.裁剪矩阵区域/旋转矩阵区域
                    var id = cutIdRect(originalimg, idrect, IMG, operateImgName);
                    if (!string.IsNullOrEmpty(id))
                    {
                        truerect = idrect;
                        info.IdentityNumber = id;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (!string.IsNullOrEmpty(info.IdentityNumber))
                {
                    // 继续按照这个身份证号码矩阵计算出其他位置的矩阵，并识别
                    info.Address = address(originalimg, IMG, truerect, operateImgName, rrects);
                    info.Name = name(originalimg, IMG, truerect, operateImgName, rrects);
                    // 性别
                    info.Sex = TxtUtil.GetSex(info.IdentityNumber);

                    info.BirthDay = TxtUtil.GetDate(info.IdentityNumber);
                }
            }
            catch (Exception ex)
            {
                NLogerHelper.Error("处理图片异常：" + ex.Message);
                NLogerHelper.Error("处理图片异常：" + ex.StackTrace);
                NLogerHelper.Error("处理图片异常：" + ex.InnerException);
                NLogerHelper.Error("处理图片异常：" + ex.Source);
            }

            return info;
        }
        #endregion

        #region  操作单张图片函数
        /// <summary>
        ///  操作单张图片函数
        /// </summary>
        /// <param name="imgPath">图片路径</param>
        /// <param name="operateImgName">存放文件夹</param>
        public static Info reserveActionn(string imgPath, string operateImgName)
        {
            Info info = new Info();
            try
            {
                NLogerHelper.Info("b1");
                //原图图像对象，留给切割使用
                Image<Bgr, byte> IMG = new Image<Bgr, byte>(imgPath);
                NLogerHelper.Info("b2");
                IMG = ResizeImage(IMG.Bitmap, imgSize);
                NLogerHelper.Info("b3");
                //操作图像对象，做图片处理使用
                Image<Bgr, byte> img = new Image<Bgr, byte>(imgPath);
                NLogerHelper.Info("b4");
                var originalimg = ResizeImage(img.Bitmap, imgSize);
                NLogerHelper.Info("a5");
                //2.灰度化图片
                originalimg.Convert<Gray, Byte>().Convert<Gray, double>();
                NLogerHelper.Info("a6");
                //originalimg.Bitmap.Save("originalimg.png");
                //3.二值化图片
                var thersimg = BinImg(originalimg, operateImgName);
                NLogerHelper.Info("a7");
                //4.边缘检测+返回矩阵组
                var contours = GetContours(thersimg, operateImgName);
                NLogerHelper.Info("a8");
                var rrects = GetRects(contours, thersimg.Size, operateImgName);
                NLogerHelper.Info("a9");
                // 筛选有效期限区域
                var validRects = ValidDateRotatedRect(contours, thersimg.Size, operateImgName);
                NLogerHelper.Info("a10");
                NLogerHelper.Info("疑似有效期限区域个数：" + validRects.Count());

                RectAndAngle truerect = new RectAndAngle();
                foreach (var validrect in validRects)
                {
                    NLogerHelper.Info("有效期限矩阵:" + validrect.center + "; " + validrect.width + "; " + validrect.height);
                    NLogerHelper.Info("有效期限偏移角度:" + validrect.angle);
                    //6.裁剪矩阵区域/旋转矩阵区域
                    var validdate = cutValidDateRect(originalimg, validrect, IMG, operateImgName);
                    if (!string.IsNullOrEmpty(validdate))
                    {
                        truerect = validrect;
                        info.ValidityDate = validdate;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (!string.IsNullOrEmpty(info.ValidityDate))
                {
                    // 继续按照这个有效期限矩阵计算出其他位置的矩阵，并识别
                    info.Institution = institution(originalimg, IMG, truerect, operateImgName, rrects);
                }
            }
            catch (Exception ex)
            {
                NLogerHelper.Error("处理图片异常：" + ex.Message);
                NLogerHelper.Error("处理图片异常：" + ex.StackTrace);
                NLogerHelper.Error("处理图片异常：" + ex.InnerException);
                NLogerHelper.Error("处理图片异常：" + ex.Source);
            }

            return info;
        }
        #endregion

        #region 读取身份证号码
        /// <summary>
        /// 读取身份证号码
        /// </summary>
        /// <param name="name"></param>
        /// <param name="imgName"></param>
        /// <param name="imgPath"></param>
        /// <param name="IMG"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static string readId(string pngname)
        {
            string ID = string.Empty;
            // 用中文和英文双文字库来读取识别结果，双重保证读取正确性
            var mat = new Mat(pngname);
            string id_chisimstr = TesseractTools.OcrImage(mat, "chi_sim");
            string id_engstr = TesseractTools.OcrImage(mat, "eng"); ;
            NLogerHelper.Info("已执行完身份证号码读取:" + id_chisimstr + "/" + id_engstr);
            //身份证号码
            id_chisimstr = TxtUtil.GetNumber(id_chisimstr);
            id_engstr = TxtUtil.GetNumber(id_engstr);
            // 先处理读取的身份证号码长度           
            if (TxtUtil.CheckIDCard18(id_chisimstr))
            {
                // 身份证
                ID = id_chisimstr;
                return ID;
            }
            if (TxtUtil.CheckIDCard18(id_engstr))
            {
                // 身份证
                ID = id_engstr;
                return ID;
            }
            if (id_chisimstr.Contains("xx") && id_chisimstr.Length == 19)
            {
                id_chisimstr = id_chisimstr.Substring(0, 18);
                if (TxtUtil.CheckIDCard18(id_chisimstr))
                {
                    // 身份证
                    ID = id_chisimstr;
                    return ID;
                }
            }
            if (id_engstr.Contains("xx") && id_engstr.Length == 19)
            {
                id_engstr = id_engstr.Substring(0, 18);
                if (TxtUtil.CheckIDCard18(id_engstr))
                {
                    // 身份证
                    ID = id_engstr;
                    return ID;
                }
            }
            // 身份证号码异常
            if (id_chisimstr.Length == 18)
            {
                ID = id_chisimstr + "(异常号码)";
            }
            else if (id_engstr.Length == 18)
            {
                ID = id_engstr + "(异常号码)";
            }
            return ID;
        }
        #endregion
        #region 读取有效期限
        /// <summary>
        /// 读取有效期限
        /// </summary>
        /// <param name="name"></param>
        /// <param name="imgName"></param>
        /// <param name="imgPath"></param>
        /// <param name="IMG"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static string readValidDate(string pngname)
        {
            // 用中文和英文双文字库来读取识别结果，双重保证读取正确性
            var mat = new Mat(pngname);
            string valid_chisimstr = TesseractTools.OcrImage(mat, "chi_sim");
            string valid_engstr = TesseractTools.OcrImage(mat, "eng"); ;
            NLogerHelper.Info("已执行完有效期限读取:" + valid_chisimstr + "/" + valid_engstr);
            //有效期限
            var check_chisimstr = TxtUtil.GetValidDate(valid_chisimstr);
            var check_engstr = TxtUtil.GetValidDate(valid_engstr);
            // 先处理读取的身份证号码长度           
            if (!string.IsNullOrEmpty(check_chisimstr))
            {
                return check_chisimstr;
            }
            if (!string.IsNullOrEmpty(check_engstr))
            {
                return check_engstr;
            }
            if (TxtUtil.PrepareValidDate(valid_chisimstr))
            {
                return valid_chisimstr;
            }
            if (TxtUtil.PrepareValidDate(valid_engstr))
            {
                return valid_engstr;
            }
            return string.Empty;
        }
        #endregion
        #region 规范图片的尺寸
        /// <summary>
        /// 规范图片的尺寸
        /// </summary>
        /// <param name="imgToResize"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Image<Bgr, byte> ResizeImage(Image imgToResize, Size size)
        {
            //获取图片宽度
            int sourceWidth = imgToResize.Width;
            //获取图片高度
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            //计算宽度的缩放比例
            nPercentW = ((float)size.Width / (float)sourceWidth);
            //计算高度的缩放比例
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            //期望的宽度
            int destWidth = (int)(sourceWidth * nPercent);
            //期望的高度
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //绘制图像
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return new Image<Bgr, byte>(b);
        }
        #endregion
        #region 二值化图像，利用AdaptiveThresholdType 全局化阈值
        /// <summary>
        /// 二值化图像，利用AdaptiveThresholdType 全局化阈值
        /// </summary>
        /// <param name="img"></param>
        /// <param name="imgName"></param>
        /// <returns></returns>
        public static Image<Gray, byte> BinImg(Image<Bgr, byte> img, string imgName)
        {
            var result = img.Convert<Gray, byte>().ThresholdAdaptive(new Gray(255),
                 AdaptiveThresholdType.GaussianC,
                 ThresholdType.Binary,
                 41,
                 new Gray(grayNum)).Erode(9);
            if (Debug)
            {
                var name = imgName + "/BinImg" + DateTime.Now.Millisecond + ".png";
                if (File.Exists(name))
                {
                    result.Save(imgName + "/BinImg2.png");
                }
                else
                {
                    result.Save(name);
                }
            }
            return result;
        }
        #endregion
        #region 获取轮廓
        /// <summary>
        /// 获取轮廓
        /// </summary>
        /// <param name="pic"></param>
        /// <returns></returns>
        private static VectorOfVectorOfPoint GetContours(Image<Gray, byte> currentImage, string imgName)
        {
            VectorOfVectorOfPoint vvp = new VectorOfVectorOfPoint();
            Image<Bgr, Byte> edges = new Image<Bgr, byte>(currentImage.Width, currentImage.Height);
            Mat b1 = new Mat();

            CvInvoke.Canny(currentImage, edges, 100, 200);
            CvInvoke.FindContours(edges, vvp, b1, RetrType.Ccomp, ChainApproxMethod.ChainApproxNone);
            if (Debug)
            {
                Image<Bgr, Byte> disp = new Image<Bgr, byte>(currentImage.Width, currentImage.Height);
                for (int i = 0; i < vvp.Size; i++)
                {
                    CvInvoke.DrawContours(disp, vvp, i, new MCvScalar(255, 255, 255), 1);
                }
                disp.Save(imgName + "/GetContours.png");
            }
            return vvp;
        }
        #endregion
        #region 查找身份证号区域
        /// <summary>
        /// 身份证号区域
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static List<RectAndAngle> IdRotatedRect(VectorOfVectorOfPoint con, Size size, string imgName)
        {
            List<RectAndAngle> rects = new List<RectAndAngle>();
            Image<Bgr, byte> a = new Image<Bgr, byte>(size);
            Point[][] con1 = con.ToArrayOfArray();
            PointF[][] con2 = Array.ConvertAll(con1, new Converter<Point[], PointF[]>(PointToPointF));
            for (int i = 0; i < con.Size; i++)
            {
                RotatedRect rrec = CvInvoke.MinAreaRect(con2[i]);
                float w = rrec.Size.Width;
                float h = rrec.Size.Height;
                if (Debug)
                {
                    var rr = RectCode(rrec);
                    if (rr == null)
                    {
                        continue;
                    }
                    var pointfs = rr.rect;
                    for (int j = 0; j < pointfs.Length; j++)
                    {
                        CvInvoke.Line(a, new Point((int)pointfs[j].X, (int)pointfs[j].Y), new Point((int)pointfs[(j + 1) % 4].X, (int)pointfs[(j + 1) % 4].Y), new MCvScalar(0, 0, 255, 255), 4);
                    }
                }
                if ((w / h > 7 && w / h < 9 && w > 0.3 * size.Width && w < 0.7 * size.Width) || (h / w > 7 && h / w < 9 && h > 0.3 * size.Width && h < 0.7 * size.Width))
                {
                    var tt = RectCode(rrec);
                    if (tt == null)
                    {
                        continue;
                    }
                    NLogerHelper.Info("寻找证件号区域 w:" + w + " ;h:" + h + " ;center:" + rrec.Center.ToString() + " ;angle:" + rrec.Angle + " ;====================");
                    rects.Add(tt);
                }
            }
            if (Debug)
            {
                a.Save(imgName + "/IdRotatedRect" + ".png");
            }
            return rects;
        }
        #endregion

        #region 查找有效期限区域
        /// <summary>
        /// 身份证号区域
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static List<RectAndAngle> ValidDateRotatedRect(VectorOfVectorOfPoint con, Size size, string imgName)
        {
            List<RectAndAngle> rects = new List<RectAndAngle>();
            Image<Bgr, byte> a = new Image<Bgr, byte>(size);
            Point[][] con1 = con.ToArrayOfArray();
            PointF[][] con2 = Array.ConvertAll(con1, new Converter<Point[], PointF[]>(PointToPointF));
            for (int i = 0; i < con.Size; i++)
            {
                RotatedRect rrec = CvInvoke.MinAreaRect(con2[i]);
                float w = rrec.Size.Width;
                float h = rrec.Size.Height;
                if (Debug)
                {
                    var rr = RectCode(rrec);
                    if (rr == null)
                    {
                        continue;
                    }
                    var pointfs = rr.rect;
                    for (int j = 0; j < pointfs.Length; j++)
                    {
                        CvInvoke.Line(a, new Point((int)pointfs[j].X, (int)pointfs[j].Y), new Point((int)pointfs[(j + 1) % 4].X, (int)pointfs[(j + 1) % 4].Y), new MCvScalar(0, 0, 255, 255), 4);
                    }
                }
                if ((w / h > 5 && w / h < 9 && w > 0.2 * size.Width && w < 0.6 * size.Width) || (h / w > 5 && h / w < 9 && h > 0.2 * size.Width && h < 0.6 * size.Width))
                {
                    var tt = RectCode(rrec);
                    if (tt == null)
                    {
                        continue;
                    }
                    NLogerHelper.Info("寻找有效期限区域 w:" + w + " ;h:" + h + " ;center:" + rrec.Center.ToString() + " ;angle:" + rrec.Angle + " ;====================");
                    rects.Add(tt);
                }
            }
            if (Debug)
            {
                a.Save(imgName + "/ValidDateRotatedRect" + ".png");
            }
            return rects;
        }
        #endregion

        #region 获取所有的矩阵轮廓
        /// <summary>
        /// 以身份证的区域作对比，找出并勾出其他区域的位置
        /// <summary>
        /// </summary>
        /// <param name="con"></param>
        /// <param name="size"></param>
        /// <param name="imgName"></param>
        /// <returns></returns>
        public static List<RectAndAngle> GetRects(VectorOfVectorOfPoint con, Size size, string imgName)
        {
            List<RectAndAngle> rects = new List<RectAndAngle>();

            Point[][] con1 = con.ToArrayOfArray();
            PointF[][] con2 = Array.ConvertAll<Point[], PointF[]>(con1, new Converter<Point[], PointF[]>(PointToPointF));
            for (int i = 0; i < con.Size; i++)
            {
                RotatedRect rrec = CvInvoke.MinAreaRect(con2[i]);
                float w = rrec.Size.Width;
                float h = rrec.Size.Height;
                var rr = RectCode(rrec);
                if (rr == null)
                {
                    continue;
                }
                rects.Add(rr);
            }
            return rects;
        }
        #endregion
        #region 截取身份证号码区域
        /// <summary>
        /// 截取身份证号码区域
        /// </summary>
        /// <param name="img"></param>
        /// <param name="idRect"></param>
        /// <param name="IMG"></param>
        /// <param name="imgName"></param>
        /// <param name="imgPath"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        static public string cutIdRect(Image<Bgr, byte> img, RectAndAngle idRect, Image<Bgr, byte> IMG, string imgName)
        {
            // 截取原始身份证号码图像
            var id_rote = Rote(IMG, idRect);
            if (id_rote != null)
            {
                var pngname = "originalId_angle_" + (int)idRect.angle + "X_" + (int)idRect.center.X + "_Y_" + (int)idRect.center.Y + "_W_" + (int)idRect.width + "_H_" + (int)idRect.height + ".png";
                id_rote.Save(imgName + "/" + pngname);
                var id = readId(imgName + "/" + pngname);
                if (!string.IsNullOrEmpty(id))
                {
                    return id;
                }
            }
            // 截取灰度身份证号码图像
            var id_rote_gray = Rote(img, idRect);
            if (id_rote_gray != null)
            {
                var pngname = "grayId_angle_" + (int)idRect.angle + "X_" + (int)idRect.center.X + "_Y_" + (int)idRect.center.Y + "_W_" + (int)idRect.width + "_H_" + (int)idRect.height + ".png";
                id_rote_gray.Save(imgName + "/" + pngname);
                var id = readId(imgName + "/" + pngname);
                if (!string.IsNullOrEmpty(id))
                {
                    return id;
                }
            }
            return string.Empty;
        }
        #endregion
        #region 截取有效期限区域
        /// <summary>
        /// 截取有效期限区域
        /// </summary>
        /// <param name="img"></param>
        /// <param name="idRect"></param>
        /// <param name="IMG"></param>
        /// <param name="imgName"></param>
        /// <param name="imgPath"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        static public string cutValidDateRect(Image<Bgr, byte> img, RectAndAngle validRect, Image<Bgr, byte> IMG, string imgName)
        {
            // 截取原始身份证号码图像
            var valid_rote = Rote(IMG, validRect);
            if (valid_rote != null)
            {
                var pngname = "originalValidDate_angle_" + (int)validRect.angle + "X_" + (int)validRect.center.X + "_Y_" + (int)validRect.center.Y + "_W_" + (int)validRect.width + "_H_" + (int)validRect.height + ".png";
                valid_rote.Save(imgName + "/" + pngname);
                var validdate = readValidDate(imgName + "/" + pngname);
                if (!string.IsNullOrEmpty(validdate))
                {
                    return validdate;
                }
            }
            // 截取灰度身份证号码图像
            var valid_rote_gray = Rote(img, validRect);
            if (valid_rote_gray != null)
            {
                var pngname = "grayValidDate_angle_" + (int)validRect.angle + "X_" + (int)validRect.center.X + "_Y_" + (int)validRect.center.Y + "_W_" + (int)validRect.width + "_H_" + (int)validRect.height + ".png";
                valid_rote_gray.Save(imgName + "/" + pngname);
                var validdate = readValidDate(imgName + "/" + pngname);
                if (!string.IsNullOrEmpty(validdate))
                {
                    return validdate;
                }
            }
            return string.Empty;
        }
        #endregion
        #region 将Point转成PointF格式
        /// <summary>
        /// 将Point转成PointF格式
        /// </summary>
        /// <param name="pf"></param>
        /// <returns></returns>
        private static PointF[] PointToPointF(Point[] pf)
        {
            PointF[] aaa = new PointF[pf.Length];
            int num = 0;
            foreach (var point in pf)
            {
                aaa[num].X = (int)point.X;
                aaa[num++].Y = (int)point.Y;
            }
            return aaa;
        }
        #endregion
        #region 调整图片角度，并输出调整后的图像
        /// <summary>
        ///  调整图片角度，并输出调整后的图像
        /// </summary>
        /// <param name="img"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Image<Bgr, byte> Rote(Image<Bgr, byte> img, RectAndAngle rect)
        {
            try
            {
                PointF center = new PointF();
                PointF[] pointfs = rect.rect;
                center = pointfs[0];
                Image<Bgr, byte> output = new Image<Bgr, byte>(new Size((int)rect.width, (int)rect.height));
                int w = (int)rect.width;
                int h = (int)rect.height;
                for (int i = (int)center.X, m = 0; i < w + (int)center.X; i++, m++)
                {
                    for (int j = (int)center.Y, n = 0; j < h + (int)center.Y; j++, n++)
                    {
                        {
                            Point p = PointRotate(center, new PointF(i, j), -rect.angle);
                            if (p.X >= img.Size.Width)
                                p.X = img.Size.Width - 1;
                            if (p.Y >= img.Size.Height)
                                p.Y = img.Size.Height - 1;
                            output[n, m] = img[p.Y, p.X];
                        }
                    }
                }
                if (Math.Abs(rect.angle) > 90)
                {
                    output = output.Rotate(180, new Bgr(Color.White));
                }
                return output;
            }
            catch
            {
                return null;
            }
        }
        #endregion
        #region 重新计算矩阵的四个顶点坐标，以正常图片的左上角为起点
        /// <summary>
        /// 重新计算矩阵的四个顶点坐标，以正常图片的左上角为起点
        /// </summary>
        /// <param name="pointfs"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static RectAndAngle RectCode(RotatedRect rect)
        {
            try
            {
                RectAndAngle result = new RectAndAngle();
                PointF[] oldPointfs = rect.GetVertices();
                PointF[] newPointfs = new PointF[4];
                // 根据第一个坐标点必须为左上角重新逆时针排序
                var xList = new List<float>();
                var yList = new List<float>();
                for (int i = 0; i < 4; i++)
                {
                    xList.Add(oldPointfs[i].X);
                    yList.Add(oldPointfs[i].Y);
                }
                xList.Sort();
                yList.Sort();
                var minx = xList[0];
                var maxx = xList[3];
                var miny = yList[0];
                var maxy = yList[3];
                var leftP = oldPointfs.Where(x => x.X == minx).ToList();
                var downP = oldPointfs.Where(x => x.Y == maxy).ToList();
                var rightP = oldPointfs.Where(x => x.X == maxx).ToList();
                var upP = oldPointfs.Where(x => x.Y == miny).ToList();
                if (leftP.Count() == 1 && downP.Count() == 1 && rightP.Count() == 1 && upP.Count() == 1)
                {
                    // 可以肯定偏移角度不为0
                    newPointfs[0] = leftP[0];
                    newPointfs[1] = downP[0];
                    newPointfs[2] = rightP[0];
                    newPointfs[3] = upP[0];
                }
                else
                {
                    // 偏移角度为0，则是水平的
                    // 比较左侧两个坐标的Y值，最小的那个为起始点
                    if (leftP[0].Y < leftP[1].Y)
                    {
                        newPointfs[0] = leftP[0];
                        newPointfs[1] = leftP[1];
                    }
                    else
                    {
                        newPointfs[0] = leftP[1];
                        newPointfs[1] = leftP[0];
                    }
                    if (rightP[0].Y > rightP[1].Y)
                    {
                        newPointfs[2] = rightP[0];
                        newPointfs[3] = rightP[1];
                    }
                    else
                    {
                        newPointfs[2] = rightP[1];
                        newPointfs[3] = rightP[0];
                    }
                }
                result.center = rect.Center;
                result.width = rect.Size.Width > rect.Size.Height ? rect.Size.Width : rect.Size.Height;
                result.height = rect.Size.Height < rect.Size.Width ? rect.Size.Height : rect.Size.Width;
                // 计算偏移角度，这里只考虑偏移角度小于90的情况
                // 计算p0和p1的距离
                double value = Math.Sqrt(Math.Abs(newPointfs[1].X - newPointfs[0].X) * Math.Abs(newPointfs[1].X - newPointfs[0].X) + Math.Abs(newPointfs[1].Y - newPointfs[0].Y) * Math.Abs(newPointfs[1].Y - newPointfs[0].Y));
                int oldW = (int)result.width;
                int newW = (int)value;
                if (Math.Abs(oldW - newW) <= 1)
                {
                    // 需要在处理一下
                    // 左上角作为起点
                    newPointfs[1] = leftP[0];
                    newPointfs[2] = downP[0];
                    newPointfs[3] = rightP[0];
                    newPointfs[0] = upP[0];
                }
                //说明偏移角度为负值
                // 将p0作为新坐标系的原点
                var startX = 0;
                var startY = 0;
                var endX = newPointfs[3].X - newPointfs[0].X;
                var endY = newPointfs[3].Y - newPointfs[0].Y;
                double angleOfLine = Math.Atan2((endY - startY), (endX - startX)) * 180 / Math.PI;
                result.angle = (float)angleOfLine;
                result.rect = newPointfs;
                return result;
            }
            catch
            {
                return null;
            }
        }
        #endregion
        #region 调整角度时候，将像素点调整后输出
        /// <summary>
        ///  调整角度时候，将像素点调整后输出
        /// </summary>
        /// <param name="center"></param>
        /// <param name="p1"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Point PointRotate(PointF center, PointF p1, double angle)
        {
            Point tmp = new Point();
            double angleHude = angle * Math.PI / 180;/*角度变成弧度*/
            double x1 = (p1.X - center.X) * Math.Cos(angleHude) + (p1.Y - center.Y) * Math.Sin(angleHude) + center.X;
            double y1 = -(p1.X - center.X) * Math.Sin(angleHude) + (p1.Y - center.Y) * Math.Cos(angleHude) + center.Y;
            tmp.X = (int)x1;
            tmp.Y = (int)y1;
            return tmp;
        }
        #endregion
        #region 根据给定角度旋转图像
        /// <summary>
        /// 根据给定角度旋转图像
        /// </summary>
        /// <param name="b"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Image RotateImg(Image b, float angle)
        {
            angle = angle % 360;            //弧度转换
            double radian = angle * Math.PI / 180.0;
            double cos = Math.Cos(radian);
            double sin = Math.Sin(radian);
            //原图的宽和高
            int w = b.Width;
            int h = b.Height;
            int W = (int)(Math.Max(Math.Abs(w * cos - h * sin), Math.Abs(w * cos + h * sin)));
            int H = (int)(Math.Max(Math.Abs(w * sin - h * cos), Math.Abs(w * sin + h * cos)));
            //目标位图
            Image dsImage = new Bitmap(W, H);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(dsImage);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //计算偏移量
            Point Offset = new Point((W - w) / 2, (H - h) / 2);
            //构造图像显示区域：让图像的中心与窗口的中心点一致
            //Rectangle rect = new Rectangle(Offset.X, Offset.Y, w, h);
            Rectangle rect = new Rectangle(Offset.X, Offset.Y, W, H);
            //Point center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            Point center = new Point(rect.X + w / 2, rect.Y + h / 2);
            g.TranslateTransform(center.X, center.Y);
            g.RotateTransform(360 - angle);
            //恢复图像在水平和垂直方向的平移
            g.TranslateTransform(-center.X, -center.Y);
            g.DrawImage(b, rect);
            //重至绘图的所有变换
            g.ResetTransform();
            g.Save();
            g.Dispose();
            //dsImage.Save("yuancd.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            return dsImage;
        }
        #endregion

        #region 获取姓名区域
        /// <summary>
        /// 姓名区域
        /// </summary>
        /// <param name="rr"></param>
        /// <returns></returns>
        public static RectAndAngle NameRotatedRect(RectAndAngle rr)
        {

            float w = (float)(rr.width * 0.773);//0.773
            float h = (float)(rr.height * 1.069);//1.4
            float px = (float)(rr.center.X - rr.width * 0.391);//
            float py = (float)(rr.center.Y - rr.width * 0.77011);//6.25
            PointF center = new PointF(px, py);
            RotatedRect ss = new RotatedRect(center, new SizeF(w, h), rr.angle);//矫正后的角度
            RectAndAngle rect = new RectAndAngle();//矫正后的角度
            rect.center = center;
            rect.width = w;
            rect.height = h;
            rect.angle = rr.angle;
            rect.rect = ResetPointfs(ss.GetVertices(), rr.angle);
            return rect;
        }
        #endregion
        #region 截取姓名区域,并识别
        /// <summary>
        /// 截取姓名区域,并识别
        /// </summary>
        /// <param name="img"></param>
        /// <param name="idRect"></param>
        /// <param name="imgName"></param>
        /// <returns></returns>
        static public string name(Image<Bgr, byte> img, Image<Bgr, byte> IMG, RectAndAngle idRect, string imgName, List<RectAndAngle> rects)
        {
            RectAndAngle nameRect = NameRotatedRect(idRect);
            // 获取所有疑似轮廓，并保存
            var rectlist = IntersectsWithRectList(rects, nameRect, idRect, 1);
            // 原始图切割
            foreach (var item in rectlist)
            {
                var name = "originalName_center_" + (int)item.center.X + "-" + (int)item.center.Y + "_size_" + (int)item.width + "-" + (int)item.height;
                try
                {
                    // 保存图片
                    var png = Rote(IMG, item);
                    if (png != null)
                    {
                        png.Save(imgName + "/" + name + ".png");
                        var realname = GetRealName(imgName + "/" + name + ".png");
                        if (!string.IsNullOrEmpty(realname))
                        {
                            return realname;
                        }
                    }
                }
                catch (Exception e)
                {
                    NLogerHelper.Info("保存姓名矩阵图片异常：" + name + "/" + e.InnerException.ToString());
                }
            }
            // 灰度图切割
            foreach (var item in rectlist)
            {
                var name = "grayName_center_" + (int)item.center.X + "-" + (int)item.center.Y + "_size_" + (int)item.width + "-" + (int)item.height;
                try
                {
                    // 保存图片
                    var png = Rote(img, item);
                    if (png != null)
                    {
                        png.Save(imgName + "/" + name + ".png");
                        var realname = GetRealName(imgName + "/" + name + ".png");
                        if (!string.IsNullOrEmpty(realname))
                        {
                            return realname;
                        }
                    }
                }
                catch (Exception e)
                {
                    NLogerHelper.Info("保存姓名矩阵图片异常：" + name + "/" + e.InnerException.ToString());
                }
            }
            return string.Empty;
        }
        #endregion
        #region 截取地址区域,并识别
        /// <summary>
        /// 截取地址区域,并识别
        /// </summary>
        /// <param name="img"></param>
        /// <param name="idRect"></param>
        /// <param name="imgName"></param>
        /// <returns></returns>
        static public string address(Image<Bgr, byte> img, Image<Bgr, byte> IMG, RectAndAngle idRect, string imgName, List<RectAndAngle> rects)
        {
            RectAndAngle addressRect = AddressRotatedRect(idRect);
            // 获取所有疑似轮廓，并保存
            var rectlist = IntersectsWithRectList(rects, addressRect, idRect, 0);
            // 原始图切割
            foreach (var item in rectlist)
            {
                var name = "originalAddress_center_" + (int)item.center.X + "-" + (int)item.center.Y + "_size_" + (int)item.width + "-" + (int)item.height;
                try
                {
                    // 保存图片
                    var png = Rote(IMG, item);
                    if (png != null)
                    {
                        png.Save(imgName + "/" + name + ".png");
                        var realaddresss = GetRealAddress(imgName + "/" + name + ".png");
                        if (!string.IsNullOrEmpty(realaddresss) && realaddresss.Length > 8)
                        {
                            return realaddresss;
                        }
                    }
                }
                catch (Exception e)
                {
                    NLogerHelper.Info("保存矩阵图片异常：" + name + "/" + e.InnerException.ToString());
                }
            }
            // 灰度图切割
            foreach (var item in rectlist)
            {
                var name = "grayAddress_center_" + (int)item.center.X + "-" + (int)item.center.Y + "_size_" + (int)item.width + "-" + (int)item.height;
                try
                {
                    // 保存图片
                    var png = Rote(img, item);
                    if (png != null)
                    {
                        png.Save(imgName + "/" + name + ".png");
                        var realaddresss = GetRealAddress(imgName + "/" + name + ".png");
                        if (!string.IsNullOrEmpty(realaddresss) && realaddresss.Length > 8)
                        {
                            return realaddresss;
                        }
                    }
                }
                catch (Exception e)
                {
                    NLogerHelper.Info("保存矩阵图片异常：" + name + "/" + e.InnerException.ToString());
                }
            }
            return string.Empty;
        }
        #endregion

        #region 截取签发机关,并识别
        /// <summary>
        /// 截取签发机关,并识别
        /// </summary>
        /// <param name="img"></param>
        /// <param name="idRect"></param>
        /// <param name="imgName"></param>
        /// <returns></returns>
        static public string institution(Image<Bgr, byte> img, Image<Bgr, byte> IMG, RectAndAngle validRect, string imgName, List<RectAndAngle> rects)
        {
            RectAndAngle institutionRect = InstitutionRotatedRect(validRect);
            // 获取所有疑似轮廓，并保存
            var rectlist = IntersectsWithRectList(rects, institutionRect, validRect, 0);
            // 原始图切割
            foreach (var item in rectlist)
            {
                var name = "originalInstitution_center_" + (int)item.center.X + "-" + (int)item.center.Y + "_size_" + (int)item.width + "-" + (int)item.height;
                try
                {
                    // 保存图片
                    var png = Rote(IMG, item);
                    if (png != null)
                    {
                        png.Save(imgName + "/" + name + ".png");
                        var realinstitution = GetRealAddress(imgName + "/" + name + ".png");
                        if (!string.IsNullOrEmpty(realinstitution))
                        {
                            return realinstitution;
                        }
                    }
                }
                catch (Exception e)
                {
                    NLogerHelper.Info("保存矩阵图片异常：" + name + "/" + e.InnerException.ToString());
                }
            }
            // 灰度图切割
            foreach (var item in rectlist)
            {
                var name = "grayInstitution_center_" + (int)item.center.X + "-" + (int)item.center.Y + "_size_" + (int)item.width + "-" + (int)item.height;
                try
                {
                    // 保存图片
                    var png = Rote(img, item);
                    if (png != null)
                    {
                        png.Save(imgName + "/" + name + ".png");
                        var realinstitution = GetRealAddress(imgName + "/" + name + ".png");
                        if (!string.IsNullOrEmpty(realinstitution))
                        {
                            return realinstitution;
                        }
                    }
                }
                catch (Exception e)
                {
                    NLogerHelper.Info("保存矩阵图片异常：" + name + "/" + e.InnerException.ToString());
                }
            }
            return string.Empty;
        }
        #endregion

        #region 校验和筛选姓名信息
        /// <summary>
        /// 校验和筛选姓名信息
        /// </summary>
        /// <param name="pngname"></param>
        /// <param name="imgName"></param>
        /// <returns></returns>
        public static string GetRealName(string pngname)
        {
            var mat = new Mat(pngname);
            string namestr = TesseractTools.OcrImage(mat, "chi_sim");
            namestr = TxtUtil.GetChinese(namestr);
            // 校验读取结果,至少名字是两个字
            if (namestr.Length >= 2)
            {
                // 将多余的“姓名”去掉
                if (namestr.Contains("名"))
                {
                    var index = namestr.IndexOf("名");
                    if (index == 0 && namestr.Length >= 3)
                    {
                        namestr = namestr.Substring(1, namestr.Length - 1);
                        NLogerHelper.Info("已执行完姓名读取:" + namestr);
                        return namestr;
                    }
                    if (index == 1 && namestr.Length >= 4)
                    {
                        namestr = namestr.Substring(2, namestr.Length - 2);
                        NLogerHelper.Info("已执行完姓名读取:" + namestr);
                        return namestr;
                    }
                }
                if (namestr.Contains("姓"))
                {
                    var index = namestr.IndexOf("姓");
                    if (index == 0 && namestr.Length >= 4)
                    {
                        namestr = namestr.Substring(2, namestr.Length - 2);
                        NLogerHelper.Info("已执行完姓名读取:" + namestr);
                        return namestr;
                    }
                }
                NLogerHelper.Info("已执行完姓名读取:" + namestr);
                return namestr;
            }
            return namestr; // 说明名字识别的不太正常
        }
        #endregion

        #region 校验和筛选住址信息
        /// <summary>
        /// 校验和筛选住址信息
        /// </summary>
        /// <param name="pngname"></param>
        /// <param name="imgName"></param>
        /// <returns></returns>
        public static string GetRealAddress(string pngname)
        {
            var mat = new Mat(pngname);
            string addressstr = TesseractTools.OcrImage(mat, "chi_sim");
            // 校验读取结果,至少文地址字占满是一行8个字
            if (addressstr.Length >= 8)
            {
                // 将多余的“地址”去掉
                if (addressstr.Contains("址"))
                {
                    var index = addressstr.IndexOf("址");
                    if (index == 0 && addressstr.Length >= 9)
                    {
                        addressstr = addressstr.Substring(1, addressstr.Length - 1);
                        NLogerHelper.Info("已执行完籍贯地址读取:" + addressstr);
                        return addressstr;
                    }
                    if (index == 1 && addressstr.Length >= 10)
                    {
                        addressstr = addressstr.Substring(2, addressstr.Length - 2);
                        NLogerHelper.Info("已执行完籍贯地址读取:" + addressstr);
                        return addressstr;
                    }
                }
                if (addressstr.Contains("住"))
                {
                    var index = addressstr.IndexOf("住");
                    if (index == 0 && addressstr.Length >= 10)
                    {
                        addressstr = addressstr.Substring(2, addressstr.Length - 2);
                        NLogerHelper.Info("已执行完籍贯地址读取:" + addressstr);
                        return addressstr;
                    }
                }
                NLogerHelper.Info("已执行完籍贯地址读取:" + addressstr);
                return addressstr;
            }
            return addressstr; // 到了这一步的，说明地址识别的不太正常
        }
        #endregion

        #region 校验和筛选签发机关信息
        /// <summary>
        /// 校验和筛选签发机关信息
        /// </summary>
        /// <param name="pngname"></param>
        /// <param name="imgName"></param>
        /// <returns></returns>
        public static string GetRealInstitution(string pngname)
        {
            var mat = new Mat(pngname);
            string institutionstr = TesseractTools.OcrImage(mat, "chi_sim");
            // 校验读取结果,包含签发机关的删除
            institutionstr.Replace("签发机关", "");
            if (institutionstr.Length > 4)
            {
                return institutionstr;
            }
            return string.Empty; // 到了这一步的，说明签发机关识别的不太正常
        }
        #endregion

        #region 地址区域
        /// <summary>
        /// 地址区域
        /// </summary>
        /// <param name="rr"></param>
        /// <returns></returns>
        public static RectAndAngle AddressRotatedRect(RectAndAngle rr)
        {
            float w = (float)((rr.width * 0.773));
            float h = (float)(rr.width * 0.2966);
            float px = (float)(rr.center.X - rr.width * 0.391); // 样本取值均值0.391
            float py = (float)(rr.center.Y - rr.width * 0.2585);
            PointF center = new PointF(px, py);
            RotatedRect ss = new RotatedRect(center, new SizeF(w, h), rr.angle);//矫正后的角度
            RectAndAngle rect = new RectAndAngle();//矫正后的角度
            rect.center = center;
            rect.width = w;
            rect.height = h;
            rect.angle = rr.angle;
            rect.rect = ResetPointfs(ss.GetVertices(), rr.angle);
            return rect;
        }
        #endregion

        #region 签发机构区域
        /// <summary>
        /// 地址区域
        /// </summary>
        /// <param name="rr"></param>
        /// <returns></returns>
        public static RectAndAngle InstitutionRotatedRect(RectAndAngle rr)
        {
            float w = (float)((rr.width));
            float h = (float)(rr.height * 1);
            float px = (float)(rr.center.X);
            float py = (float)(rr.center.Y - rr.height * 1.1);
            PointF center = new PointF(px, py);
            RotatedRect ss = new RotatedRect(center, new SizeF(w, h), rr.angle);//矫正后的角度
            RectAndAngle rect = new RectAndAngle();//矫正后的角度
            rect.center = center;
            rect.width = w;
            rect.height = h;
            rect.angle = rr.angle;
            rect.rect = ResetPointfs(ss.GetVertices(), rr.angle);
            return rect;
        }
        #endregion

        #region 根据角度，重新排列四个顶点的顺序
        public static PointF[] ResetPointfs(PointF[] oldPointfs, float angle)
        {
            PointF[] newPointfs = new PointF[4];
            // 根据第一个坐标点必须为左上角重新逆时针排序
            var xList = new List<float>();
            var yList = new List<float>();
            for (int i = 0; i < 4; i++)
            {
                xList.Add(oldPointfs[i].X);
                yList.Add(oldPointfs[i].Y);
            }
            xList.Sort();
            yList.Sort();
            var minx = xList[0];
            var maxx = xList[3];
            var miny = yList[0];
            var maxy = yList[3];
            var leftP = oldPointfs.Where(x => x.X == minx).ToList();
            var downP = oldPointfs.Where(x => x.Y == maxy).ToList();
            var rightP = oldPointfs.Where(x => x.X == maxx).ToList();
            var upP = oldPointfs.Where(x => x.Y == miny).ToList();
            if (angle == 0)
            {
                // 偏移角度为0，则是水平的
                // 比较左侧两个坐标的Y值，最小的那个为起始点
                if (leftP[0].Y < leftP[1].Y)
                {
                    newPointfs[0] = leftP[0];
                    newPointfs[1] = leftP[1];
                }
                else
                {
                    newPointfs[0] = leftP[1];
                    newPointfs[1] = leftP[0];
                }
                if (rightP[0].Y > rightP[1].Y)
                {
                    newPointfs[2] = rightP[0];
                    newPointfs[3] = rightP[1];
                }
                else
                {
                    newPointfs[2] = rightP[1];
                    newPointfs[3] = rightP[0];
                }

            }
            else if (angle > 0)
            {
                // 左上角作为起点
                newPointfs[1] = leftP[0];
                newPointfs[2] = downP[0];
                newPointfs[3] = rightP[0];
                newPointfs[0] = upP[0];

            }
            else
            {
                newPointfs[0] = leftP[0];
                newPointfs[1] = downP[0];
                newPointfs[2] = rightP[0];
                newPointfs[3] = upP[0];
            }
            return newPointfs;
        }
        #endregion

        #region 判断两个矩阵是否重叠
        /// <summary>
        /// 判断两个矩阵是否重叠
        /// </summary>
        /// <param name="rectList"></param>
        /// <param name="arearect"></param>
        /// <returns></returns>
        public static RectAndAngle IntersectsWithRect(List<RectAndAngle> rectList, RectAndAngle arearect, RectAndAngle idrect, int type)
        {
            List<RectAndAngle> result = new List<RectAndAngle>();
            foreach (var rect in rectList)
            {
                var rect1 = new Rectangle((int)rect.rect[0].X, (int)rect.rect[0].Y, (int)rect.width, (int)rect.height);
                var rect2 = new Rectangle((int)arearect.rect[0].X, (int)arearect.rect[0].Y, (int)arearect.width, (int)arearect.height);
                var rect3 = new Rectangle((int)idrect.rect[0].X, (int)idrect.rect[0].Y, (int)idrect.width, (int)idrect.height);
                var flag = rect1.IntersectsWith(rect2);
                var flag3 = rect1.IntersectsWith(rect3);
                if (flag && !flag3)
                {
                    result.Add(rect);
                }
            }
            if (type == 0)
            {
                // 地址
                var rr = result.Where(x => x.height >= idrect.height).OrderByDescending(x => x.height * x.width).ToList(); // 地址区域取面积最大的
                if (rr.Count > 0)
                {
                    return rr[0];
                }
            }
            else if (type == 1)
            {
                //  姓名
                var rr = result.Where(x => x.height >= idrect.height).OrderByDescending(x => x.height).ToList(); // 姓名区域选取最高的那块
                if (rr.Count > 0)
                {
                    return rr[0];
                }
            }
            return null;
        }
        #endregion

        #region 判断两个矩阵是否重叠,返回所有重叠矩阵
        /// <summary>
        /// 判断两个矩阵是否重叠
        /// </summary>
        /// <param name="rectList"></param>
        /// <param name="arearect"></param>
        /// <returns></returns>
        public static List<RectAndAngle> IntersectsWithRectList(List<RectAndAngle> rectList, RectAndAngle arearect, RectAndAngle idrect, int type)
        {
            List<RectAndAngle> result = new List<RectAndAngle>();
            //1.首先根据区域中心坐标，筛选一遍,姓名和住址的中心正常情况下都会比身份证号码区域的靠左，靠上
            rectList = rectList.Where(x => x.center.X < idrect.center.X && x.center.Y < idrect.center.Y).ToList();
            foreach (var rect in rectList)
            {
                // 待筛选矩阵
                var rect1 = new Rectangle((int)rect.rect[0].X, (int)rect.rect[0].Y, (int)rect.width, (int)rect.height);
                // 参照姓名/住址/有效期限矩阵
                var rect2 = new Rectangle((int)arearect.rect[0].X, (int)arearect.rect[0].Y, (int)arearect.width, (int)arearect.height);
                // 身份证号码矩阵
                var rect3 = new Rectangle((int)idrect.rect[0].X, (int)idrect.rect[0].Y, (int)idrect.width, (int)idrect.height);
                var flag = rect1.IntersectsWith(rect2);
                var flag2 = rect2.Contains(rect1);
                //var flag4 = rect1.Contains(rect2);
                var flag3 = rect1.IntersectsWith(rect3);
                if (!flag3)
                {
                    if (flag || flag2)
                    {
                        result.Add(rect);
                    }
                }
            }
            //2.根据长款高等尺寸大小来筛选，这里将姓名和住址前方的标注文字也算上
            if (type == 0)
            {
                // 住址 
                result = result.Where(x => x.width > idrect.width * 0.7 && x.width < idrect.width * 1.3).OrderByDescending(x => x.height * x.width).ToList(); // 地址区域取面积从大到小排列
            }
            else if (type == 1)
            {
                //  姓名
                result = result.Where(x => x.height >= idrect.height && x.height < idrect.height * 1.2 && x.width > x.height * 1.6).OrderByDescending(x => x.height).ToList(); // 姓名区域选取Y值从高到低排列
            }
            else if (type == 2)
            {
                //  有效期限
                result = result.Where(x => x.height >= idrect.height && x.height < idrect.height * 2.5 && x.width < x.width * 1.1).OrderByDescending(x => x.height).ToList(); // 签发机关区域选取Y值从高到低排列
            }
            //if(result.Count() == 0)
            //{
            //    result.Add(arearect);
            //}
            result.Add(arearect);
            return result;
        }
        #endregion

        #region 直方图均衡化（彩色图像）
        /// <summary>
        /// 直方图均衡化（彩色图像）
        /// </summary>
        /// <param name="img"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        public static string EqualizeHist(string img, string temp)
        {
            //【1】加载并显示源图像
            Mat srcImage = CvInvoke.Imread(img);
            //【2】将srcImage转换到YCrCb颜色空间
            Mat yccImage = new Mat(); //定义yccImage变量，存储转化后的图
            CvInvoke.CvtColor(srcImage, yccImage, ColorConversion.Bgr2YCrCb); //颜色空间转化

            //【3】分离yccImage，以便对其亮度通道（Y通道）应用直方图均衡化
            VectorOfMat channels = new VectorOfMat(); //定义channels存储split后的各通道图像
            CvInvoke.Split(yccImage, channels); //调用Split函数，分离yccImage颜色通道
            CvInvoke.EqualizeHist(channels[0], channels[0]); //对Y通道进行直方图均衡化

            //【4】转换回Bgr颜色空间，并显示结果图
            CvInvoke.Merge(channels, yccImage); //合并所有通道，重新生成yccImage
            Mat dstImage = new Mat(); //定义存储结果的Mat变量
            CvInvoke.CvtColor(yccImage, dstImage, ColorConversion.YCrCb2Bgr); //从YCrCb转回Bgr颜色空间
            string src = temp + "/直方图.png";
            dstImage.Save(src);
            return src;
        }
        #endregion

        #region 获取身份证区域轮廓
        /// <summary>
        /// 获取轮廓
        /// </summary>
        /// <param name="pic"></param>
        /// <returns></returns>
        public static string GetCard(string imgPath, string operateImgName)
        {
            NLogerHelper.Info("c1");
            //原图图像对象
            Image<Bgr, byte> IMG = new Image<Bgr, byte>(imgPath);
            NLogerHelper.Info("c2");
            //2.灰度化图片
            IMG.Convert<Gray, Byte>().Convert<Gray, double>();
            NLogerHelper.Info("c3");
            //3.二值化图片
            var thersimg = BinImg(IMG, operateImgName);
            NLogerHelper.Info("c4");
            //4.边缘检测+返回矩阵组 
            VectorOfVectorOfPoint vvp = new VectorOfVectorOfPoint();
            Image<Bgr, Byte> edges = new Image<Bgr, byte>(IMG.Width, IMG.Height);
            Mat b1 = new Mat();
            CvInvoke.Canny(IMG, edges, 100, 200);//
            CvInvoke.FindContours(edges, vvp, b1, RetrType.Ccomp, ChainApproxMethod.ChainApproxNone);
            if (Debug)
            {
                // MASK: 优化性能
                Image<Bgr, Byte> disp = new Image<Bgr, byte>(IMG.Width, IMG.Height);
                for (int i = 0; i < vvp.Size; i++)
                {
                    CvInvoke.DrawContours(disp, vvp, i, new MCvScalar(255, 255, 255), 1);
                }
                disp.Save(operateImgName + "/GetCardContours.png");
            }
            List<RectAndAngle> rects = new List<RectAndAngle>();
            Image<Bgr, byte> a = new Image<Bgr, byte>(IMG.Size);
            Point[][] con1 = vvp.ToArrayOfArray();
            PointF[][] con2 = Array.ConvertAll(con1, new Converter<Point[], PointF[]>(PointToPointF));
            for (int i = 0; i < vvp.Size; i++)
            {
                RotatedRect rrec = CvInvoke.MinAreaRect(con2[i]);
                float w = rrec.Size.Width;
                float h = rrec.Size.Height;
                if (Debug)
                {
                    // MASK: 优化性能
                    var rr = RectCode(rrec);
                    if (rr == null)
                    {
                        continue;
                    }
                    var pointfs = rr.rect;
                    for (int j = 0; j < pointfs.Length; j++)
                    {
                        CvInvoke.Line(a, new Point((int)pointfs[j].X, (int)pointfs[j].Y), new Point((int)pointfs[(j + 1) % 4].X, (int)pointfs[(j + 1) % 4].Y), new MCvScalar(0, 0, 255, 255), 4);
                    }
                }
                NLogerHelper.Info("寻找证件号区域 w:" + w + " ;h:" + h + " ;center:" + rrec.Center.ToString() + " ;angle:" + rrec.Angle + " ;====================");
                if ((w / h > 1 && w / h < 2 ) || (h / w > 1 && h / w < 2 ))
                {
                    var tt = RectCode(rrec);
                    if (tt == null)
                    {
                        continue;
                    }
                    rects.Add(tt);
                }
                if (Debug)
                {
                    a.Save(operateImgName + "/CardRotatedRect" + ".png");
                }
            }
            var cards = rects.Where(x => 1 == 1).OrderByDescending(x => x.width * x.height);
            if (cards.Count() > 0)
            {
                var card = cards.FirstOrDefault();
                var cardpath = operateImgName + "card" + DateTime.Now.Millisecond.ToString() + ".png";
                var newImg = new Image<Bgr, byte>(imgPath);
                var rr = Rote(newImg, card);
                if(rr != null)
                {
                    rr.Save(cardpath);
                    return cardpath;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }
        #endregion

        //切割图片
        public static void CutImage(string imgPath,string temp,int size)
        {
            Image<Bgr, byte> img = new Image<Bgr, byte>(imgPath);
            int w = img.Width;
            int h = img.Height;
            for(int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    RotatedRect rect = new RotatedRect();
                    rect.Angle = 0;
                    rect.Center = new PointF(((float)w / (float)size) * (float)i + ((float)w / (float)size) / (float)2, ((float)h / (float)size) * (float)j + ((float)h / (float)size) / (float)2);
                    rect.Size = new Size(w / size, h / size);
                    
                    PointF[] pointfs = rect.GetVertices();
                    Point p = Point.Round(pointfs[1]);
                    Size s = Size.Round(rect.Size);
                    Rectangle rectangle = new Rectangle(p, s);
                    Image<Bgr, byte> Sub = img.GetSubRect(rectangle);
                    Image<Bgr, byte> CropImage = new Image<Bgr, byte>(Sub.Size);
                    CvInvoke.cvCopy(Sub, CropImage, IntPtr.Zero);
                    CropImage.Save(temp + i+"-"+j + ".png");
                    NLogerHelper.Info("d1");
                    //2.灰度化图片
                    CropImage.Convert<Gray, Byte>().Convert<Gray, double>();
                    NLogerHelper.Info("d3");
                    //3.二值化图片
                    var thersimg = BinImg(CropImage, temp);
                    //NLogerHelper.Info("d4");
                    //4.边缘检测+返回矩阵组 
                    VectorOfVectorOfPoint vvp = new VectorOfVectorOfPoint();
                    Image<Bgr, Byte> edges = new Image<Bgr, byte>(CropImage.Width, CropImage.Height);
                    Mat b1 = new Mat();
                    //CvInvoke.Canny(CropImage, edges, 100, 200);
                    CvInvoke.Canny(thersimg, edges, 100, 200);//
                    CvInvoke.FindContours(edges, vvp, b1, RetrType.Ccomp, ChainApproxMethod.ChainApproxNone);
                    if (Debug)
                    {
                        // MASK: 优化性能
                        Image<Bgr, Byte> disp = new Image<Bgr, byte>(CropImage.Width, CropImage.Height);
                        for (int k = 0; k < vvp.Size; k++)
                        {
                            CvInvoke.DrawContours(disp, vvp, k, new MCvScalar(255, 255, 255), 1);
                        }
                        disp.Save(temp + i + "-" + j + "rect.png");
                    }
                }
            }
        }

        //切割图片
        public static Point ContrastImage(string imgPath1, string imgPath2, int p)
        {
            Image<Bgr, byte> img1 = new Image<Bgr, byte>(imgPath1);
            Image<Bgr, byte> img2 = new Image<Bgr, byte>(imgPath2);
            var result1 = Handle.ContrastPoint(img1.Bitmap, img2.Bitmap, p);
            //var result2 = Handle.ContrastString(img1.Bitmap, img2.Bitmap, p);
            return result1;
        }

        #region 校验和筛选住址信息
        /// <summary>
        /// 校验和筛选住址信息
        /// </summary>
        /// <param name="pngname"></param>
        /// <param name="imgName"></param>
        /// <returns></returns>
        public static string GetBusinessLicense(string pngname)
        {
            var mat = new Mat(pngname);
            string str = TesseractTools.OcrImage(mat, "chi_sim+eng");
            return str;
        }
        #endregion
    }
}