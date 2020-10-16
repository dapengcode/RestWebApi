using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RestWebApi.Config;

namespace RestWebApi.Sign
{
    /// <summary> API签名
    /// </summary>
    sealed class Signer
    {
        /// <summary> 验证签名
        /// </summary>
        /// <param name="sign">url参数中传入的签名</param>
        /// <param name="apiname">api名称</param>
        /// <param name="time">time</param>
        /// <returns></returns>
        private static bool CheckSign(string sign, string apiName, string time)
        {
            if (time == null)
            {
                Trace.WriteLine("签名失败 time 为空");
                return false;
            }
            //重新签名
            var signature = ToMD5(apiName, time);
            //比较签名,正确直接返回
            if (signature == sign) return true;
            Trace.WriteLine(signature, "签名失败");
            Trace.WriteLine(apiName, "apiname");
            return false;
        }
        /// <summary> 将字符串累加之后转为guid的类型的
        /// </summary>
        /// <param name="strs">需要累加的字符串</param>
        /// <remarks>周子鉴 2015.08.01</remarks>
        private static string ToMD5(params string[] strs)
        {
            using (var md5Provider = new MD5CryptoServiceProvider())
            {
                var str = string.Concat(strs);
                str = str.ToLower();
                var bytes = Encoding.UTF8.GetBytes(str);
                var hash = md5Provider.ComputeHash(bytes);
                var retStr = System.BitConverter.ToString(hash);
                retStr = retStr.Replace("-", "").ToLower();
                return retStr;
            }
        }

        private static void Swap(byte[] arr, int a, int b)
        {
            var temp = arr[a];
            arr[a] = arr[b];
            arr[b] = temp;
        }

        public  Exception CheckAsync(IWebApi api)
        {
            if (api == null)
                throw new ArgumentNullException(nameof(api));


            if (string.IsNullOrEmpty(api.RequestValues.Headers["sign"]))
            {
                return new ApiException(ExceptionCode.ParameterError, "sign");
            }
            if (string.IsNullOrEmpty(api.RequestValues.Headers["time"]))
            {
                return new ApiException(ExceptionCode.ParameterError, "time");
            }

            var sign = api.RequestValues.Headers["sign"];
            var time = api.RequestValues.Headers["time"];
            if (!DateTime.TryParseExact(api.RequestValues.Headers["time"], "yyyyMMddHHmmssfff", null, DateTimeStyles.None, out var requestTime))
            {
                return new ApiException(ExceptionCode.ParameterError, "time");
            }
            if (Math.Abs((int)(requestTime - DateTime.Now).TotalMinutes) > 3)
            {
                return new ApiException(ExceptionCode.TimestampOutOfRange);
            }

            var apiName =string.Concat(api.Url.Segments);

            if (CheckSign(sign, apiName, time))
            {
                return null;
            }
            return new ApiException(ExceptionCode.SignatureInvalid);
        }


    }
}
