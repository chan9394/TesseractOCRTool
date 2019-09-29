using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using OCR_IdentityCard.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace OCR_IdentityCard.Controllers
{
    public class TesseractController : ApiController
    {
        // 文件存放的根路径
        static string folder = ConfigurationManager.AppSettings["tempPath"].ToString();
        static string cutfolder = ConfigurationManager.AppSettings["tempPath1"].ToString();
        [HttpPost]
        public async Task<IHttpActionResult> CheckCard()
        {
            // 文件的临时处理文件夹
            var temp = Guid.NewGuid().ToString("N");
            if (!Directory.Exists(folder + temp))
            {
                Directory.CreateDirectory(folder + temp);
            }
            // 拿到接口传进来的文件，数据库记录的字段信息
            var FrontPath = string.Empty;
            var Flag = 0;
            var frontFlag = 0;
            var reserveFlag = 0;
            var CreateDate = DateTime.Now;
            var IdentityNumber = string.Empty;
            var Name = string.Empty;
            var Sex = string.Empty;
            var Nation = string.Empty;
            var BirthDay = string.Empty;
            var Address = string.Empty;
            var Institution = string.Empty;
            var ValidityDate = string.Empty;
            var ReversePath = string.Empty;
            // 取出照片信息，并保存
            #region 取出照片信息，并保存
            var frontFile = HttpContext.Current.Request.Files["front"];
            var fronttask = Task.Run(() =>
            {
                if (frontFile != null)
                {
                    var frontinfo = Front(frontFile, temp);
                    if (frontinfo != null)
                    {
                        FrontPath = frontinfo.FrontPath;
                        IdentityNumber = frontinfo.IdentityNumber;
                        Name = frontinfo.Name;
                        Sex = frontinfo.Sex;
                        Nation = frontinfo.Nation;
                        BirthDay = frontinfo.BirthDay;
                        Address = frontinfo.Address;
                        frontFlag = 1;
                    }
                }
            });
            //反面
            var reverseFile = HttpContext.Current.Request.Files["reverse"];
            var reversetask = Task.Run(() =>
            {
                if (reverseFile != null)
                {
                    var reverseinfo = Reverse(reverseFile, temp);
                    if (reverseinfo != null)
                    {
                        Institution = reverseinfo.Institution;
                        ValidityDate = reverseinfo.ValidityDate;
                        ReversePath = reverseinfo.ReversePath;
                        reserveFlag = 1;
                    }
                }
            });
            try
            {
                await Task.WhenAll(fronttask, reversetask);
            }
            catch (Exception ex)
            {
                NLogerHelper.Error(ex.Message);
            }

            if (frontFlag > 0 && reserveFlag > 0)
            {
                Flag = 3;
            }
            else if (frontFlag > 0 && reserveFlag == 0)
            {
                Flag = 1;
            }
            else if (frontFlag == 0 && reserveFlag > 0)
            {
                Flag = 2;
            }
            // 保存到数据库
            try
            {
                if (Flag > 0)
                {
                    var sql = @"insert into identity (FrontPath,Flag,CreateDate,IdentityNumber,Name,Sex,Nation,BirthDay,Address,Institution,ValidityDate,ReversePath) values (@FrontPath,@Flag,@CreateDate,@IdentityNumber,@Name,@Sex,@Nation,@BirthDay,@Address,@Institution,@ValidityDate,@ReversePath)";
                    MySqlParameter[] parameters = new MySqlParameter[] {
                        new MySqlParameter("@FrontPath", FrontPath),
                        new MySqlParameter("@Flag", Flag),
                        new MySqlParameter("@CreateDate", CreateDate),
                        new MySqlParameter("@IdentityNumber", IdentityNumber),
                        new MySqlParameter("@Name", Name),
                        new MySqlParameter("@Sex", Sex),
                        new MySqlParameter("@Nation", Nation),
                        new MySqlParameter("@BirthDay", BirthDay),
                        new MySqlParameter("@Address", Address),
                        new MySqlParameter("@Institution", Institution),
                        new MySqlParameter("@ValidityDate", ValidityDate),
                        new MySqlParameter("@ReversePath", ReversePath)
                    };
                    var r = Common.MySqlHelper.ExecuteNonQuery(sql, parameters, CommandType.Text);
                    if (r)
                    {
                        List<Detail> data = new List<Detail>();
                        data.Add(new Detail { Key = "姓名", Value = Name });
                        data.Add(new Detail { Key = "性别", Value = Sex });
                        //data.Add(new Detail { Key = "民族", Value = Nation });
                        data.Add(new Detail { Key = "生日", Value = BirthDay });
                        data.Add(new Detail { Key = "住址", Value = Address });
                        data.Add(new Detail { Key = "号码", Value = IdentityNumber });
                        data.Add(new Detail { Key = "签发机关", Value = Institution });
                        data.Add(new Detail { Key = "有效期限", Value = ValidityDate });
                        return Json(new
                        {
                            Code = 200,
                            Msg = "解析完成！(识别效果不理想的影响因素有很多，比如:光线，拍摄角度，有遮挡物，图片尺寸等，可调整图片质量重新识别提高识别率！)",
                            Data = data
                        });
                    }
                    else
                        return Json(new
                        {
                            Code = 10001,
                            Msg = "识别信息存储失败，可稍后重试！"
                        });
                }
                else
                {
                    return Json(new
                    {
                        Code = 10000,
                        Msg = "请至少上传一张符合图片格式的身份证照片！"
                    });
                }
            }
            catch (Exception ex)
            {
                NLogerHelper.Info(ex.Message);
                return Json(new
                {
                    Code = 10001,
                    Msg = "上传异常！"
                });
            }
            #endregion
        }

        static public ReturnData Front(HttpPostedFile frontFile, string temp)
        {
            var extension = frontFile.FileName.Split('.').Last().ToLower();

            if (extension == "png" || extension == "jpg" || extension == "jpeg" || extension == "bmp" || extension == "jfif" || extension == "tiff")
            {
                // 处理图片对象
                // 正面
                var frontImgName = folder + temp + "/正面." + extension;
                NLogerHelper.Info("正面--" + frontImgName);
                frontFile.SaveAs(frontImgName);
                // 开始识别
                var result = ImageHandle.frontActionn(frontImgName, folder + temp + "/");
                var resultstr = JsonConvert.SerializeObject(result);
                NLogerHelper.Info("result:" + resultstr);
                if (!string.IsNullOrEmpty(result.IdentityNumber) && !string.IsNullOrEmpty(result.Name))
                {
                    // 结果赋值
                    return new ReturnData()
                    {
                        FrontPath = frontImgName,
                        IdentityNumber = result.IdentityNumber,
                        Name = result.Name,
                        Sex = result.Sex,
                        Nation = result.Nation,
                        BirthDay = result.BirthDay,
                        Address = result.Address,
                        frontFlag = 1
                    };
                }
                else
                {
                    // 获取卡片区域的图像
                    var cardpath = ImageHandle.GetCard(frontImgName, folder + temp + "/");
                    if (!string.IsNullOrEmpty(cardpath))
                    {
                        var cardresult = ImageHandle.frontActionn(cardpath, folder + temp + "/");
                        var cardresultstr = JsonConvert.SerializeObject(cardresult);
                        NLogerHelper.Info("cardresultstr:" + cardresultstr);
                        if (!string.IsNullOrEmpty(cardresult.IdentityNumber) && !string.IsNullOrEmpty(cardresult.Name))
                        {
                            // 结果赋值
                            return new ReturnData()
                            {
                                FrontPath = frontImgName,
                                IdentityNumber = cardresult.IdentityNumber,
                                Name = cardresult.Name,
                                Sex = cardresult.Sex,
                                Nation = cardresult.Nation,
                                BirthDay = cardresult.BirthDay,
                                Address = cardresult.Address,
                                frontFlag = 1
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }
        static public ReturnData Reverse(HttpPostedFile reverseFile, string temp)
        {
            var extension = reverseFile.FileName.Split('.').Last().ToLower();

            if (extension == "png" || extension == "jpg" || extension == "jpeg" || extension == "bmp" || extension == "jfif" || extension == "tiff")
            {
                // 处理图片对象
                // 背面
                var reserveImgName = folder + temp + "/背面." + extension;
                NLogerHelper.Info("背面--" + reserveImgName);
                reverseFile.SaveAs(reserveImgName);
                // 开始识别
                var result = ImageHandle.reserveActionn(reserveImgName, folder + temp + "/");
                var resultstr = JsonConvert.SerializeObject(result);
                NLogerHelper.Info("result:" + resultstr);
                if (!string.IsNullOrEmpty(result.ValidityDate) && !string.IsNullOrEmpty(result.Institution))
                {
                    // 结果赋值
                    return new ReturnData()
                    {
                        Institution = result.Institution,
                        ValidityDate = result.ValidityDate,
                        ReversePath = reserveImgName,
                        reserveFlag = 1
                    };
                }
                else
                {
                    // 获取卡片区域的图像
                    var cardpath = ImageHandle.GetCard(reserveImgName, folder + temp + "/");
                    if (!string.IsNullOrEmpty(cardpath))
                    {
                        var cardresult = ImageHandle.reserveActionn(cardpath, folder + temp + "/");
                        var cardresultstr = JsonConvert.SerializeObject(cardresult);
                        NLogerHelper.Info("cardresultstr:" + cardresultstr);
                        if (!string.IsNullOrEmpty(cardresult.ValidityDate) && !string.IsNullOrEmpty(cardresult.Institution))
                        {
                            // 结果赋值
                            return new ReturnData()
                            {
                                Institution = cardresult.Institution,
                                ValidityDate = cardresult.ValidityDate,
                                ReversePath = reserveImgName,
                                reserveFlag = 1
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        public IHttpActionResult CutImage(int size)
        {
            // 文件的临时处理文件夹
            var temp = Guid.NewGuid().ToString("N");
            if (!Directory.Exists(cutfolder + temp))
            {
                Directory.CreateDirectory(cutfolder + temp);
            }
            // 拿到接口传进来的文件
            var file = HttpContext.Current.Request.Files[0];
            var extension = file.FileName.Split('.').Last().ToLower();

            if (extension == "png" || extension == "jpg" || extension == "jpeg" || extension == "bmp" || extension == "jfif" || extension == "tiff")
            {
                var imgName = cutfolder + temp + "/原图." + extension;
                NLogerHelper.Info("原图--" + imgName);
                file.SaveAs(imgName);
                ImageHandle.CutImage(imgName, cutfolder + temp + "/",size);
                return Json(new
                {
                    Code = 200,
                    Msg = "分割与轮廓查找已完成！"
                });
            }
            else
            {
                return Json(new
                {
                    Code = 10001,
                    Msg = "请选择图片类型上传！"
                });
            }
        }

        [HttpPost]
        public IHttpActionResult ContrastImage(int SimilarValue)
        {
            // 文件的临时处理文件夹
            string imageName1 = string.Empty;
            string imageName2 = string.Empty;
            var temp = Guid.NewGuid().ToString("N");
            if (!Directory.Exists(cutfolder + temp))
            {
                Directory.CreateDirectory(cutfolder + temp);
            }
            // 拿到接口传进来的文件
            var file1 = HttpContext.Current.Request.Files[0];
            var extension = file1.FileName.Split('.').Last().ToLower();

            if (extension == "png" || extension == "jpg" || extension == "jpeg" || extension == "bmp" || extension == "jfif" || extension == "tiff")
            {
                imageName1 = cutfolder + temp + "/原图1." + extension;
                NLogerHelper.Info("原图--" + imageName1);
                file1.SaveAs(imageName1);
            }
            else
            {
                return Json(new
                {
                    Code = 10001,
                    Msg = "请选择图片类型上传！"
                });
            }
            // 拿到接口传进来的文件
            var file2 = HttpContext.Current.Request.Files[1];
            var extension1 = file2.FileName.Split('.').Last().ToLower();

            if (extension1 == "png" || extension1 == "jpg" || extension1 == "jpeg" || extension1 == "bmp")
            {
                imageName2 = cutfolder + temp + "/原图2." + extension1;
                NLogerHelper.Info("原图--" + imageName2);
                file2.SaveAs(imageName2);
            }
            else
            {
                return Json(new
                {
                    Code = 10001,
                    Msg = "请选择图片类型上传！"
                });
            }
            var p = ImageHandle.ContrastImage(imageName1, imageName2, SimilarValue);
            return Json(new
            {
                Code = 200,
                Msg = "比对完成！",
                Data=p
            });
        }
    }
}