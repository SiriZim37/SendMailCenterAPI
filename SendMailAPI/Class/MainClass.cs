using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using SendMailAPI.Models;
using SendMailAPI.Class.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static SendMailAPI.Class.Entity.ResponseModelClass;

namespace SendMailAPI.Class
{
    public class MainClass
    {
        private static CultureInfo cultures = new CultureInfo("en-US");
        private DateTimeFormatInfo dateFormat = cultures.DateTimeFormat;
        private string strConnEx = GetConnection.GetConnectionStr()._connectStrEx;
        public string urlPKS;
        public string pathAPIToken = "tltauthenticationapi/";
        public string pathAPICreditPortal = "creditportalexternalapi/";
        public string pathOnlineAppExternal = "onlineappapiexternal/";
        public string pathDealerPortalExternal = "dealerportalexternal/";
        public MainClass()
        {
            urlPKS = Environment.GetEnvironmentVariable("PKS_URL");
        }
        public DataSet ExecuteDataset(string strConn, string str_store, SqlParameter[] para)
        {
            var cnn = new SqlConnection(strConn);
            var cmd = new SqlCommand(str_store, cnn);
            var ds = new DataSet();
            var da = new SqlDataAdapter();
            try
            {
                cnn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = str_store;
                int i = 0;
                while (i <= para.Length - 1)
                {
                    cmd.Parameters.Add(para[i]);
                    i = i + 1;
                }

                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Parameters.Clear();
                cmd.Dispose();
                cnn.Close();
                cnn.Dispose();
            }
        }

        public DataSet ExecuteDatasetNoParam(string strConn, string str_store)
        {
            var cnn = new SqlConnection(strConn);
            var cmd = new SqlCommand(str_store, cnn);
            var ds = new DataSet();
            var da = new SqlDataAdapter();
            try
            {
                cnn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = str_store;
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Parameters.Clear();
                cmd.Dispose();
                cnn.Close();
                cnn.Dispose();
            }
        }

        public int ExecuteNonQuery(string strConn, string str_store, SqlParameter[] para)
        {
            var cnn = new SqlConnection(strConn);
            var cmd = new SqlCommand(str_store, cnn);
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                int i = 0;
                while (i <= para.Length - 1)
                {
                    cmd.Parameters.Add(para[i]);
                    i = i + 1;
                }
                cnn.Open();
                int retval = cmd.ExecuteNonQuery();
                return retval;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Parameters.Clear();
                cmd.Dispose();
                cnn.Close();
                cnn.Dispose();
            }
        }

        public String ExecuteOutput(string strConn, string str_store, SqlParameter[] para)
        {
            var cnn = new SqlConnection(strConn);
            var cmd = new SqlCommand(str_store, cnn);
            try
            {
                cnn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = str_store;
                int i = 0;
                while (i <= para.Length - 1)
                {
                    cmd.Parameters.Add(para[i]);
                    i = i + 1;
                }
                //cmd.Parameters.Add("@O_RESULT", SqlDbType.VarChar, 200);
                //cmd.Parameters["@O_RESULT"].Direction = ParameterDirection.Output;
                cmd.Parameters["@O_LOGID"].Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return cmd.Parameters["@O_LOGID"].Value.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Parameters.Clear();
                cmd.Dispose();
                cnn.Close();
                cnn.Dispose();
            }
        }


        public string DecryptionTokenAPIRegis(string token)
        {
            try
            {
                var master = ExecuteDatasetNoParam("", "dbo.usp_Master_KeysGen");
                string pre_add = "";
                string post_add = "";
                string key = "";
                string key_gen = "";
                foreach (DataRow dr in master.Tables[0].Rows)
                {
                    if ("pre_add_jwt".Equals(dr["MASTER_KEY_01"]))
                        pre_add = Convert.ToString(dr["MASTER_DESC"]).Trim();
                    if ("post_add_jwt".Equals(dr["MASTER_KEY_01"]))
                        post_add = Convert.ToString(dr["MASTER_DESC"]).Trim();
                    if ("key_jwt".Equals(dr["MASTER_KEY_01"]))
                        key = Convert.ToString(dr["MASTER_DESC"]).Trim();
                    if ("key_gen_jwt".Equals(dr["MASTER_KEY_01"]))
                        key_gen = Convert.ToString(dr["MASTER_DESC"]).Trim();
                }

                var parts = token.Split('.');
                string header = parts[0].Remove(0, 1);
                string payload = parts[1].Replace(pre_add, "");
                payload = payload.Replace(post_add, "");
                string foot = parts[2];

                // ************* CHECK VALIDATE ******** ' 

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetValidationParameters(key);
                var vertoken = header + "." + payload + "." + foot;
                SecurityToken validatedToken;
                var handler = tokenHandler.ValidateToken(vertoken, validationParameters, out validatedToken);
                bool isAuthen = handler.Identity.IsAuthenticated;

                if (isAuthen)
                {
                    var crypto = Base64UrlDecode(parts[2]);
                    string headerJson = Encoding.UTF8.GetString(Base64UrlDecode(header));
                    var headerData = JObject.Parse(headerJson);
                    string payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
                    var payloadData = JObject.Parse(payloadJson);
                    string cus_id_wcf = payloadData.GetValue("PPAYLOAD").ToString();
                    return cus_id_wcf;
                }
                else
                {
                    return "invalid";
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static TokenValidationParameters GetValidationParameters(string key)
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = false,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidIssuer = "",
                ValidAudience = "",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
            };
        }

        public static byte[] FromBase64Url(string base64Url)
        {
            string padded = base64Url.Length % 4 == 0 ? base64Url : base64Url + "====".Substring(base64Url.Length % 4);
            string base64 = padded.Replace("_", "/").Replace("-", "+");
            return Convert.FromBase64String(base64);
        }

        public static byte[] Base64UrlDecode(string input)
        {
            string output = input;
            output = output.Replace('-', '+');
            output = output.Replace('_', '/');
            switch (output.Length % 4)
            {
                case 0:
                    {
                        break;
                    }

                case 1:
                    {
                        output += "===";
                        break;
                    }

                case 2:
                    {
                        output += "==";
                        break;
                    }

                case 3:
                    {
                        output += "=";
                        break;
                    }

                default:
                    {
                        throw new Exception("Illegal base64url string!");
                        break;
                    }
            }

            var converted = Convert.FromBase64String(output);
            return converted;
        }


        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public string CheckMasterAuthorization(string authHeader)
        {
            string decode64 = Base64Decode(authHeader);
            string[] decode64s = decode64.Split("&");
            string user = decode64s[0].ToString().Split(":")[1];
            string pass = decode64s[1].ToString().Split(":")[1];
            string type = decode64s[2].ToString().Split(":")[1];
            string res = "";
            try
            {
                DataSet ds = new DataSet();
                var parameters = new SqlParameter[]
                                                    {
                                           new SqlParameter("@P_MASTER_KEY1", SqlDbType.VarChar, 200) { Value = user },
                                           new SqlParameter("@P_MASTER_KEY2", SqlDbType.VarChar, 200) { Value = pass },
                                           new SqlParameter("@P_Type", SqlDbType.VarChar, 200) { Value = type }
                                                    };

                ds = ExecuteDataset("", "usp_CheckMasterAuthorization", parameters);
                if (!string.IsNullOrEmpty(ds.Tables[0].Rows[0]["Results"].ToString()))
                {
                    res = ds.Tables[0].Rows[0]["Results"].ToString();
                }
                return res;
            }
            catch (Exception ex)
            {
                return "catch";
            }
        }

        public DateTime ConvertStringToDate(string iDate)
        {
            try
            {
                if (!string.IsNullOrEmpty(iDate.Trim()))
                {
                    DateTime resDate;
                    if ((iDate.Length < 9))
                    {
                        string d = iDate.Substring(0, 1);
                        string m = iDate.Substring(2, 1);
                        string y = iDate.Substring(4, 4);
                        resDate = Convert.ToDateTime(m + "/" + d + "/" + y, cultures);
                    }
                    else if ((iDate.Length == 9))
                        resDate = Convert.ToDateTime(iDate);
                    else if ((iDate.Length > 10))
                        resDate = Convert.ToDateTime(iDate);
                    else if ((iDate.Length == 10))
                    {
                        if ((iDate.Split("/")[0].Length > 2))
                        {
                            string d = iDate.Substring(7, 2);
                            string m = iDate.Substring(5, 2);
                            string y = "";
                            if ((Convert.ToInt32(iDate.Substring(0, 4)) > 2500))
                                y = (Convert.ToInt32(iDate.Substring(0, 4)) - 543).ToString();
                            else
                                y = iDate.Substring(6, 4);

                            resDate = Convert.ToDateTime(m + "/" + d + "/" + y, cultures);
                        }
                        else
                        {
                            string d = iDate.Substring(0, 2);
                            string m = iDate.Substring(3, 2);
                            string y = "";
                            if ((Convert.ToInt32(iDate.Substring(6, 4)) > 2500))
                                y = (Convert.ToInt32(iDate.Substring(6, 4)) - 543).ToString();
                            else
                                y = iDate.Substring(6, 4);

                            resDate = Convert.ToDateTime(m + "/" + d + "/" + y, cultures);
                        }
                    }
                    else if ((iDate.Substring(0, 2).Length > 12))
                    {
                        string d = iDate.Substring(0, 2);
                        string m = iDate.Substring(3, 2);
                        string y = iDate.Substring(6, 4);
                        resDate = Convert.ToDateTime(m + "/" + d + "/" + y, cultures);
                    }
                    else
                        resDate = Convert.ToDateTime(iDate);
                    return resDate;
                }
                else
                    return DateTime.MinValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string KeepLog(string WEBSERVICE, string WEBSERVICE_FUNC, string ERROR_MSG, string TRANS_CMD, string TRANS_KEY)
        {
            try
            {
                string logid = "";
                var parameters = new SqlParameter[]
                    {
                      new SqlParameter("@P_WEBSERVICE", SqlDbType.VarChar, 50) { Value = WEBSERVICE },
                      new SqlParameter("@P_WEBSERVICE_FUNC", SqlDbType.VarChar, 50) { Value = WEBSERVICE_FUNC },
                      new SqlParameter("@P_ERROR_MSG", SqlDbType.VarChar, -1) { Value =ERROR_MSG },
                      new SqlParameter("@P_TRANS_CMD", SqlDbType.VarChar, -1) { Value =TRANS_CMD },
                      new SqlParameter("@P_TRANS_KEY", SqlDbType.VarChar, 100) { Value =TRANS_KEY },
                      new SqlParameter("@O_RESULT", SqlDbType.VarChar, 50)
                     };
                //logid = ExecuteOutput(..., "usp_KeepLog", parameters);
                return logid;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string KeepLogEX(string WEBSERVICE, string WEBSERVICE_FUNC, string ERROR_MSG, string TRANS_CMD, string TRANS_KEY)
        {
            try
            {
                string logid = "";
                var parameters = new SqlParameter[]
                    {
                      new SqlParameter("@P_WEBSERVICE", SqlDbType.VarChar, 50) { Value = WEBSERVICE },
                      new SqlParameter("@P_WEBSERVICE_FUNC", SqlDbType.VarChar, 50) { Value = WEBSERVICE_FUNC },
                      new SqlParameter("@P_ERROR_MSG", SqlDbType.VarChar, -1) { Value =ERROR_MSG },
                      new SqlParameter("@P_TRANS_CMD", SqlDbType.VarChar, -1) { Value =TRANS_CMD },
                      new SqlParameter("@P_TRANS_KEY", SqlDbType.VarChar, 100) { Value =TRANS_KEY },
                      new SqlParameter("@O_LOGID", SqlDbType.VarChar, 50)
                     };
                logid = ExecuteOutput(strConnEx, "usp_KeepLog", parameters);
                return logid;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public bool SetBase64ToPDFFile(string branch, string contractNo, string base64BinaryStr)
        {
            bool isSuccess = false;
            try
            {
                string FilePDF = branch + contractNo + ".pdf";
                string folderDestination = SettingFolderSavePDF(branch, contractNo);
                string saveFile = folderDestination + "\\" + FilePDF;

                // ลบไฟล์ zip เดิมก่อน 
                string zipOld = folderDestination + "\\" + branch + contractNo + ".zip";
                FileInfo dir = new FileInfo(zipOld);
                if (dir.Exists)
                {
                    File.Delete(zipOld);// delete old pdf if exist
                }

                using (System.IO.FileStream stream = System.IO.File.Create(saveFile))
                {
                    System.Byte[] byteArray = System.Convert.FromBase64String(base64BinaryStr);
                    stream.Write(byteArray, 0, byteArray.Length);
                }
                isSuccess = true;
            }
            catch (Exception ex)
            {
                KeepLogEX("MainClass", "SetBase64ToPDFFile", ex.Message.ToString(), "", "");
                isSuccess = false;
            }
            return isSuccess;
        }

     

        public string SettingFolderSavePDF(string branch, string contractNo)
        {
            try
            {

                DateTime now = DateTime.Now;
                string assDate = now.ToString("yyyyMMdd");
                //string Directory = System.AppDomain.CurrentDomain.BaseDirectory; 
                //string Directory = "E:\\New folder\\";
                string Directory = "D:\\inetpub\\wwwroot\\TransferOWN_File\\";
                // string Directory = "D:\\Project_SourceTree\\SKIPPAYMENT\\Gawogawo\\";   // SIRI LOCAL 
                string folderDestination = Directory + assDate + "\\" + branch + contractNo;
                if (!System.IO.Directory.Exists(folderDestination))
                {
                    System.IO.Directory.CreateDirectory(folderDestination);
                }
                return folderDestination;
            }
            catch (Exception ex)
            {
                return "SettingFolderSavePDF_" + ex.Message.ToString();
            }
        }

   

        public string GetDataSaltByCont(string branch, string contractNo, string pselect = "SALT")
        {

            string salt = "";
            DataSet ds = new DataSet();
            try
            {
                var parameters = new SqlParameter[]
                {
                  new SqlParameter("@P_SELECT", SqlDbType.VarChar, 50) { Value = pselect },
                  new SqlParameter("@P_BRANCH", SqlDbType.VarChar, 50) { Value = branch } ,
                  new SqlParameter("@P_CONTRACT_NO", SqlDbType.VarChar, 50) { Value = contractNo }
                };
                ds = ExecuteDataset(strConnEx, "usp_GetSaltTrackingOWN", parameters);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    salt = ds.Tables[0].Rows[0]["SALT"].ToString();

                }
                return salt;
            }
            catch (Exception ex)
            {
                return salt;
            }
        }

        public string HashZip(string ext_con)
        {
            bool @bool = false;
            string res = "";
            try
            {
                string hashStr = BCrypt.Net.BCrypt.HashPassword(ext_con, 10);
                res = hashStr;
            }
            catch (Exception ex)
            {
                return "";
            }
            return res;
        }

   

        public string Validate_ZIP(string PIN_C, string PIN_S)
        {
            bool @bool = false;
            string res = "";
            try
            {
                string hashStr = BCrypt.Net.BCrypt.HashPassword(PIN_S, PIN_C);

                if (hashStr == PIN_C && (BCrypt.Net.BCrypt.Verify(PIN_S, PIN_C)))
                    @bool = true;

                if ((@bool))
                    res = PIN_S;
                else
                    res = "";
            }
            catch (Exception ex)
            {
                return "";
            }
            return res;
        }


  

        public string AnyFileToBase64(string File_Path)
        {
            FileInfo fInfo = new FileInfo(File_Path);
            long numBytes = fInfo.Length;
            FileStream fStream = new FileStream(File_Path, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fStream);
            byte[] data = br.ReadBytes(System.Convert.ToInt32(numBytes));
            string base64String = Convert.ToBase64String(data);
            br.Close();
            fStream.Close();
            return base64String;
        }

        public void KeepZipFile(string branch, string contractNO, string idcard, string doc_type, string doc_name, string key_pdf, string pathPDF, string doc_date)
        {
            DataSet ds = new DataSet();
            try
            {
                var parameters = new SqlParameter[]
                {
                      new SqlParameter("@P_DOC_TYPE", SqlDbType.VarChar, 20) { Value = doc_type },
                      new SqlParameter("@P_BRANCH", SqlDbType.VarChar, 2) { Value = branch },
                      new SqlParameter("@P_CONTRACT_NO", SqlDbType.VarChar, 50) { Value = contractNO },
                      new SqlParameter("@P_IDCARD", SqlDbType.VarChar, 20) { Value = idcard },
                      new SqlParameter("@P_DOC_NAME", SqlDbType.VarChar, 100) { Value = doc_name },
                      new SqlParameter("@P_PATH_PDF", SqlDbType.VarChar, 200) { Value = pathPDF },
                      new SqlParameter("@P_KEY_PDF", SqlDbType.VarChar, 200) { Value = key_pdf },
                      new SqlParameter("@P_DOC_DATE", SqlDbType.VarChar, 20) { Value = doc_date }
                 };
                ds = ExecuteDataset(strConnEx, "usp_SyncTransferOWN", parameters);
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }

        public string postDataNoHeader(string dictData, string pathUrl, string userAgent = "")
        {
            string Data = "";
            try
            {
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(dictData);
                HttpWebRequest myWebRequest = (HttpWebRequest)WebRequest.Create(pathUrl);
                myWebRequest.ContentType = "application/json"; // 
                myWebRequest.ContentLength = byteArray.Length;
                if (userAgent != "")
                {
                    myWebRequest.UserAgent = userAgent;
                }
                myWebRequest.Method = WebRequestMethods.Http.Post;
                myWebRequest.Credentials = CredentialCache.DefaultCredentials;
                System.IO.Stream dataStream = myWebRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse myWebResponse = myWebRequest.GetResponse();
                dataStream = myWebResponse.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                Data = responseFromServer;
                reader.Close();
                dataStream.Close();
                myWebResponse.Close();
            }
            catch (Exception ex)
            {
                Data = "error---" + ex.Message;
            }
            return Data;
        }



        #region Authen
        public async Task<bool> AuthenPks(string token)
        {
            try
            {
                HttpClient client = new HttpClient();
                Uri serverUri = new Uri("https://pksdev.tlt.co.th");
                Uri relativeUri = new Uri("tltauthenticationapi/authentication/token/verify", UriKind.Relative);
                Uri fullUri = new Uri(serverUri, relativeUri);
                var param = new TokenJWT() { Token = token };
                var response = await client.PostAsJsonAsync(fullUri, param);
                var returnValue = await response.Content.ReadAsAsync<AuthenData>();
                return response.IsSuccessStatusCode && returnValue.Status == "1";
            }
            catch (Exception ex)
            {

                return false;
            }

        }

        public string GetUrl(string type, string name)
        {
            string url = "";
            if (type == "Autherizetion" && name == "token")
            {
                url = urlPKS + pathAPIToken + "authentication/token/generate";
            }
            else if (type == "Autherizetion" && name == "decrypt")
            {
                url = urlPKS + pathAPIToken + "cryptography/decryption";
            }
            else if (type == "Dengine")
            {
                url = "https://intws1dev1.tlt.co.th/" + pathAPICreditPortal + "credit/getdatadeengine";
            }
            else if (type == "acceptoffer")
            {
                url = "https://intws1dev1.tlt.co.th/" + pathAPICreditPortal + "credit/setacceptoffer";
            }
            else if (type == "submitoffer")
            {
                url = "https://intws1dev1.tlt.co.th/" + pathAPICreditPortal + "credit/setoffersubmit";
            }
            else if (type == "cancelcarcredit")
            {
                url = "https://intws1dev1.tlt.co.th/" + pathAPICreditPortal + "credit/SetCreditCancel";
            }
            else if (type == "dealerEmail")
            {
                url = urlPKS + pathDealerPortalExternal + "viewdata/getEmail";
            }
            else if (type == "equick")
            {
                url = urlPKS + pathOnlineAppExternal + "Equick/InsertSaleEquick";
            }
            else if (type == "econtractPDF")
            {
                url = urlPKS + pathOnlineAppExternal + "customer/getpdf";
            }

            return url;
        }




        #endregion
        public bool Call_Line_Bot(string notify_msg)
        {
            string KEY_APIUALT = "xazGoR4Gv0q0nB7IsApUB8agDOhlgceyE0y7LqnpSZd";

            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://notify-api.line.me/api/notify");
                var postData = string.Format("message={0}", notify_msg);
                var data = Encoding.UTF8.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Headers.Add("Authorization", "Bearer " + KEY_APIUALT);

                using (var stream = request.GetRequestStream()) stream.Write(data, 0, data.Length);

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

    }
}
