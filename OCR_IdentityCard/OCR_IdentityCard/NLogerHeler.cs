
/****************************************************************
 * 作    者：zouml
 * CLR 版本：4.0.30319.42000
 * 创建时间：2018/12/9 17:38:04
 * 当前版本：1.0.0.1
 * 
 * 描述说明：NLog帮助类
 *
 * 修改历史：
 *
*****************************************************************
Copyright @ zouml 2018 All rights reserved    
*****************************************************************/

using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_IdentityCard
{
    public class NLogerHelper
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();//初始化Nlog

        /// <summary>
        /// 调试
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        public static void Debug(string msg, params object[] args)
        {
            logger.Debug(msg, args);
        }

        /// <summary>
        /// 信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        public static void Info(string msg, params object[] args)
        {
            logger.Info(msg, args);
        }


        /// <summary>
        /// 异常信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        public static void Error(string msg, params object[] args)
        {
            logger.Error(msg, args);
        }
    }
}
