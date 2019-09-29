/****************************************************************
 * 作    者：chan
 * CLR 版本：4.0.30319.42000
 * 创建时间：2019/5/5 17:54:59
 * 当前版本：1.0.0.1
 * 
 * 描述说明：
 *
 * 修改历史：
 *
*****************************************************************
Copyright @ chan 2019 All rights reserved    
*****************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OCR_IdentityCard.Common
{
    public class RectAndAngle
    {
        public PointF[] rect {
            get;set;
        }
        public float angle {
            get; set;
        }
        public PointF center {
            get; set;
        }
        public float width {
            get; set;
        }
        public float height {
            get; set;
        }
    }
    public class Detail
    {
        public string Key {
            get; set;
        }
        public string Value {
            get; set;
        }
    }
    public class Info
    {
        public string Name {
            get; set;
        }
        public string Sex {
            get; set;
        }
        public string Nation {
            get; set;
        }
        public string BirthDay {
            get; set;
        }
        public string Address {
            get; set;
        }
        public string IdentityNumber {
            get; set;
        }
        public string ValidityDate {
            get; set;
        }
        public string Institution {
            get; set;
        }
    }
    public class ReturnData
    {
        public string FrontPath {
            get; set;
        }
        public string ReversePath {
            get; set;
        }
        public int frontFlag {
            get; set;
        }
        public int reserveFlag {
            get; set;
        }
        public string Name {
            get; set;
        }
        public string Sex {
            get; set;
        }
        public string Nation {
            get; set;
        }
        public string BirthDay {
            get; set;
        }
        public string Address {
            get; set;
        }
        public string IdentityNumber {
            get; set;
        }
        public string ValidityDate {
            get; set;
        }
        public string Institution {
            get; set;
        }
    }
    public class TesseractResult
    {
        public bool flag {
            get; set;
        }
        public string resultstr {
            get; set;
        }
    }
    public class TxtUtil
    {
        /// <summary>
        /// 读取txt文件内容
        /// </summary>
        /// <param name="Path">文件地址</param>
        static public string ReadClearContent(string txt)
        {
            var regex = new Regex(@"([^\u4e00-\u9fa5a-zA-z0-9\s].*?)");
            string content = txt;
            content = content.Replace(" ", "");
            content = content.Replace("\n", "");
            content = content.Replace("\f", "");
            content = content.Replace("\r", "");
            content = content.Replace("_", "");
            content = regex.Replace(content, "");
            return content;
        }
        static public string GetSex(string idStr)
        {
            if (Convert.ToInt32(idStr.ToCharArray()[idStr.Length - 2]) % 2 == 0)
                return "女";


            else return "男";
        }
        static public string GetDate(string idStr)
        {
            try
            {
                string year = idStr.Substring(6, 4);
                string month = idStr.Substring(10, 2);
                string day = idStr.Substring(12, 2);
                return year + "年" + month + "月" + day + "日";
            }
            catch { return null; }
        }

        static public string GetValidDate(string validStr)
        {
            try
            {
                var str = Regex.Replace(validStr, @"[^0-9]+", "");
                if(str.Length == 16)
                {
                    // 纯数字期限
                    string start = str.Substring(0, 8).Insert(6, "-").Insert(4, "-");
                    DateTime time = new DateTime();
                    if (DateTime.TryParse(start, out time) == false)
                    {
                        return string.Empty;//起始日期验证  
                    }
                    string end = str.Substring(8, 8).Insert(6, "-").Insert(4, "-");
                    if (DateTime.TryParse(end, out time) == false)
                    {
                        return string.Empty;//起始日期验证  
                    }
                    return start.Replace("-", ".") + "-" + end.Replace("-", ".");
                }
                else
                {
                    return string.Empty;
                }
            }
            catch { return string.Empty; }
        }

        static public bool PrepareValidDate(string validStr)
        {
            try
            {
                var str = Regex.Replace(validStr, @"[^0-9]+", "");
                if (str.Length > 15)
                {
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        static public string GetNumber(string str)
        {
            str = Regex.Replace(str, "[^a-zA-Z0-9]", "");
            Regex r = new Regex("X|x|\\d+\\.?\\d*");
            bool ismatch = r.IsMatch(str);
            MatchCollection mc = r.Matches(str);

            string result = string.Empty;
            for (int i = 0; i < mc.Count; i++)
            {
                result += mc[i];
            }
            return result;
        }

        static public string GetChinese(string str)
        {
            Regex r = new Regex("[\u4e00-\u9fa5]");
            bool ismatch = r.IsMatch(str);
            MatchCollection mc = r.Matches(str);

            string result = string.Empty;
            for (int i = 0; i < mc.Count; i++)
            {
                result += mc[i];
            }
            return result;
        }

        /// <summary>
        /// 身份证号验证
        /// </summary>
        /// <param name="idNumber"></param>
        /// <returns></returns>
        static public bool CheckIDCard18(string idNumber)
        {
            try
            {
                long n = 0;
                if (long.TryParse(idNumber.Remove(17), out n) == false
                || n < Math.Pow(10, 16) || long.TryParse(idNumber.Replace('x', '0').Replace('X', '0'), out n) == false)
                {
                    return false;//数字验证  
                }
                string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
                if (address.IndexOf(idNumber.Remove(2)) == -1)
                {
                    return false;//省份验证  
                }
                string birth = idNumber.Substring(6, 8).Insert(6, "-").Insert(4, "-");
                DateTime time = new DateTime();
                if (DateTime.TryParse(birth, out time) == false)
                {
                    return false;//生日验证  
                }
                string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
                string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
                char[] Ai = idNumber.Remove(17).ToCharArray();
                int sum = 0;
                for (int i = 0; i < 17; i++)
                {
                    sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
                }
                int y = -1;
                Math.DivRem(sum, 11, out y);
                if (arrVarifyCode[y] != idNumber.Substring(17, 1).ToLower())
                {
                    return false;//校验码验证  
                }
                return true;
            }
            catch
            {
                return false;
            }
            
        }
        static public bool CheckIDCard15(string Id)
        {
            long n = 0;
            if (long.TryParse(Id, out n) == false || n < Math.Pow(10, 14))
            {
                return false;
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;
            }
            string birth = Id.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;
            }
            return true;//正确
        }

    }
}
