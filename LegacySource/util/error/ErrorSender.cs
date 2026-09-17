using System;
using System.IO;
using System.Net;
using System.Text;

namespace Util.Error
{
    class ErrorSender
    {
        static void Main(string[] args)
        {
            //get("https://urafitu7e7.execute-api.us-east-2.amazonaws.com/default/Bugs2?TableName=Bugs");

            new ErrorSender().Send("babababa", "hello there", "dasdafsdfs\n \tdasdas");

            //post("https://urafitu7e7.execute-api.us-east-2.amazonaws.com/default/Bugs2");

            //sendPOST();
            LOG.Ln("POST DONE");
        }

        public bool Send(string key, string message, string outString) throws Exception
        {
            Uri url = new Uri("https://gamebugs-f058.restdb.io/rest/bugs");
            HttpWebRequest con = (HttpWebRequest)WebRequest.Create(url);
            con.Method = "POST";
            con.Accept = "*/*";
            con.Headers["x-apikey"] = "60599d5bff8b0c1fbbc28dfb";
            con.ContentType = "application/json";

            string code = $"\"{key}\"";

            {
                message = message.Replace("\t", " ");
                message = message.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");

                message = $"\"{message}\"";
            }

            {
                outString = outString.Replace("\t", " ");
                string[] ss = outString.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                outString = "[";
                for (int i = 0; i < ss.Length; i++)
                {
                    outString += ToHex(ss[i]);
                    if (i < ss.Length - 1)
                        outString += ", ";
                }

                outString += "]";
            }

            LOG.Ln(code);

            LOG.Ln(message);

            LOG.Ln(outString);

            string body = "{"
                + "\"key\": " + code + ","
                + "\"message\": " + message + ","
                + "\"dump\": " + outString
                + "}";

            LOG.Ln();
            LOG.Ln(body);

            byte[] bs = Encoding.UTF8.GetBytes(body);

            con.Headers["charset"] = "utf-8";
            con.ContentLength = bs.Length;

            con.AllowWriteStreamBuffering = true;
            using (Stream requestStream = con.GetRequestStream())
            {
                requestStream.Write(bs, 0, bs.Length);
            }

            int responseCode = (int)((HttpWebResponse)con.GetResponse()).StatusCode;
            LOG.Ln("POST Response Code :: " + responseCode);

            if (responseCode == (int)HttpStatusCode.Created) //success
            {
                using (var responseStream = ((HttpWebResponse)con.GetResponse()).GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        string response = reader.ReadToEnd();
                        LOG.Ln(response);
                        return true;
                    }
                }
            }
            else
            {
                LOG.Ln("POST request not worked");
                return false;
            }
        }

        private string ToHex(string outString)
        {
            byte[] bs = Encoding.UTF8.GetBytes(outString);
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < bs.Length; i++)
            {
                str.Append(bs[i].ToString("x"));
            }
            outString = "\"" + str.ToString() + "\"";
            LOG.Ln(outString);
            return outString;
        }
    }
}