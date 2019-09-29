using OCR_IdentityCard.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace OCR_IdentityCard.Controllers
{
    public class LicenseController : ApiController
    {
        // 文件存放的根路径
        static string folder = ConfigurationManager.AppSettings["tempPath2"].ToString();
        [HttpPost]
        public IHttpActionResult CutImage(int size)
        {
            // 文件的临时处理文件夹
            var temp = Guid.NewGuid().ToString("N");
            if (!Directory.Exists(folder + temp))
            {
                Directory.CreateDirectory(folder + temp);
            }
            // 拿到接口传进来的文件
            var file = HttpContext.Current.Request.Files[0];
            var extension = file.FileName.Split('.').Last().ToLower();

            if (extension == "png" || extension == "jpg" || extension == "jpeg" || extension == "bmp" || extension == "jfif" || extension == "tiff")
            {
                var imgName = folder + temp + "/原图." + extension;
                NLogerHelper.Info("原图--" + imgName);
                file.SaveAs(imgName);
                //var str = ImageHandle.GetBusinessLicense(imgName);
                ImageHandle.GetCard(imgName, folder + temp + "/");
                return Json(new
                {
                    Code = 200,
                    Msg = "信息识别完成！",
                    Data = ""
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
    }
}