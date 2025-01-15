using BorisMobile.Helper;
using BorisMobile.Repository;
using BorisMobile.Utilities;
using ICSharpCode.SharpZipLib.GZip;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SqlCeCommand = Microsoft.Data.Sqlite.SqliteCommand;
namespace BorisMobile.DataTransferController
{
    public class WebApiServices
    {
        private bool m_suppressProtocolValidation;
        private GZipInputStream m_compressedStream;
        private char[] m_inData = new char[4096];
        private RequestState m_requestState = new RequestState();

        public WebApiServices()
        {
        }
        
        public async Task<string> SendRequest(string strRequest,string url,string method)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = method;
                request.AllowWriteStreamBuffering = true;
                request.Headers.Add("Ticket:" + null);

                m_requestState.Init(request);
                byte[] rdr = Encoding.UTF8.GetBytes(strRequest);
                using (Stream reqStream = request.GetRequestStream())
                {
                    GZipOutputStream zipStream = new GZipOutputStream(reqStream); // don't seem able to Dispose of this directly (dispose, calls Close which seems to end up flushing and giving a ContentLength error) 
                    zipStream.Write(rdr, 0, rdr.GetLength(0));
                    zipStream.Close();

                    var response = await request.GetResponseAsync();

                    CheckProtocol(response);
                    string failCode = response.Headers["FailCode"];
                    if (failCode == null)
                    {
                        using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
                        {
                            var responseLine = responseReader.ReadLine();

                            return responseLine;
                        }
                    }
                    else
                    {
                        m_requestState.result = (failCode == "Auth") ? WebRequestResult.AUTH_FAILED : WebRequestResult.RESPONSE_ERROR;
                        throw new ApplicationException("Request failed: " + failCode);
                    }
                }
            }
            catch (Exception e)
            {
                m_requestState.result = WebRequestResult.REQUEST_ERROR;
                m_requestState.exception = e;
                return WebResponseState.Exception.ToString();
            }
        }
        // Overloaded method to check protocol with HttpResponseMessage
        public void CheckProtocol(HttpResponseMessage response)
        {
            IEnumerable<string> protocolValues;
            if (!response.Headers.TryGetValues("m5Protocol", out protocolValues))
            {
                if (!m_suppressProtocolValidation)
                {
                    m_requestState.result = WebRequestResult.RESPONSE_ERROR;
                    throw new ApplicationException("CLIENT ID CANNOT ACCESS SERVER");
                }
            }
        }

        public void CheckProtocol(WebResponse response)
        {
            string protocol = response.Headers["m5Protocol"];
            if (string.IsNullOrEmpty(protocol) && !m_suppressProtocolValidation)
            {
                m_requestState.result = WebRequestResult.RESPONSE_ERROR;
                throw new ApplicationException("CLIENT ID CANNOT ACCESS SERVER");
            }
        }

        public async Task<WebResponseState> RunDownload(Models.GenericAttachments attachment,string token)
        {
            try
            {
                var m_attachmentsDir = FilesHelper.GetAttachmentDirectoryMAUI(Helper.Constants.APP_NAME);
                var m_configDir = FilesHelper.GetConfigDirectoryMAUI();

                string uploadUrl = $"https://provisioning.boris-software.com/as.m5WOAtt?guid={attachment.IdGuid}&generic=yes";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uploadUrl);
                request.Method = "GET";
                request.AllowWriteStreamBuffering = true;

                request.ReadWriteTimeout = 30000;


                request.Headers.Add("Ticket:" + token);
                m_requestState.Init(request);

                var response = await request.GetResponseAsync();

                CheckProtocol(response);
                string failCode = response.Headers["FailCode"];
                if (failCode == null)
                {
                    using (MemoryStream decompressedResponse = GetDecompressedResponseFromStream(response.GetResponseStream()))
                    {
                        using (FileStream wrtr = new FileStream(TempFileName(Path.Combine(m_attachmentsDir, attachment.ShortFileName)), FileMode.Create))
                        {
                            // Allocate byte buffer to hold stream contents
                            byte[] inData = new byte[4096];
                            int bytesRead = decompressedResponse.Read(inData, 0, inData.Length);
                            while (bytesRead > 0)
                            {
                                wrtr.Write(inData, 0, bytesRead);
                                bytesRead = decompressedResponse.Read(inData, 0, inData.Length);
                            }
                        }
                    }
                    IO.SafeFileDelete(Path.Combine(m_configDir, attachment.ShortFileName));
                    if (attachment.EntityType.Equals("CF"))
                    {
                        //await CopyFile(TempFileName(Path.Combine(m_attachmentsDir, attachment.ShortFileName)) , Path.Combine(FilesHelper.GetConfigDirectoryMAUI(), attachment.ShortFileName));
                        File.Move(TempFileName(Path.Combine(m_attachmentsDir, attachment.ShortFileName)), Path.Combine(FilesHelper.GetConfigDirectoryMAUI(), attachment.ShortFileName));
                    }
                    if (attachment.EntityType.Equals("IM"))
                    {
                        //await CopyFile(TempFileName(Path.Combine(m_attachmentsDir, attachment.ShortFileName)), Path.Combine(FilesHelper.GetImagesDirectoryMAUI(), attachment.ShortFileName));
                        File.Move(TempFileName(Path.Combine(m_attachmentsDir, attachment.ShortFileName)), Path.Combine(FilesHelper.GetImagesDirectoryMAUI(), attachment.ShortFileName));
                    }

                    m_requestState.result = WebRequestResult.HAVE_DATA;
                    return WebResponseState.Success;
                }
                else
                {
                    m_requestState.result = WebRequestResult.RESPONSE_ERROR;
                    return WebResponseState.Failed;
                }

            }
            catch (Exception e)
            {
                return WebResponseState.Exception;
            }
    }
   
        private async Task CopyFile(string sourceFile, string destinationfile)
        {
            if (File.Exists(destinationfile))
            {
                File.Delete(destinationfile);
            }
            if (!File.Exists(destinationfile))
            {
                using var stream =  File.OpenRead(sourceFile);

                using (Stream s = File.Create(destinationfile))
                {
                    stream.CopyTo(s);
                }
            }
        }

        public MemoryStream GetDecompressedResponseFromStream(Stream inStream)
        {
            FreeCompressedResponse();
            m_compressedStream = new GZipInputStream(inStream);
            MemoryStream decompressedStream = new MemoryStream();
            int totalSize = 0;
            int size = 2048;
            byte[] writeData = new byte[2048];
            while (true)
            {
                size = m_compressedStream.Read(writeData, 0, size);
                totalSize += size;
                if (size > 0)
                {
                    decompressedStream.Write(writeData, 0, size);
                }
                else
                {
                    break;
                }
            }
            decompressedStream.Seek(0, SeekOrigin.Begin);
            return decompressedStream;
        }

        private void FreeCompressedResponse()
        {
            if (m_compressedStream != null)
            {
                m_compressedStream = null;
            }
        }

        public string TempFileName(string baseFile)
        {
            return baseFile + ".tmp";
        }

        
        public async Task<string> GetFilesRequest(string url, string token,DataTransferParameters dataTransferParameters)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + Helper.Constants.TRANSACTIONS_FILENAME);
                request.Method = "PUT";
                request.AllowWriteStreamBuffering = true;
                request.Headers.Add("Ticket:" + token);
                request.AllowAutoRedirect = false;
                request.Timeout = 600000;

                m_requestState.Init(request);
                using (Stream reqStream = request.GetRequestStream())
                {
                    GZipOutputStream zipStream = new GZipOutputStream(reqStream);
                    XmlSerializer parameterSer = new XmlSerializer(typeof(DataTransferParameters));
                    parameterSer.Serialize(zipStream, dataTransferParameters);
                    zipStream.Close();

                    var response = await request.GetResponseAsync();

                    CheckProtocol(response);
                    string failCode = response.Headers["FailCode"];
                    if (failCode == null)
                    {

                        var m_returnedTransactions = ResponseAsString(response);

                        return m_returnedTransactions;
                    }
                    else
                    {
                        m_requestState.result = WebRequestResult.RESPONSE_ERROR;
                        throw new ApplicationException("Request failed: " + failCode);
                    }
                }
            }
            catch (Exception e)
            {
                m_requestState.result = WebRequestResult.REQUEST_ERROR;
                m_requestState.exception = e;
                return WebResponseState.Exception.ToString();
            }
        }
        public async Task<string> ProcessTransactions(XmlDocument xmlTransactions)
        {
            XmlNodeList transactionList = xmlTransactions.SelectNodes("Trans/Row");
            foreach (XmlNode oneTransaction in transactionList)
            {
                string transactionType = ((XmlElement)oneTransaction).GetAttribute("tr");
                SqlCeCommand sqlCommand = null;
                switch (transactionType)
                {
                    case "I":
                        var dataElement = (XmlElement)oneTransaction;
                        Models.GenericAttachments genericAttachments = new Models.GenericAttachments();
                        foreach (XmlAttribute oneAtt in dataElement.Attributes)
                        {
                            if (oneAtt.Name != "tr")
                            {
                                if (oneAtt.Name == "IdGuid")
                                    genericAttachments.IdGuid = new Guid(oneAtt.Value);
                                if(oneAtt.Name== "EntityType")
                                    genericAttachments.EntityType = oneAtt.Value;
                                if (oneAtt.Name == "EntityId")
                                    genericAttachments.EntityId = Int32.Parse(oneAtt.Value);
                                if (oneAtt.Name == "ShortFileName")
                                    genericAttachments.ShortFileName = oneAtt.Value;
                            }
                        }
                        genericAttachments.NeedToDownload = 1;
                        IRepo<Models.GenericAttachments> genericAttachmentRepo = new Repo<Models.GenericAttachments>(App.Database);
                        var res = await genericAttachmentRepo.Insert(genericAttachments);
                        break;

                    case "U":
                        break;

                    case "D":
                        break;

                    default:
                        throw new ApplicationException( oneTransaction.OuterXml);
                }
            }
            XmlElement lastElement = (XmlElement)transactionList[transactionList.Count - 1];
            string lastIdOrGuid = lastElement.GetAttribute("Id");
            if (string.IsNullOrEmpty(lastIdOrGuid))
            {
                lastIdOrGuid = lastElement.GetAttribute("IdGuid");
            }
            return lastIdOrGuid;
        }
        
        private string ResponseAsString(WebResponse response)
        {
            using (MemoryStream decompressedResponse = GetDecompressedResponseFromStream(response.GetResponseStream()))
            {
                using (StreamReader responseReader = new StreamReader(decompressedResponse, Encoding.UTF8))
                {
                    StringBuilder sb = new StringBuilder();
                    int totalLengthReadForLog = 0;
                    int charsRead = responseReader.Read(m_inData, 0, m_inData.Length);
                    while (charsRead > 0)
                    {
                        sb.Append(m_inData, 0, charsRead);
                        totalLengthReadForLog += charsRead;
                        charsRead = responseReader.Read(m_inData, 0, m_inData.Length);
                    }
                    return sb.ToString();
                }
            }
        }
    }
}
