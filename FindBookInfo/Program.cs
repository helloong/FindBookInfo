using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Net;
using System.Web;

namespace FindBookInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            string item;
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(@"isbn_test.txt");
                while((item = file.ReadLine()) != null)
                {
                    Console.WriteLine("Search Isbn: [{0}]", item);
                    // FindBookInfo(사업자번호, 포스IP, 상품코드[ISBN])
                    Console.WriteLine(FindBookInfo("12345678901", "192.168.55.32", item));
                }
                file.Close();
                System.Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("상품 검색 오류!!");
                Console.WriteLine(ex.Message);
            }
        }

        public static string FindBookInfo(string bsnnNum, string ipAdrs, string isbn)
        {
            string url = ""; // API URL
            string key = ""; // Key
            string paramString = bsnnNum + ";" + ipAdrs + ";" + isbn;
            string encParamString = Encrypt(paramString, key);

            StringBuilder dataParams = new StringBuilder();
            dataParams.Append("param=" + HttpUtility.UrlEncode(encParamString));
            byte[] byteDataParams = UTF8Encoding.UTF8.GetBytes(dataParams.ToString());

            CookieContainer cookie = new CookieContainer();
            Cookie vndrCode = new Cookie("bsnnNum", "12345678901", "/", ""); // 마지막 파라메터 API 도메인으로 대체
            cookie.Add(vndrCode);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = cookie;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.ContentLength = byteDataParams.Length;

            Stream stDataParams = request.GetRequestStream();
            stDataParams.Write(byteDataParams, 0, byteDataParams.Length);
            stDataParams.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream stReadData = response.GetResponseStream();
            StreamReader srReadData = new StreamReader(stReadData, Encoding.UTF8);

            string strResult = srReadData.ReadToEnd();
            return strResult;
        }

        public static string Encrypt(string textToEncrypt, string key)
        {
            RijndaelManaged cipher = new RijndaelManaged();
            cipher.Mode = CipherMode.CBC;
            cipher.Padding = PaddingMode.PKCS7;
            cipher.KeySize = 128;
            cipher.BlockSize = 128;
            byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = Encoding.UTF8.GetBytes(""); // Salt Key
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }
            Array.Copy(pwdBytes, keyBytes, len);
            cipher.Key = keyBytes;
            cipher.IV = keyBytes;
            ICryptoTransform transform = cipher.CreateEncryptor();
            byte[] plainText = Encoding.UTF8.GetBytes(textToEncrypt);

            return Convert.ToBase64String(transform.TransformFinalBlock(plainText, 0, plainText.Length));
        }
    }
}
