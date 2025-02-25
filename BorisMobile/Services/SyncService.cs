using BorisMobile.DataHandler;
using BorisMobile.DataTransferController;
using BorisMobile.Helper;
using BorisMobile.Models;
using BorisMobile.Repository;
using BorisMobile.Utilities;
using BorisMobile.XML;
using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.SharpZipLib.GZip;
using SkiaSharp;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using static BorisMobile.DataHandler.Data.DataEnum;
using static BorisMobile.DataHandler.SyncDataHandler;
using RequestState = BorisMobile.DataTransferController.RequestState;
using SqlCeCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SqlCeDataReader = Microsoft.Data.Sqlite.SqliteDataReader;
namespace BorisMobile.Services
{
    public class SyncService
    {
        private RequestState m_requestState = new RequestState();
        SyncDataHandler syncDataHandler = new SyncDataHandler();
        WebApiServices webApiService = new WebApiServices();
        //string currentEntity;
        ArrayList urlList;
        string token;
        string username;
        int m_userId = -1;
        DataTransferController.DataTransferParameters dataTransferParameter;
        public async Task<string> TransferData(string token,string username)
        {
            try
            {
                Preferences.Default.Set("UserName", username);
                this.token = token;
                this.username = username;
                var m_configXml = new XmlConfigDoc(Helper.Constants.APPLICATION_CONFIG_FILE);
                urlList = XMLHelper.URLList(m_configXml);
                string[] entitiesToSync = null;
                int[] parentIds = null;
                entitiesToSync = new string[] {"Customers", "Locations" ,"Contacts", "Users", "GenericListDefinitions", "GenericLists",
                                                    "AuditsForCustomer", "CustomersForGroup", "GroupsForUser", "Audits", "AuditWork",
                                                    "WorkOrderDefinitions", "WorkOrders", "WorkOrderAttachments", "GenericAttachments", "AuditsForLocation"
#if SKU_magic5Instant || SKU_magic5InstantLegacy || SKU_magic5 || SKU_magic5Legacy || SKU_ArtTrail || SKU_GSW || SKU_magic5InstantFree || !MonoDroid || SKU_Elecro || SKU_CoolStream || SKU_Wogan || SKU_magic5HVAC || SKU_Tregroes || SKU_AMEC || SKU_JLB || SKU_DrillCut || SKU_Sage50 || SKU_Essential || SKU_magic5CarHire || SKU_Elcot || SKU_MCM || !IsFiretronic
                                                       ,"DataOrganisations"
            };
#endif


                for (int entityNo = 0; entityNo < entitiesToSync.Length; entityNo++)
                {
                    dataTransferParameter = new DataTransferController.DataTransferParameters();
                    //dataTransferParameter.EntityName = entitiesToSync[entityNo];
                    string oneEntity = entitiesToSync[entityNo];
                    if (oneEntity.StartsWith("GenericAttachmentsDownloads_"))
                    {
                        string entityType = oneEntity.Split('_')[1];
                        DownloadGenericAttachmentsForEntity(entityType, parentIds[entityNo]);
                        continue;
                    }
                    //DataTransferEntity dataTransferEntity = null;
                    switch (oneEntity)
                    {
                        case "Customers":
                            dataTransferParameter.EntityName = "Customers";
                            break;
                        case "Locations":
                            dataTransferParameter.EntityName = "Locations";
                            break;
                        case "Contacts":
                            dataTransferParameter.EntityName = "Contacts";
                            break;
                        case "Users":
                            dataTransferParameter.EntityName = "Users";
                            break;
                        case "GenericListDefinitions":
                            dataTransferParameter.EntityName = "GenericListDefinitions";
                            break;
                        case "GenericLists":
                            dataTransferParameter.EntityName = "GenericLists";
                            break;
                        case "AuditsForCustomer":
                            dataTransferParameter.EntityName = "AuditsForCustomer";
                            break;
                        case "CustomersForGroup":
                            dataTransferParameter.EntityName = "CustomersForGroup";
                            break;
                        case "GroupsForUser":
                            dataTransferParameter.EntityName = "GroupsForUser";
                            break;
                        case "Audits":
                            dataTransferParameter.EntityName = "Audits";
                            break;
                        case "WorkOrderDefinitions":
                            dataTransferParameter.EntityName = "WorkOrderDefinitions";
                            break;
                        case "WorkOrders":
                            dataTransferParameter.EntityName = "WorkOrders";
                            break;
                        case "WorkOrderAttachments":
                            dataTransferParameter.EntityName = "WorkOrderAttachments";
                            break;
                        case "GenericAttachments":
                            dataTransferParameter.EntityName = "GenericAttachments";
                            break;
                        case "AuditsForLocation":
                            dataTransferParameter.EntityName = "AuditsForLocation";
                            break;
                        case "DataOrganisations":
                            dataTransferParameter.EntityName = "DataOrganisations";
                            break;
                        default:
                            break;
                    }
                    if(dataTransferParameter.EntityName != null && dataTransferParameter.EntityName != string.Empty)
                        await RunTransfer();

                }
                WeakReferenceMessenger.Default.Send(new SyncUpdateMessage(string.Empty));


                if (m_userId <= 0)
                {
                    m_userId = await syncDataHandler.GetUserIdForUsername(username);
                    Preferences.Set("UserId", m_userId);
                }
                await DownloadAttachmentType(false, m_userId);
                await DownloadAttachmentType(true, m_userId);
                await ProcessGenericAttachments(entitiesToSync);
                return "success";
            }
            catch(Exception ex)
            {
                Debug.WriteLine("SyncService TransferData(md)" + ex.Message);
                return "fail";
            }
        }

        async Task DownloadGenericAttachmentsForEntity(string entityType, int entityId)
        {
            XmlElement m_signInPayload = await XMLHelper.GetSignInPayload();
            Models.Attachments attachment = syncDataHandler.GetNextAttachmentToDownloadForEntity(m_signInPayload, entityType, entityId);
            int max_retries = 3;
            int retry = 0;
            while (attachment != null)
            {
                WeakReferenceMessenger.Default.Send(new SyncUpdateMessage(attachment.FileName));

                if (!await ReceiveAttachment(attachment, true))
                {
                    if (retry < max_retries)
                    {
                        retry++;
                        continue; // try again with the same attachment
                    }
                }
                else
                {
                    //m_currentSession.SessionStats.UploadDocumentCount();
                    attachment = syncDataHandler.GetNextAttachmentToDownloadForEntity(m_signInPayload, entityType, entityId);
                    retry = 0;
                }
            }
        }
        
        private async Task ProcessGenericAttachments(string[] entitiesToDownload)
        {
            try
            {
                var m_configXml = new XmlConfigDoc(Helper.Constants.APPLICATION_CONFIG_FILE);
                XmlElement dataTransferSettings = m_configXml.XmlDocument.SelectSingleNode("Config/DataTransfer") as XmlElement;
                XmlElement m_userAndAppOptions = await syncDataHandler.GetUserAndAppOptions(m_configXml, m_userId);
                XmlElement m_signInPayload = await XMLHelper.GetSignInPayload();

                Collection<string> entities = new Collection<string>(entitiesToDownload);
                if (entities != null && entities.IndexOf("GenericAttachments") != -1)
                {
                    if (entities.IndexOf("Locations") != -1 && entities.IndexOf("Customers") != -1)
                    {
                        // Can we delete any unwanted attachments? If we're just synching GenericAttachments (eg. upgrade only) then we don't want to delete attachments related to customers etc that might not be up to date
                        Collection<Attachments> atts = syncDataHandler.GetUnwantedGenericAttachmentsBasic(XmlUtils.BoolAtt(m_userAndAppOptions, "downloadAllCustomerAttachments", false), XmlUtils.BoolAtt(m_userAndAppOptions, "downloadAllLocationAttachments", false));
                        foreach (Attachments oneAtt in atts)
                        {
                            syncDataHandler.SetAttachmentAsNotDownloaded(oneAtt.IdGuid, true);
                            //m_dataHandler.DeleteGenericAttachment(m_attachmentsDir, oneAtt.IdGuid);

                            IO.SafeFileDelete(FilesHelper.GetAttachmentDirectoryMAUI + "/" + oneAtt.FileName);

                            //string fullFileName = Misc.GetFullPathForAttachment(ConfiguredApplication.Application.ApplicationName, att.ShortFileName);
                            //string message = att.ShortFileName;
                            //if (!File.Exists(fullFileName))
                            //{
                            //    message = "*" + message;
                            //}
                            //else
                            //{
                            //    Utilities.IO.SafeFileDelete(fullFileName);
                            //}

                        }
                    }

                    // Copy over and then delete any config or image updates
                    await ProcessAttachments(FilesHelper.GetConfigDirectoryMAUI(), "CF");
                    await ProcessAttachments(FilesHelper.GetImagesDirectoryMAUI(), "IM");
                    //ProcessAttachments(m_imagesDir, "MD");

                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private async Task ProcessAttachments(string destDir, string entityType)
        {
            Collection<Attachments> configFiles = syncDataHandler.GetGenericAttachmentsForEntity(entityType, -1);
            if (configFiles != null)
            {
                foreach (Attachments oneAttachment in configFiles)
                {
                    string sourceFile = Path.Combine(FilesHelper.GetAttachmentDirectoryMAUI(), oneAttachment.FileName);
                    if (File.Exists(sourceFile)) // may have been copied already
                    {
                        if (entityType != "MD")
                        {
                            string destFile = Path.Combine(destDir, oneAttachment.FileName);
                            IO.SafeFileDelete(destFile);
                            File.Copy(sourceFile, destFile, true); // even using "true" on Android causes ERROR_ALREADY_EXISTS. Possibly.
                            syncDataHandler.DeleteGenericAttachment(FilesHelper.GetAttachmentDirectoryMAUI(), oneAttachment.IdGuid);
                            
                        }
                    }
                }
            }
        }

        async Task DownloadAttachmentType(bool wantGeneric, int userId)
        {
            try
            {
                // Now we've finished, download any attachments that we might need

                // Change for drawingListIds etc.
                // Get all relevant listIds for job-related locations and job-related customers
                string additionalSql = "";
                var m_configXml = new XmlConfigDoc(Helper.Constants.APPLICATION_CONFIG_FILE);
                XmlElement dataTransferSettings = m_configXml.XmlDocument.SelectSingleNode("Config/DataTransfer") as XmlElement;
                XmlElement m_userAndAppOptions = await syncDataHandler.GetUserAndAppOptions(m_configXml, userId);
                XmlElement m_signInPayload = await XMLHelper.GetSignInPayload();

                WOFilter woFilter = new WOFilter(dataTransferSettings, userId);
                if (wantGeneric)
                {
                    List<int> listIdsForJobs = syncDataHandler.ListIdsForJobs(woFilter);
                    string str = "";
                    foreach (int listid in listIdsForJobs)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            str += ", ";
                        }
                        //str += "'" + listid + "'";
                        str += listid;
                    }
                    if (!string.IsNullOrEmpty(str))
                    {
                        str = " OR (JobSpecific = 1 AND Id IN (" + str + "))";
                    }

                    // 231120 EPLR, allow <DataTransfer> setting for different apps which excludes certain document libraries or lists
                    additionalSql = string.Format(" OR (( EntityType = 'LI') AND EntityId IN (SELECT Id FROM GenericLists WHERE List IN (SELECT Id FROM GenericListDefinitions WHERE (JobSpecific = 0 OR JobSpecific IS NULL) {0})))", str);
                    if (m_userAndAppOptions != null)
                    {
                        if (XmlUtils.BoolAtt(m_userAndAppOptions, "downloadAllCustomerAttachments", false))
                        {
                            additionalSql += " OR ( EntityType = 'CU')";
                        }
                        if (XmlUtils.BoolAtt(m_userAndAppOptions, "downloadAllLocationAttachments", false))
                        {
                            additionalSql += " OR ( EntityType = 'LO')";
                        }
                        if (XmlUtils.BoolAtt(m_userAndAppOptions, "downloadAllListItemAttachments", false))
                        {
                            additionalSql += " OR ( EntityType = 'LI')";
                        }
                    }
                }

                Models.Attachments attachment = syncDataHandler.GetNextAttachmentToDownload(wantGeneric, m_signInPayload, additionalSql, woFilter);
                int max_retries = 3;
                int retry = 0;
                while (attachment != null)
                {
                    WeakReferenceMessenger.Default.Send(new SyncUpdateMessage(attachment.FileName));

                    if (!await ReceiveAttachment(attachment, wantGeneric))
                    {
                        if (retry < max_retries)
                        {
                            //string mess = (m_requestState != null && m_requestState.exception != null) ? m_requestState.exception.Message : "";
                            retry++;
                            continue; // try again with the same attachment
                        }
                        else
                        {
                            throw new Exception(); //m_requestState.exception; // so that we don't try processing any more DownloadAttachmentType (generic v non-generic)
                        }
                    }
                    else
                    {
                        //m_currentSession.SessionStats.UploadDocumentCount();
                        attachment = syncDataHandler.GetNextAttachmentToDownload(wantGeneric, m_signInPayload, additionalSql, woFilter);
                        retry = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task<bool> ReceiveAttachment(Models.Attachments attachment, bool wantGeneric)
        {
            try
            {
                string attachmentsDir = FilesHelper.GetAttachmentDirectoryMAUI();
                string imagesDir = FilesHelper.GetImagesDirectoryMAUI();
                string configDir = FilesHelper.GetConfigDirectoryMAUI();
                string fileExtension = Path.GetExtension(attachment.FileName)?.ToLower();
                string targetDirectory;

                switch (fileExtension)
                {
                    case ".png":
                        targetDirectory = imagesDir;
                        break;
                    case ".xml":
                    case ".gif":
                        Console.WriteLine($"XML file {attachment.FileName}");
                        targetDirectory = configDir;
                        break;
                    default:
                        targetDirectory = attachmentsDir;
                        break;
                }

                var targetFilePath = Path.Combine(targetDirectory, attachment.FileName.ToLower());
                bool isJobPdf = (attachment.FileName == $"job_{attachment.IdGuid}.pdf");

                var attchmentFilePath = Path.Combine(attachmentsDir,attachment.FileName.ToLower());
                // Only download if file doesn't exist or is a job PDF
                if (!File.Exists(attchmentFilePath) || isJobPdf)
                //    if (!File.Exists(targetFilePath) || isJobPdf)
                {
                    string uploadUrl = string.Format("{0}{1}?guid={2}&generic={3}",
                                                 urlList[0],
                                                 Helper.Constants.WO_ATTACHMENTS_FILENAME,
                                                 attachment.IdGuid,
                                                 wantGeneric ? "yes" : "");

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uploadUrl);
                    request.Method = "GET";
                    request.AllowWriteStreamBuffering = true;
                    request.ReadWriteTimeout = 30000;
                    request.Headers.Add("Ticket:" + token);
                    m_requestState.Init(request);

                    using (var response = await request.GetResponseAsync())
                    {
                        webApiService.CheckProtocol(response);
                        string failCode = response.Headers["FailCode"];

                        if (failCode == null)
                        {
                            // Create a temporary file path
                            string tempFilePath = GetTempFilePath(targetFilePath);

                            using (MemoryStream decompressedResponse = webApiService.GetDecompressedResponseFromStream(response.GetResponseStream()))
                            using (FileStream wrtr = new FileStream(tempFilePath, FileMode.Create))
                            {
                                // Use the same buffer size as the original code
                                byte[] inData = new byte[4096];
                                int bytesRead;

                                // Reset the decompressed stream position
                                decompressedResponse.Position = 0;

                                while ((bytesRead = decompressedResponse.Read(inData, 0, inData.Length)) > 0)
                                {
                                    await wrtr.WriteAsync(inData, 0, bytesRead);
                                }

                                await wrtr.FlushAsync();
                            }

                            // Delete existing file if it exists
                            if (File.Exists(targetFilePath))
                            {
                                IO.SafeFileDelete(targetFilePath);
                            }

                            // Move temp file to target location
                            File.Move(tempFilePath, targetFilePath);
                        }
                        else
                        {
                            throw new ApplicationException("Failed to retrieve attachment. FailCode: " + failCode);
                        }
                    }
                }

                await syncDataHandler.SetAttachmentAsDownloaded(attachment.IdGuid, wantGeneric);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"ReceiveAttachment failed: {e.Message}");
                if (m_requestState.exception != null && !(m_requestState.exception is WebException))
                {
                    throw m_requestState.exception;
                }
                //m_requestState.result = WebRequestResult.REQUEST_ERROR;
                m_requestState.exception = e;
                return false;
            }
        }

        private string GetTempFilePath(string originalPath)
        {
            string directory = Path.GetDirectoryName(originalPath);
            string filename = Path.GetFileName(originalPath);
            return Path.Combine(directory, $"temp_{filename}");
        }


        //private async Task<bool> ReceiveAttachment(Models.Attachments attachment, bool wantGeneric)
        //{
        //    try
        //    {
        //        string m_attachmentsDir = Misc.GetAttachmentDirectoryMAUI(Helper.Constants.APP_NAME);
        //        bool isJobPdf = (attachment.FileName == "job_" + attachment.IdGuid.ToString() + ".pdf");
        //        var m_attachmentFileName = Path.Combine(m_attachmentsDir , attachment.FileName);
        //        // We *are* asking for an attachment if we already have it and it's a job pdf
        //        if (!File.Exists(m_attachmentFileName) || isJobPdf) // don't ask for the file if it already exists
        //        {
        //            // We don't want to overwrite it in case it's changed - may need some more logic behind this eventually
        //            string uploadUrl = string.Format("{0}{1}?guid={2}&generic={3}", urlList[0], Helper.Constants.WO_ATTACHMENTS_FILENAME, attachment.IdGuid, wantGeneric ? "yes" : "");
        //            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uploadUrl);
        //            request.Method = "GET";
        //            request.AllowWriteStreamBuffering = true;

        //            request.ReadWriteTimeout = 30000; // 12/06/13 try to force quicker timeouts (CCL hanging on many downloads)


        //            request.Headers.Add("Ticket:" + token);
        //            m_requestState.Init(request);

        //            // Start the asynchronous request.
        //            var response = await request.GetResponseAsync();

        //            webApiService.CheckProtocol(response);
        //            string failCode = response.Headers["FailCode"];
        //            if (failCode == null)
        //            {
        //                using (MemoryStream decompressedResponse = webApiService.GetDecompressedResponseFromStream(response.GetResponseStream()))
        //                {
        //                    using (FileStream wrtr = new FileStream(webApiService.TempFileName(m_attachmentFileName), FileMode.Create))
        //                    {
        //                        // Allocate byte buffer to hold stream contents
        //                        byte[] inData = new byte[4096];
        //                        int bytesRead = decompressedResponse.Read(inData, 0, inData.Length);
        //                        while (bytesRead > 0)
        //                        {
        //                            wrtr.Write(inData, 0, bytesRead);
        //                            bytesRead = decompressedResponse.Read(inData, 0, inData.Length);
        //                        }

        //                        // Explicitly flush to ensure all data is written
        //                        wrtr.Flush();
        //                    }
        //                }

        //                var destinationFilePath = Path.Combine(Misc.GetImagesDirectoryMAUI(), attachment.FileName.ToLower());

        //                if (!File.Exists(destinationFilePath))
        //                {
        //                    using var stream = File.OpenRead(webApiService.TempFileName(m_attachmentFileName));

        //                    using var destStream = File.Create(destinationFilePath);

        //                    stream.Seek(0, SeekOrigin.Begin); // Ensure stream starts from beginning
        //                    await stream.CopyToAsync(destStream);
        //                    await destStream.FlushAsync(); // Ensure all data is written
        //                }
        //                else
        //                {
        //                    IO.SafeFileDelete(destinationFilePath);
        //                    //string tempContent = File.ReadAllText(webApiService.TempFileName(m_attachmentFileName));
        //                    //Debug.WriteLine("Temp XML Content: " + sourceFile + " " + tempContent);
        //                    using var stream = File.OpenRead(webApiService.TempFileName(m_attachmentFileName));

        //                    using var destStream = File.Create(destinationFilePath);

        //                    stream.Seek(0, SeekOrigin.Begin); // Ensure stream starts from beginning
        //                    await stream.CopyToAsync(destStream);
        //                    await destStream.FlushAsync(); // Ensure all data is written
        //                }
        //                    //IO.SafeFileDelete(m_attachmentFileName.ToLower());

        //                    //File.Move(webApiService.TempFileName(m_attachmentFileName), m_attachmentFileName);
        //                    //File.Move(TempFileName(Path.Combine(m_attachmentsDir, attachment.ShortFileName)), Path.Combine(Misc.GetConfigDirectory(), attachment.ShortFileName).ToLower());

        //                    //if (entityType.Equals("CF"))
        //                    //    CopyFile(webApiService.TempFileName(Path.Combine(m_attachmentsDir, attachment.FileName)), Path.Combine(Misc.GetConfigDirectoryMAUI(), attachment.FileName));
        //                    //if (entityType.Equals("IM"))
        //                //await CopyFile(webApiService.TempFileName(m_attachmentFileName), Path.Combine(Misc.GetImagesDirectoryMAUI(), attachment.FileName));
        //                    //else
        //                      //  CopyFile(webApiService.TempFileName(Path.Combine(m_attachmentsDir, attachment.FileName)), Path.Combine(Misc.GetImagesDirectoryMAUI(), attachment.FileName));
        //                    //m_requestState.result = WebRequestResult.HAVE_DATA;
        //            }
        //            else
        //            {
        //                //m_requestState.result = WebRequestResult.RESPONSE_ERROR;
        //                throw new ApplicationException();
        //            }
        //            //IAsyncResult asyncResult = request.BeginGetResponse(WorkOrderAttachmentCallback, m_requestState);
        //            //SetTimeout(asyncResult);
        //            //allDone.WaitOne(); // this blocks until a response is received
        //            //if (m_requestState.exception != null)
        //            //{
        //            //    throw m_requestState.exception;
        //            //}
        //            // The attachment has now been saved in the appropriate location
        //        }
        //        await syncDataHandler.SetAttachmentAsDownloaded(attachment.IdGuid, wantGeneric);
        //        return true;
        //    }
        //    catch (Exception e)
        //    {

        //        // 12/06/13 user aborts were just hanging in the UI, ReceiveAttachment cancellations need to abort the session in the same way that RunOneEntity does
        //        // Also, timeouts on ReceiveAttachment weren't being re-thrown so processing would just seem to hang forever if left
        //        ///if (m_requestState.exception != null && m_requestState.exception as WebException == null) // give up by throwing exception
        //       // {
        //        //    throw m_requestState.exception;
        //        //}

        //        // Fall through and return false so that we can retry (eg. timeout)
        //        //m_requestState.result = WebRequestResult.REQUEST_ERROR;
        //        //m_requestState.exception = e;

        //    }
        //    return false;
        //}

        private async Task CopyFile(string sourceFile, string destinationfile)
        {
            if (!File.Exists(destinationfile))
            {
                if (Path.GetExtension(sourceFile) == ".xml.tmp")
                {
                    string tempContent = File.ReadAllText(sourceFile);
                    Debug.WriteLine("Temp XML Content: " + sourceFile+ " " + tempContent);
                    using var stream = File.OpenRead(sourceFile);

                    using var destStream = File.Create(destinationfile);

                    stream.Seek(0, SeekOrigin.Begin); // Ensure stream starts from beginning
                    await stream.CopyToAsync(destStream);
                    await destStream.FlushAsync(); // Ensure all data is written
                }
                else
                {
                    using var stream = File.OpenRead(sourceFile);

                    using var destStream = File.Create(destinationfile);

                    stream.Seek(0, SeekOrigin.Begin); // Ensure stream starts from beginning
                    await stream.CopyToAsync(destStream);
                    await destStream.FlushAsync(); // Ensure all data is written
                }
                
                //using (Stream s = File.Create(destinationfile))
                //{
                //    stream.CopyTo(s);
                //}
            }
        }

        public async Task InitialiseDataParameters()
        {
            IRepo<Models.DataTransferParameters> dataTransferParameterRepo = new Repo<Models.DataTransferParameters>(DBHelper.Database);
            var res = await dataTransferParameterRepo.Get();
            var parameterObject = res.Where(X => X.EntityType == dataTransferParameter.EntityName).FirstOrDefault();
            if(parameterObject == null)
            {
                //fresh sync
                dataTransferParameter.LastTransactionDate = new DateTime();
                dataTransferParameter.LastId = -1;
                dataTransferParameter.LastGuid = Guid.Empty;
                dataTransferParameter.BaseTransactionDate = new DateTime();

                var paramToInsert = DataTransferParameterConversion(dataTransferParameter);
                var insertedId = await dataTransferParameterRepo.Insert(paramToInsert);
                dataTransferParameter.Id = insertedId;
            }
            else
            {
                dataTransferParameter.Id = parameterObject.Id;
                dataTransferParameter.LastTransactionDate = parameterObject.LastTransactionDate;
                dataTransferParameter.BaseTransactionDate = parameterObject.BaseTransactionDate;
                dataTransferParameter.LastId = parameterObject.LastId;
                dataTransferParameter.LastGuid = parameterObject.LastGuid;
            }

        }

        public async Task SetNewTransactionDate(string date)
        {
            DateTime newDate = Misc.ConvertISOStringToDateTime(date);
            if (newDate.Equals(dataTransferParameter.LastTransactionDate) == false)
            {
                dataTransferParameter.LastTransactionDate = newDate;
                dataTransferParameter.LastId = -1;
                dataTransferParameter.LastGuid = Guid.Empty;
                IRepo<Models.DataTransferParameters> dataTransferParameterRepo = new Repo<Models.DataTransferParameters>(DBHelper.Database);
                var paramToUpdate = DataTransferParameterConversion(dataTransferParameter);
                await dataTransferParameterRepo.Update(paramToUpdate);
            }
        }
        public async Task SetNewDateAndId(string date,string lastProcessedId, bool getdata)
        {
            DateTime newDate = Misc.ConvertISOStringToDateTime(date);
            dataTransferParameter.LastTransactionDate = newDate;
            if (getdata == true)
            {
                dataTransferParameter.BaseTransactionDate = newDate;
            }
            int lastId = -1;
            Guid lastGuid = Guid.Empty;
            try
            {
                lastId = Int32.Parse(lastProcessedId);
                dataTransferParameter.LastId = lastId;
                dataTransferParameter.LastGuid = Guid.Empty;
            }
            catch (Exception)
            {
                lastGuid = new Guid(lastProcessedId);
                dataTransferParameter.LastGuid = lastGuid;
                dataTransferParameter.LastId = -1;
            }
            IRepo<Models.DataTransferParameters> dataTransferParameterRepo = new Repo<Models.DataTransferParameters>(DBHelper.Database);
            var paramToUpdate = DataTransferParameterConversion(dataTransferParameter);
            await dataTransferParameterRepo.Update(paramToUpdate);
        }

        int count = 0;
        public async Task RunTransfer()
        {
            try
            {
                count = 0;
                bool getData = true;
                string maxDate = string.Empty;
                string lastIdProcessed = string.Empty;
                await InitialiseDataParameters();
                while (getData == true)
                {
                    var response = await webApiService.GetFilesRequest(urlList[0].ToString(), token, dataTransferParameter);
                    XmlDocument transactionDoc = new XmlDocument();
                    transactionDoc.LoadXml(response);

                    XmlElement headerElement = (XmlElement)transactionDoc.SelectSingleNode("Trans");
                    string minDate = headerElement.GetAttribute("minDate");
                    if (minDate != "")
                    {
                        await SetNewTransactionDate(minDate);
                        lastIdProcessed = await ProcessTransactions(transactionDoc);
                        //string lastIdProcessed = oneEntity.ProcessTransactions(transactionDoc, SendMessageBackToParentUI);  // this also updates sync parameters table (last trans date etc)
                        string repeatFlag = headerElement.GetAttribute("repeat");
                        getData = (repeatFlag == "1");
                        maxDate = headerElement.GetAttribute("maxDate");
                        await SetNewDateAndId(headerElement.GetAttribute("maxDate"), lastIdProcessed, getData == false);
                    }
                    else
                    {
                        getData = false;    // no transactions
                    }
                }
            }catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        public Models.DataTransferParameters DataTransferParameterConversion(DataTransferController.DataTransferParameters controllerParam)
        {
            Models.DataTransferParameters param = new Models.DataTransferParameters();
            param.Id = controllerParam.Id;
            param.EntityType = controllerParam.EntityName;
            param.LastGuid = controllerParam.LastGuid;
            param.LastId = controllerParam.LastId;
            param.LastTransactionDate = controllerParam.LastTransactionDate;
            param.BaseTransactionDate = controllerParam.BaseTransactionDate;

            return param;
        }

        public T ParseFromXml<T>(XmlElement data) where T : new()
        {
            T obj = new T();
            try
            {
                var properties = typeof(T).GetProperties();

                // Create an instance of T
                
                // Find the root element, then loop over each child element
                foreach (XmlAttribute element in data.Attributes)
                {
                    if (element.Name != "tr")
                    {
                        foreach (var property in properties)
                        {
                            if (element.Name == property.Name)
                            {
                                if (property.PropertyType == typeof(Guid))
                                {
                                    var guidValue = new Guid(element.Value);
                                    property.SetValue(obj, guidValue);
                                    break;
                                }
                                else
                                {
                                    object value = Convert.ChangeType(element.Value, property.PropertyType);
                                    property.SetValue(obj, value);
                                    break;
                                }
                            }
                        }
                    }
                }
                foreach (XmlNode oneChild in data.ChildNodes)
                {
                    foreach (var property in properties)
                    {
                        if (oneChild.Name == property.Name)
                        {
                            object value = Convert.ChangeType(oneChild.InnerXml, property.PropertyType);
                            property.SetValue(obj, value);
                        }
                    }
                    //SetValueIntoParameter(sqlCommand, oneChild.Name, oneChild.InnerXml);
                }

                
            }catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return obj;
        }


        int insertCount = 0;
        int updateCount = 0;
        int deleteCount = 0;
        public async Task<string> ProcessTransactions(XmlDocument xmlTransactions)
        {
            string lastIdOrGuid = string.Empty;
            try
            {
                int lastId = 0;
                XmlNodeList transactionList = xmlTransactions.SelectNodes("Trans/Row");
                foreach (XmlNode oneTransaction in transactionList)
                {
                    count++;
                    string transactionType = ((XmlElement)oneTransaction).GetAttribute("tr");
                    var dataElement = (XmlElement)oneTransaction;
                    var entityName = dataTransferParameter.EntityName;
                    int? resultId = null;

                    switch (transactionType)
                    {
                        case "I":
                            insertCount++;
                            WeakReferenceMessenger.Default.Send(new SyncUpdateMessage(entityName + ": " + count));
                            lastId = await HandleInsertOperation(entityName, dataElement);
                            break;

                        case "U":
                            updateCount++;
                            WeakReferenceMessenger.Default.Send(new SyncUpdateMessage(entityName + ": " + count));
                            await HandleUpdateOperation(entityName, dataElement);
                            break;

                        case "D":
                            deleteCount++;
                            WeakReferenceMessenger.Default.Send(new SyncUpdateMessage(entityName + ": " + count));
                            await HandleDeleteOperation(entityName, dataElement);
                            break;

                        default:
                            throw new ApplicationException(oneTransaction.OuterXml);
                    }

                }
                XmlElement lastElement = (XmlElement)transactionList[transactionList.Count - 1];
                lastIdOrGuid = lastElement.GetAttribute("Id");
                if (string.IsNullOrEmpty(lastIdOrGuid))
                {
                    lastIdOrGuid = lastElement.GetAttribute("IdGuid");
                }
                //InsertDataTransferParameter(dataTransferParameter.EntityName, lastIdOrGuid);
                return lastIdOrGuid;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return lastIdOrGuid;
            }
        }

        private async Task HandleDeleteOperation(string entityName, XmlElement dataElement)
        {
            //int id = int.Parse(dataElement.GetAttribute("Id")); // Assumes 'Id' attribute is the primary key

            switch (entityName)
            {
                case "Customers":
                    var customer = ParseFromXml<Models.Customers>(dataElement);
                    IRepo<Models.Customers> customerRepo = new Repo<Models.Customers>(DBHelper.Database);
                    await customerRepo.Delete(customer);
                    break;

                case "Locations":
                    var location = ParseFromXml<Models.Locations>(dataElement);
                    IRepo<Models.Locations> locationRepo = new Repo<Models.Locations>(DBHelper.Database);
                    await locationRepo.Delete(location);
                    break;
                case "Contacts":
                    var contact = ParseFromXml<Models.Contacts>(dataElement);
                    IRepo<Models.Contacts> conrepo = new Repo<Models.Contacts>(DBHelper.Database);
                    await conrepo.Delete(contact);
                    break;
                case "Users":
                    var user = ParseFromXml<Models.Users>(dataElement);
                    IRepo<Models.Users> userrepo = new Repo<Models.Users>(DBHelper.Database);
                    await userrepo.Delete(user);
                    break;
                case "GenericListDefinitions":
                    var genericListDefinitions = ParseFromXml<Models.GenericListDefinitions>(dataElement);
                    IRepo<Models.GenericListDefinitions> genlistrepo = new Repo<Models.GenericListDefinitions>(DBHelper.Database);
                    await genlistrepo.Delete(genericListDefinitions);
                    break;
                case "GenericLists":
                    var gen = ParseFromXml<Models.GenericLists>(dataElement);
                    IRepo<Models.GenericLists> genrepo = new Repo<Models.GenericLists>(DBHelper.Database);
                    await genrepo.Delete(gen);
                    break;
                case "AuditsForCustomer":
                    var auditcustomer = ParseFromXml<Models.AuditsForCustomer>(dataElement);
                    IRepo<Models.AuditsForCustomer> auditcustomerrepo = new Repo<Models.AuditsForCustomer>(DBHelper.Database);
                    await auditcustomerrepo.Delete(auditcustomer);
                    break;
                case "CustomersForGroup":
                    var customergroup = ParseFromXml<Models.CustomersForGroup>(dataElement);
                    IRepo<Models.CustomersForGroup> customergrouprepo = new Repo<Models.CustomersForGroup>(DBHelper.Database);
                    await customergrouprepo.Delete(customergroup);
                    break;
                case "GroupsForUser":
                    var groupsForUser = ParseFromXml<Models.GroupsForUser>(dataElement);
                    IRepo<Models.GroupsForUser> groupsForUserrepo = new Repo<Models.GroupsForUser>(DBHelper.Database);
                    await groupsForUserrepo.Delete(groupsForUser);
                    break;
                case "Audits":
                    var audits = ParseFromXml<Models.Audits>(dataElement);
                    IRepo<Models.Audits> auditsrepor = new Repo<Models.Audits>(DBHelper.Database);
                    await auditsrepor.Delete(audits);
                    break;
                case "WorkOrderDefinitions":
                    var workOrderDefinitions = ParseFromXml<Models.WorkOrderDefinitions>(dataElement);
                    IRepo<Models.WorkOrderDefinitions> workOrderDefinitionsRepo = new Repo<Models.WorkOrderDefinitions>(DBHelper.Database);
                    await workOrderDefinitionsRepo.Delete(workOrderDefinitions);
                    break;
                case "WorkOrders":
                    var workOrders = ParseFromXml<Models.WorkOrders>(dataElement);
                    IRepo<Models.WorkOrders> workOrdersRepo = new Repo<Models.WorkOrders>(DBHelper.Database);
                    await workOrdersRepo.Delete(workOrders);
                    break;
                case "WorkOrderAttachments":
                    var workOrderAttachments = ParseFromXml<Models.WorkOrderAttachments>(dataElement);
                    IRepo<Models.WorkOrderAttachments> workOrderAttachmentsRepo = new Repo<Models.WorkOrderAttachments>(DBHelper.Database);
                    await workOrderAttachmentsRepo.Delete(workOrderAttachments);
                    break;
                case "GenericAttachments":
                    var genericAttachments = ParseFromXml<Models.GenericAttachments>(dataElement);
                    IRepo<Models.GenericAttachments> genericAttachmentsRepo = new Repo<Models.GenericAttachments>(DBHelper.Database);
                    await genericAttachmentsRepo.Delete(genericAttachments);
                    break;
                case "AuditsForLocation":
                    var auditsForLocation = ParseFromXml<Models.AuditsForLocation>(dataElement);
                    IRepo<Models.AuditsForLocation> auditsForLocationRepo = new Repo<Models.AuditsForLocation>(DBHelper.Database);
                    await auditsForLocationRepo.Delete(auditsForLocation);
                    break;
                case "DataOrganisations":
                    var dataOrganisations = ParseFromXml<Models.DataOrganisations>(dataElement);
                    IRepo<Models.DataOrganisations> dataOrganisationsRepo = new Repo<Models.DataOrganisations>(DBHelper.Database);
                    await dataOrganisationsRepo.Delete(dataOrganisations);
                    break;
            }
        }


        private async Task HandleUpdateOperation(string entityName, XmlElement dataElement)
        {
            switch (entityName)
            {
                case "Customers":
                    var customer = ParseFromXml<Models.Customers>(dataElement);
                    IRepo<Models.Customers> customerRepo = new Repo<Models.Customers>(DBHelper.Database);
                    await customerRepo.Update(customer);
                    break;
                case "Locations":
                    var location = ParseFromXml<Models.Locations>(dataElement);
                    IRepo<Models.Locations> locationRepo = new Repo<Models.Locations>(DBHelper.Database);
                    await locationRepo.Update(location);
                    break;
                case "Contacts":
                    var contact = ParseFromXml<Models.Contacts>(dataElement);
                    IRepo<Models.Contacts> conrepo = new Repo<Models.Contacts>(DBHelper.Database);
                    await conrepo.Update(contact);
                    break;
                case "Users":
                    var user = ParseFromXml<Models.Users>(dataElement);
                    IRepo<Models.Users> userrepo = new Repo<Models.Users>(DBHelper.Database);
                    await userrepo.Update(user);
                    break;
                case "GenericListDefinitions":
                    var genericListDefinitions = ParseFromXml<Models.GenericListDefinitions>(dataElement);
                    IRepo<Models.GenericListDefinitions> genlistrepo = new Repo<Models.GenericListDefinitions>(DBHelper.Database);
                    await genlistrepo.Update(genericListDefinitions);
                    break;
                case "GenericLists":
                    var gen = ParseFromXml<Models.GenericLists>(dataElement);
                    IRepo<Models.GenericLists> genrepo = new Repo<Models.GenericLists>(DBHelper.Database);
                    await genrepo.Update(gen);
                    break;
                case "AuditsForCustomer":
                    var auditcustomer = ParseFromXml<Models.AuditsForCustomer>(dataElement);
                    IRepo<Models.AuditsForCustomer> auditcustomerrepo = new Repo<Models.AuditsForCustomer>(DBHelper.Database);
                    await auditcustomerrepo.Update(auditcustomer);
                    break;
                case "CustomersForGroup":
                    var customergroup = ParseFromXml<Models.CustomersForGroup>(dataElement);
                    IRepo<Models.CustomersForGroup> customergrouprepo = new Repo<Models.CustomersForGroup>(DBHelper.Database);
                    await customergrouprepo.Update(customergroup);
                    break;
                case "GroupsForUser":
                    var groupsForUser = ParseFromXml<Models.GroupsForUser>(dataElement);
                    IRepo<Models.GroupsForUser> groupsForUserrepo = new Repo<Models.GroupsForUser>(DBHelper.Database);
                    await groupsForUserrepo.Update(groupsForUser);
                    break;
                case "Audits":
                    var audits = ParseFromXml<Models.Audits>(dataElement);
                    IRepo<Models.Audits> auditsrepor = new Repo<Models.Audits>(DBHelper.Database);
                    await auditsrepor.Update(audits);
                    break;
                case "WorkOrderDefinitions":
                    var workOrderDefinitions = ParseFromXml<Models.WorkOrderDefinitions>(dataElement);
                    IRepo<Models.WorkOrderDefinitions> workOrderDefinitionsRepo = new Repo<Models.WorkOrderDefinitions>(DBHelper.Database);
                    await workOrderDefinitionsRepo.Update(workOrderDefinitions);
                    break;
                case "WorkOrders":
                    var workOrders = ParseFromXml<Models.WorkOrders>(dataElement);
                    IRepo<Models.WorkOrders> workOrdersRepo = new Repo<Models.WorkOrders>(DBHelper.Database);
                    await workOrdersRepo.Update(workOrders);
                    break;
                case "WorkOrderAttachments":
                    var workOrderAttachments = ParseFromXml<Models.WorkOrderAttachments>(dataElement);
                    IRepo<Models.WorkOrderAttachments> workOrderAttachmentsRepo = new Repo<Models.WorkOrderAttachments>(DBHelper.Database);
                    await workOrderAttachmentsRepo.Update(workOrderAttachments);
                    break;
                case "GenericAttachments":
                    var genericAttachments = ParseFromXml<Models.GenericAttachments>(dataElement);
                    IRepo<Models.GenericAttachments> genericAttachmentsRepo = new Repo<Models.GenericAttachments>(DBHelper.Database);
                    await genericAttachmentsRepo.Update(genericAttachments);
                    break;
                case "AuditsForLocation":
                    var auditsForLocation = ParseFromXml<Models.AuditsForLocation>(dataElement);
                    IRepo<Models.AuditsForLocation> auditsForLocationRepo = new Repo<Models.AuditsForLocation>(DBHelper.Database);
                    await auditsForLocationRepo.Update(auditsForLocation);
                    break;
                case "DataOrganisations":
                    var dataOrganisations = ParseFromXml<Models.DataOrganisations>(dataElement);
                    IRepo<Models.DataOrganisations> dataOrganisationsRepo = new Repo<Models.DataOrganisations>(DBHelper.Database);
                    await dataOrganisationsRepo.Update(dataOrganisations);
                    break;
                default:
                    break;
            }
        }


        private async Task<int> HandleInsertOperation(string entityName, XmlElement dataElement)
        {
            int lastId;
            switch (dataTransferParameter.EntityName)
            {
                case "Customers":
                    var customers = ParseFromXml<Models.Customers>(dataElement);
                    IRepo<Models.Customers> repo = new Repo<Models.Customers>(DBHelper.Database);
                    lastId = await repo.Insert(customers);
                    break;
                case "Locations":
                    var location = ParseFromXml<Models.Locations>(dataElement);
                    IRepo<Models.Locations> locrepo = new Repo<Models.Locations>(DBHelper.Database);
                    lastId = await locrepo.Insert(location);
                    break;
                case "Contacts":
                    var contact = ParseFromXml<Models.Contacts>(dataElement);
                    IRepo<Models.Contacts> conrepo = new Repo<Models.Contacts>(DBHelper.Database);
                    lastId = await conrepo.Insert(contact);
                    break;
                case "Users":
                    var user = ParseFromXml<Models.Users>(dataElement);
                    IRepo<Models.Users> userrepo = new Repo<Models.Users>(DBHelper.Database);
                    lastId = await userrepo.Insert(user);
                    break;
                case "GenericListDefinitions":
                    var genericListDefinitions = ParseFromXml<Models.GenericListDefinitions>(dataElement);
                    IRepo<Models.GenericListDefinitions> genlistrepo = new Repo<Models.GenericListDefinitions>(DBHelper.Database);
                    lastId = await genlistrepo.Insert(genericListDefinitions);
                    break;
                case "GenericLists":
                    var gen = ParseFromXml<Models.GenericLists>(dataElement);
                    IRepo<Models.GenericLists> genrepo = new Repo<Models.GenericLists>(DBHelper.Database);
                    lastId = await genrepo.Insert(gen);
                    break;
                case "AuditsForCustomer":
                    var auditcustomer = ParseFromXml<Models.AuditsForCustomer>(dataElement);
                    IRepo<Models.AuditsForCustomer> auditcustomerrepo = new Repo<Models.AuditsForCustomer>(DBHelper.Database);
                    lastId = await auditcustomerrepo.Insert(auditcustomer);
                    break;
                case "CustomersForGroup":
                    var customergroup = ParseFromXml<Models.CustomersForGroup>(dataElement);
                    IRepo<Models.CustomersForGroup> customergrouprepo = new Repo<Models.CustomersForGroup>(DBHelper.Database);
                    lastId = await customergrouprepo.Insert(customergroup);
                    break;
                case "GroupsForUser":
                    var groupsForUser = ParseFromXml<Models.GroupsForUser>(dataElement);
                    IRepo<Models.GroupsForUser> groupsForUserrepo = new Repo<Models.GroupsForUser>(DBHelper.Database);
                    lastId = await groupsForUserrepo.Insert(groupsForUser);
                    break;
                case "Audits":
                    var audits = ParseFromXml<Models.Audits>(dataElement);
                    IRepo<Models.Audits> auditsrepor = new Repo<Models.Audits>(DBHelper.Database);
                    lastId = await auditsrepor.Insert(audits);
                    break;
                case "WorkOrderDefinitions":
                    var workOrderDefinitions = ParseFromXml<Models.WorkOrderDefinitions>(dataElement);
                    IRepo<Models.WorkOrderDefinitions> workOrderDefinitionsRepo = new Repo<Models.WorkOrderDefinitions>(DBHelper.Database);
                    lastId = await workOrderDefinitionsRepo.Insert(workOrderDefinitions);
                    break;
                case "WorkOrders":
                    var workOrders = ParseFromXml<Models.WorkOrders>(dataElement);
                    IRepo<Models.WorkOrders> workOrdersRepo = new Repo<Models.WorkOrders>(DBHelper.Database);
                    lastId = await workOrdersRepo.Insert(workOrders);
                    break;
                case "WorkOrderAttachments":
                    var workOrderAttachments = ParseFromXml<Models.WorkOrderAttachments>(dataElement);
                    IRepo<Models.WorkOrderAttachments> workOrderAttachmentsRepo = new Repo<Models.WorkOrderAttachments>(DBHelper.Database);
                    lastId = await workOrderAttachmentsRepo.Insert(workOrderAttachments);
                    break;
                case "GenericAttachments":
                    var genericAttachments = ParseFromXml<Models.GenericAttachments>(dataElement);
                    genericAttachments.NeedToDownload = 1;
                    IRepo<Models.GenericAttachments> genericAttachmentsRepo = new Repo<Models.GenericAttachments>(DBHelper.Database);
                    lastId = await genericAttachmentsRepo.Insert(genericAttachments);
                    break;
                case "AuditsForLocation":
                    var auditsForLocation = ParseFromXml<Models.AuditsForLocation>(dataElement);
                    IRepo<Models.AuditsForLocation> auditsForLocationRepo = new Repo<Models.AuditsForLocation>(DBHelper.Database);
                    lastId = await auditsForLocationRepo.Insert(auditsForLocation);
                    break;
                case "DataOrganisations":
                    var dataOrganisations = ParseFromXml<Models.DataOrganisations>(dataElement);
                    IRepo<Models.DataOrganisations> dataOrganisationsRepo = new Repo<Models.DataOrganisations>(DBHelper.Database);
                    lastId = await dataOrganisationsRepo.Insert(dataOrganisations);
                    break;
                default:
                    lastId = 0;
                    break;
            }
            return lastId;
        }

        public void InsertDataTransferParameter(string currentEntity, string lastIdOrGuid)
        {
            Models.DataTransferParameters dataTransferParameters = new Models.DataTransferParameters();
            dataTransferParameters.EntityType = currentEntity;
            dataTransferParameters.LastTransactionDate = DateTime.Now;
            try
            {
                var res  = Int32.Parse(lastIdOrGuid);
                dataTransferParameters.LastId = res;
                dataTransferParameters.LastGuid = Guid.Empty;
            }
            catch(Exception ex)
            {
                dataTransferParameters.LastId = -1;
                dataTransferParameters.LastGuid = new Guid(lastIdOrGuid);
            }
            
            dataTransferParameters.BaseTransactionDate = DateTime.Now;

            IRepo<Models.DataTransferParameters> dataTransferParametersRepo = new Repo<Models.DataTransferParameters>(DBHelper.Database);
            dataTransferParametersRepo.Insert(dataTransferParameters);

        }
        bool mustSignalCompletion;

        public async Task SyncDataToServer()
        {
            List<AuditsInProgress> auditList = await syncDataHandler.GetPendingAuditInProgress();

            foreach (AuditsInProgress audit in auditList)
            {
                List<Attachments> attachmentList = await syncDataHandler.GetAuditsInProgressAttachments(audit.IdGuid);
                var finalXMLString = AddHeaderToXml(audit.XmlResults,audit.UserId,audit.CustomerId,audit.AuditId,audit.LocationId,audit.WorkOrderId,audit.DateOfAudit,audit.DateTimeStarted);

                XmlDocument resultDoc = new XmlDocument();
                resultDoc.LoadXml("<root/>");
                XmlNode oneRes = resultDoc.CreateElement("oneres");
                XmlElement id = resultDoc.CreateElement("id");
                id.InnerText = audit.IdGuid.ToString();
                XmlElement res = resultDoc.CreateElement("res");
                res.InnerXml = finalXMLString;
                XmlElement keepWOOpen = resultDoc.CreateElement("woKeepOpen");
                bool keepOpen = true;
                keepWOOpen.InnerXml = (keepOpen) ? "1" : "0";
                XmlElement atts = resultDoc.CreateElement("hasAtts");
                atts.InnerXml = (attachmentList.Count>0) ? "1" : "0";
                if (attachmentList.Count>0)
                {
                    mustSignalCompletion = true;
                }
                oneRes.AppendChild(id);
                oneRes.AppendChild(atts);
                oneRes.AppendChild(keepWOOpen);
                oneRes.AppendChild(res);
                resultDoc.DocumentElement.AppendChild(oneRes);
                //Console.WriteLine("Uploading pending " + i);

                var m_configXml = new XmlConfigDoc(Helper.Constants.APPLICATION_CONFIG_FILE);
                urlList = XMLHelper.URLList(m_configXml);



                string uploadUrl = $"{urlList[0]}MD_{Helper.Constants.RESULTS_FILENAME}/";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uploadUrl);
                request.Method = "PUT";
                request.AllowWriteStreamBuffering = true;
                request.Headers.Add("Ticket:" + Preferences.Get("Token", ""));
                request.Timeout = 600000;

                m_requestState.Init(request);

                //string resOuterXML = "<root><oneres><id>1ce0d1fe-205e-4dc5-be52-3c3476ee23e7</id><hasAtts>1</hasAtts><woKeepOpen>1</woKeepOpen><res><results><Header User=\"6664\" Customer=\"7722\" Audit=\"15484\" Location=\"11712\" WorkOrder=\"4525\" DateOfAudit=\"2025-02-07T11:42:49\" DateTimeStarted=\"2025-02-07T11:42:49\" DateTimeReleased=\"2025-02-11T22:34:51\" ReleaseStatus=\"5\" Platform=\"Android\" /><aud_2><aud_2_1><aud_2_1_1><locationList res=\"\" /><asset res=\"\" /><frAR res=\"258175\" score=\"0\" /><level res=\"Fg\" /><aud_2_1_1_14 res=\"$GPSLocation[network 51.396848,0.097389 hAcc=85 et=+2d23h57m5s186ms alt=132.60000610351562 vAcc=15 sAcc=??? bAcc=??? {Bundle[mParcelledData.dataSize=52]}]\" /></aud_2_1_1><aud_2_1_3><sealType res=\"700105\" score=\"0\" count=\"0\" /><x res=\"5\" count=\"0\" /><y res=\"6\" count=\"0\" /><totQty res=\"8\" count=\"0\" /></aud_2_1_3><aud_2_1_5><penetratingServices res=\"\" count=\"0\" /></aud_2_1_5><aud_2_1_4><photo res=\"1\" /><aud_2_1_4_7 res=\"\" /><photo0 res=\"1\" /><aud_2_1_4_8 res=\"\" /><comments res=\"\" /><wComp res=\"2025-02-11T00:00:00\" /></aud_2_1_4></aud_2_1><aud_2_2><aud_2_1_2><siteFile res=\"\" /><docs res=\"\" /></aud_2_1_2><aud_2_3_1><drawingLocation res=\"\" /></aud_2_3_1><calcs /></aud_2_2></aud_2></results></res></oneres></root>\r\n";
                byte[] rdr = Encoding.UTF8.GetBytes(resultDoc.OuterXml);//(resOuterXML);//(resultDoc.OuterXml);

                using (Stream reqStream = request.GetRequestStream())
                {
                    GZipOutputStream zipStream = new GZipOutputStream(reqStream); // don't seem able to Dispose of this directly (dispose, calls Close which seems to end up flushing and giving a ContentLength error) 
                    zipStream.Write(rdr, 0, rdr.GetLength(0));
                    zipStream.Close();

                    var response = await request.GetResponseAsync();

                    webApiService.CheckProtocol(response);
                    string failCode = response.Headers["FailCode"];
                    if (failCode == null)
                    {
                        using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
                        {
                            var responseLine = responseReader.ReadLine();
                            Console.WriteLine(responseLine);
                            //return responseLine;

                            if (attachmentList.Count > 0)
                            {
                                bool attachSuccess = true;
                                foreach (var item in attachmentList)
                                {
                                    
                                    int repeat = 0;
                                    
                                    byte[] attachmentData = await IO.ReadFullFile((string)item.FileName);

                                    await UploadAttachment($"{urlList[0]}", item.IdGuid, item.UniqueName, attachmentData, repeat,
                                       item.FileName,
                                        item.SubFormIdGuid,
                                        item.IsCopiedFromWorkOrder == 1 ?true:false, item.Id, -1);

                                    
                                }
                                if (mustSignalCompletion)
                                {
                                    await SignalCompletionOfResultAndAttachments($"{urlList[0]}", audit.IdGuid.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        m_requestState.result = WebRequestResult.RESPONSE_ERROR;
                        Console.WriteLine("Error with fail code UploadDataToServer " + failCode);
                        throw new ApplicationException("Request failed: " + failCode);
                    }
                }

                await syncDataHandler.UpdateAuditInProgressStatus(audit.IdGuid, ResultStatusEnum.COMPLETE);
            }

        }

        public string AddHeaderToXml(string xmlString,int userId,int customerId,int auditId,int locationId,int workOrderId,DateTime dateOfAudit, DateTime dateTimeStarted)
        {
            // Load XML string into XDocument
            XDocument doc = XDocument.Parse(xmlString);

            // Find the <results> node
            XElement resultsNode = doc.Descendants("results").FirstOrDefault();

            if (resultsNode != null)
            {
                // Create the new <Header> element
                XElement headerElement = new XElement("Header",
                    new XAttribute("User", userId),
                    new XAttribute("Customer", customerId),
                    new XAttribute("Audit", auditId),
                    new XAttribute("Location", locationId),
                    new XAttribute("WorkOrder", workOrderId),
                    new XAttribute("DateOfAudit", dateOfAudit),
                    new XAttribute("DateTimeStarted", dateTimeStarted),
                    new XAttribute("DateTimeReleased", DateTime.Now),
                    new XAttribute("ReleaseStatus", "5"),
                    new XAttribute("Platform", DeviceInfo.Current.Platform)
                );

                // Insert <Header> as the first child of <results>
                resultsNode.AddFirst(headerElement);
            }

            // Return updated XML as string
            return doc.ToString();
        }

        public async Task<int> UploadDataToServer(string strRequest,Guid uploadGuid)
        {
            try
            {
                List<AuditsInProgress> auditList = await syncDataHandler.GetPendingAuditInProgress();

                using (SqlCeCommand sqlAttachments = syncDataHandler.GetAttachmentsAwaitingUploadCommand(uploadGuid))
                {
                    using (SqlCeDataReader attachments = sqlAttachments.ExecuteReader())
                    {
                        //get ready the xml to send to server
                        XmlDocument resultDoc = new XmlDocument();
                        resultDoc.LoadXml("<root/>");
                        XmlNode oneRes = resultDoc.CreateElement("oneres");
                        XmlElement id = resultDoc.CreateElement("id");
                        id.InnerText = uploadGuid.ToString();
                        XmlElement res = resultDoc.CreateElement("res");
                        res.InnerXml = strRequest;
                        XmlElement keepWOOpen = resultDoc.CreateElement("woKeepOpen");
                        bool keepOpen = true;
                        keepWOOpen.InnerXml = (keepOpen) ? "1" : "0";
                        XmlElement atts = resultDoc.CreateElement("hasAtts");
                        atts.InnerXml = (attachments.HasRows == true) ? "1" : "0";
                         if (attachments.HasRows == true)
                         {
                            mustSignalCompletion = true;
                         }
                        oneRes.AppendChild(id);
                        oneRes.AppendChild(atts);
                        oneRes.AppendChild(keepWOOpen);
                        oneRes.AppendChild(res);
                        resultDoc.DocumentElement.AppendChild(oneRes);
                        //Console.WriteLine("Uploading pending " + i);

                        var m_configXml = new XmlConfigDoc(Helper.Constants.APPLICATION_CONFIG_FILE);
                        urlList = XMLHelper.URLList(m_configXml);



                        string uploadUrl = $"{urlList[0]}MD_{Helper.Constants.RESULTS_FILENAME}/";
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uploadUrl);
                        request.Method = "PUT";
                        request.AllowWriteStreamBuffering = true;
                        request.Headers.Add("Ticket:" + Preferences.Get("Token", ""));
                        request.Timeout = 600000;

                        m_requestState.Init(request);

                        //string resOuterXML = "<root><oneres><id>1ce0d1fe-205e-4dc5-be52-3c3476ee23e7</id><hasAtts>1</hasAtts><woKeepOpen>1</woKeepOpen><res><results><Header User=\"6664\" Customer=\"7722\" Audit=\"15484\" Location=\"11712\" WorkOrder=\"4525\" DateOfAudit=\"2025-02-07T11:42:49\" DateTimeStarted=\"2025-02-07T11:42:49\" DateTimeReleased=\"2025-02-11T22:34:51\" ReleaseStatus=\"5\" Platform=\"Android\" /><aud_2><aud_2_1><aud_2_1_1><locationList res=\"\" /><asset res=\"\" /><frAR res=\"258175\" score=\"0\" /><level res=\"Fg\" /><aud_2_1_1_14 res=\"$GPSLocation[network 51.396848,0.097389 hAcc=85 et=+2d23h57m5s186ms alt=132.60000610351562 vAcc=15 sAcc=??? bAcc=??? {Bundle[mParcelledData.dataSize=52]}]\" /></aud_2_1_1><aud_2_1_3><sealType res=\"700105\" score=\"0\" count=\"0\" /><x res=\"5\" count=\"0\" /><y res=\"6\" count=\"0\" /><totQty res=\"8\" count=\"0\" /></aud_2_1_3><aud_2_1_5><penetratingServices res=\"\" count=\"0\" /></aud_2_1_5><aud_2_1_4><photo res=\"1\" /><aud_2_1_4_7 res=\"\" /><photo0 res=\"1\" /><aud_2_1_4_8 res=\"\" /><comments res=\"\" /><wComp res=\"2025-02-11T00:00:00\" /></aud_2_1_4></aud_2_1><aud_2_2><aud_2_1_2><siteFile res=\"\" /><docs res=\"\" /></aud_2_1_2><aud_2_3_1><drawingLocation res=\"\" /></aud_2_3_1><calcs /></aud_2_2></aud_2></results></res></oneres></root>\r\n";
                        byte[] rdr = Encoding.UTF8.GetBytes(resultDoc.OuterXml);//(resOuterXML);//(resultDoc.OuterXml);
                        using (Stream reqStream = request.GetRequestStream())
                        {
                            GZipOutputStream zipStream = new GZipOutputStream(reqStream); // don't seem able to Dispose of this directly (dispose, calls Close which seems to end up flushing and giving a ContentLength error) 
                            zipStream.Write(rdr, 0, rdr.GetLength(0));
                            zipStream.Close();

                            var response = await request.GetResponseAsync();

                            webApiService.CheckProtocol(response);
                            string failCode = response.Headers["FailCode"];
                            if (failCode == null)
                            {
                                using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
                                {
                                    var responseLine = responseReader.ReadLine();
                                    Console.WriteLine(responseLine);
                                    //return responseLine;

                                    if (attachments.HasRows == true)
                                    {
                                        bool attachSuccess = true;
                                        while ((attachSuccess == true) && (attachments.Read() == true))
                                        {
                                            //j++;
                                            //SyncLog("Uploading att " + j);
                                            //SendMessageBackToParentUI(GetResourceString(DataTransferResources.CLIENTDT_Attachment) + j);
                                            int repeat = 0;
                                            //if (attachments["Repeat"] != DBNull.Value)
                                            //{
                                              //  repeat = DataHandler.GetIntFromReader(attachments["Repeat"]);
                                            //}
                                            //int pageRepeatListItemId = -1;
                                            //if (attachments["PageRepeatListItemId"] != DBNull.Value)
                                            //{
                                              //  pageRepeatListItemId = DataHandler.GetIntFromReader(attachments["PageRepeatListItemId"]);
                                            //}

                                            byte[] attachmentData = await IO.ReadFullFile((string)attachments["AttachmentData"]);

                                            await UploadAttachment($"{urlList[0]}", uploadGuid, (string)attachments["UniqueName"], attachmentData, repeat,
                                                attachments["FileName"] != DBNull.Value ? (string)attachments["FileName"] : "",
                                                attachments["SubFormIdGuid"] != DBNull.Value ? GetGuidFromReader(attachments["SubFormIdGuid"]) : Guid.Empty,
                                                attachments["IsCopiedFromWorkOrder"] != DBNull.Value ? GetBoolFromReader(attachments["IsCopiedFromWorkOrder"]) : false, attachments["Id"] != DBNull.Value ? GetIntFromReader(attachments["Id"]) : 0, -1);
                                            //if (m_requestState.exception != null)
                                            //{
                                            //    //SyncLog("About to throw exception: " + m_requestState.exception);
                                            //    throw m_requestState.exception;
                                            //}
                                            //DebugLog("Done att " + j);
                                            
                                            //DebugLog("Uploaded att " + j);
                                        }
                                        if (mustSignalCompletion)
                                        {
                                            await SignalCompletionOfResultAndAttachments($"{urlList[0]}",uploadGuid.ToString());
                                        }
                                    }
                                }
                            }
                            else
                            {
                                m_requestState.result = WebRequestResult.RESPONSE_ERROR;
                                Console.WriteLine("Error with fail code UploadDataToServer " + failCode);
                                throw new ApplicationException("Request failed: " + failCode);
                            }
                        }
                    }
                }

                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine($"UploadDataToServer : {e.Message}");
                return -1;
            }
        }

        public async Task UploadAttachment(string url,Guid resultGuid, string uniqueName, byte[] attachmentBytes, int repeat, string filePath, Guid subFormGuid, bool isCopiedFromWorkOrder, int localId, int pageRepeatListItemId)
        {
            try
            {
                //string filePath = fileName;
                string fileName = Path.GetFileName(filePath);
                string queryString = "?guid=" + resultGuid.ToString() + "&uniqueName=" + uniqueName + "&fileName=";
                if (repeat != -1)
                {
                    queryString += "&repeat=" + repeat;
                }
                if (pageRepeatListItemId != -1)
                {
                    queryString += "&pageRepeatListItemId=" + pageRepeatListItemId;
                }
                if (subFormGuid != Guid.Empty)
                {
                    queryString += "&subForm=" + subFormGuid.ToString();
                }
                if (isCopiedFromWorkOrder)
                {
                    queryString += "&isCopiedFromWorkOrder=1";
                }
                queryString += "&localId=" + localId;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + Constants.ATTACHMENTS_FILENAME + queryString);
                request.Method = "PUT";
                request.AllowWriteStreamBuffering = true;

                request.KeepAlive = false; // back in 23/06/12 bottom of https://bugzilla.novell.com/show_bug.cgi?id=648862#c12
                request.Timeout = 600000;//Xml.XmlUtils.IntAtt(m_configSettings, "uploadRequestTimeoutSecs", 120) * 1000;
                request.ReadWriteTimeout = 600000;

                request.Headers.Add("Ticket:" + Preferences.Get("Token", ""));
                m_requestState.Init(request);
                using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(600000)))
                {
                    try
                    {
                        // Check if image needs processing (especially for camera images)
                        if (attachmentBytes.Length > 65000) // If larger than ~1MB
                        {
                            attachmentBytes = await CompressImageForDatabaseLimit(attachmentBytes);
                        }
                        using (var memoryBuffer = new MemoryStream(attachmentBytes))
                        {
                            using (Stream reqStream = request.GetRequestStream())
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead;
                                long totalSent = 0;

                                while ((bytesRead = await memoryBuffer.ReadAsync(buffer, 0, buffer.Length, cts.Token)) > 0)
                                {
                                    await reqStream.WriteAsync(buffer, 0, bytesRead, cts.Token);
                                    totalSent += bytesRead;
                                    Console.WriteLine($"Bytes sent: {totalSent} of {attachmentBytes.Length}");
                                    // Add a small delay to prevent flooding
                                    if (totalSent % (buffer.Length * 10) == 0)
                                        await Task.Delay(1, cts.Token);
                                }
                            }
                        }
                        // Get response with timeout
                        using (var response = await request.GetResponseAsync().WaitAsync(cts.Token))
                        {
                            webApiService.CheckProtocol(response);
                            string failCode = response.Headers["FailCode"];
                            if (failCode == null)
                            {
                                Console.WriteLine("Passcode",failCode);
                                syncDataHandler.UpdateAttachmentUploadStatus(localId, AttachmentStatusEnum.UPLOADED);

                            }
                            else
                            {
                                Console.WriteLine("Failcode", failCode);

                            }
                            using (var responseStream = response.GetResponseStream())
                            using (var reader = new StreamReader(responseStream))
                            {
                                string responseContent = await reader.ReadToEndAsync();
                                Console.WriteLine("Response", responseContent);
                                //return !string.IsNullOrEmpty(responseContent);
                            }
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine("Upload request timed out");
                        throw new TimeoutException("The request timed out while trying to upload the image");
                    }
                }
                //using (Stream reqStream = request.GetRequestStream())
                //{
                //    MemoryStream rdr = new MemoryStream(attachmentBytes);
                //    // Allocate byte buffer to hold file contents
                //    byte[] inData = new byte[4096];
                //    // loop through the local file reading each data block
                //    //  and writing to the request stream buffer
                //    int totalSent = 0;
                //    int bytesRead = rdr.Read(inData, 0, inData.Length);
                //    while (bytesRead > 0)
                //    {
                //        reqStream.Write(inData, 0, bytesRead);
                //        totalSent += bytesRead;
                //        Console.WriteLine("Bytes sent: " + totalSent);
                //        bytesRead = rdr.Read(inData, 0, inData.Length);
                //    }
                //    rdr.Close();
                //    reqStream.Close();

                //    // Start the asynchronous request.
                //    var res = await request.GetResponseAsync();

                //    webApiService.CheckProtocol(res);
                //    string failCode = res.Headers["FailCode"];
                //    if (failCode == null)
                //    {
                //        using (StreamReader responseReader = new StreamReader(res.GetResponseStream()))
                //        {
                //            var responseLine = responseReader.ReadLine();
                //            Console.WriteLine(responseLine);

                //            try
                //            {
                //                //syncDataHandler.UpdateAttachmentUploadStatus(localId, (int)DataEnum.AttachmentStatusEnum.UPLOADED);
                //            }
                //            catch (Exception e)
                //            {
                //                Console.WriteLine("Failed to update attachment status: " + e.Message); // malformed db??

                //            }
                //        }
                //    }
                //    else
                //    {
                //        m_requestState.result = WebRequestResult.RESPONSE_ERROR;
                //        Console.WriteLine("Error with fail code " + failCode);
                //        throw new ApplicationException("Request failed: " + failCode);
                //    }
                //            //SetTimeout(asyncResult);

                //            // The response came in the allowed time. The work processing will happen in the 
                //            // callback function.
                //            //DebugLog("Waiting on allDone");
                //            //allDone.WaitOne();
                //            //DebugLog("Finished waiting");
                //        }
            }
            //catch (Exception e)
            //{
            //    m_requestState.result = WebRequestResult.REQUEST_ERROR;
            //    m_requestState.exception = e;
            //    Console.WriteLine("Exception in UploadAttachment: " + e.ToString());
            //}
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var errorResponse = ex.Response)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            string errorText = await reader.ReadToEndAsync();
                            Console.WriteLine($"Error response: {errorText}");
                        }
                    }
                }
                Console.WriteLine($"WebException: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during upload: {ex.Message}");
                throw;
            }
        }

        public async Task SignalCompletionOfResultAndAttachments(string url,string resultGuid)
        {
            string queryString = "?guid=" + resultGuid.ToString();
            //SendRequest(Constants.SIGNAL_END_OF_RESULTS_FILENAME, queryString, new AsyncCallback(UploadCallback), resultGuid.ToString());

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{url}MD_{Constants.SIGNAL_END_OF_RESULTS_FILENAME}{queryString}");
                request.Method = "PUT";
                request.AllowWriteStreamBuffering = true;
                request.Headers.Add("Ticket:" + Preferences.Get("Token", ""));

                request.Timeout = 600000; // !!!!!!***!!!!
                m_requestState.Init(request);

                byte[] rdr = System.Text.Encoding.UTF8.GetBytes(resultGuid);
                using (Stream reqStream = request.GetRequestStream())
                {
                    GZipOutputStream zipStream = new GZipOutputStream(reqStream); // don't seem able to Dispose of this directly (dispose, calls Close which seems to end up flushing and giving a ContentLength error) 
                    zipStream.Write(rdr, 0, rdr.GetLength(0));
                    zipStream.Close();
                    // Start the asynchronous request.
                    //allDone.Reset();

                    var response = await request.GetResponseAsync();
                    //SetTimeout(asyncResult);
                    //allDone.WaitOne();

                    webApiService.CheckProtocol(response);
                    string failCode = response.Headers["FailCode"];
                    if (failCode == null)
                    {
                        using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
                        {
                            var responseLine = responseReader.ReadLine();
                            Console.WriteLine(responseLine);
                        }
                    }
                    else
                    {
                        m_requestState.result = WebRequestResult.RESPONSE_ERROR;
                        //DebugLog("Error with fail code " + failCode);
                        throw new ApplicationException("Request failed: " + failCode);
                    }
                }
                //Stream s = new GZipOutputStream(request.GetRequestStream());
                //s.Write(rdr, 0, rdr.GetLength(0));
                //s.Close();
                //m_requestState.Init(request);
                //m_requestState.request = request;
                //allDone.Reset();



                //// Start the asynchronous request.
                //IAsyncResult result = (IAsyncResult)request.BeginGetResponse(callback, m_requestState);
                //allDone.WaitOne();
            }
            catch (Exception e)
            {
                m_requestState.result = WebRequestResult.REQUEST_ERROR;
                m_requestState.exception = e;
                //DebugLog("SendReq Exception caught!! " + e.ToString());
            }
        }
        //        private void UploadLocalListsCallback(IAsyncResult asynchronousResult)
        //        {
        //            CallbackWithStringResponse(asynchronousResult, -1);
        //        }
        //        private void CallbackWithStringResponse(IAsyncResult asynchronousResult, int messageType)
        //        {
        //            CallbackGeneral(asynchronousResult, true, false, messageType);
        //        }

        //        private void CallbackGeneral(IAsyncResult asynchronousResult, bool setResponseString, bool setTransactions, int messageType)
        //        {
        //            try
        //            {
        //                RequestState myRequestState = (RequestState)asynchronousResult.AsyncState;
        //                HttpWebRequest myHttpWebRequest = myRequestState.request;
        //                using (HttpWebResponse response = (HttpWebResponse)myHttpWebRequest.EndGetResponse(asynchronousResult))
        //                {
        //                    webApiService.CheckProtocol(response);
        //                    string failCode = response.Headers["FailCode"];
        //                    if (failCode == null)
        //                    {
        //                        if (setResponseString)
        //                        {
        //                            var m_uploadResponse = ResponseAsString(response);
        //                            if (messageType == 1)
        //                            {
        //                                //SetSessionStatus(SessionStatus.FINISHED_NATURALLY);
        //                                //SendMessageBackToParentUI("$res:" + m_uploadResponse);
        //                            }
        //                        }
        //                        if (setTransactions)
        //                        {
        //                            var m_returnedTransactions = ResponseAsString(response);
        //                        }
        //                        m_requestState.result = WebRequestResult.HAVE_DATA;
        //                    }
        //                    else
        //                    {
        //                        m_requestState.result = WebRequestResult.RESPONSE_ERROR;
        //                        //DebugLog("Error with fail code " + failCode);
        //                        throw new ApplicationException("Request failed: " + failCode);
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                if (m_requestState.result != WebRequestResult.USER_ABORTED)
        //                {
        //                    m_requestState.exception = e;
        //                } // else - exception details set in abort request
        //            }
        //            //allDone.Set();
        //        }

        //        private string ResponseAsString(HttpWebResponse response)
        //        {
        //            char[]  m_inData = new char[4096];

        //            using (MemoryStream decompressedResponse = GetDecompressedResponseFromStream(response.GetResponseStream()))
        //            {
        //                using (StreamReader responseReader = new StreamReader(decompressedResponse, Encoding.UTF8))
        //                {
        //                    StringBuilder sb = new StringBuilder();
        //                    int totalLengthReadForLog = 0;
        //                    int charsRead = responseReader.Read(m_inData, 0, m_inData.Length);
        //                    while (charsRead > 0)
        //                    {
        //                        sb.Append(m_inData, 0, charsRead);
        //                        totalLengthReadForLog += charsRead;
        //                        charsRead = responseReader.Read(m_inData, 0, m_inData.Length);
        //                    }
        //                    return sb.ToString();
        //                }
        //            }
        //        }
        //        private GZipInputStream m_compressedStream;
        //        private MemoryStream GetDecompressedResponseFromStream(Stream inStream)
        //        {
        //            FreeCompressedResponse();
        //            m_compressedStream = new GZipInputStream(inStream);
        //            MemoryStream decompressedStream = new MemoryStream();
        //            int totalSize = 0;
        //            int size = 2048;
        //            byte[] writeData = new byte[2048];
        //            while (true)
        //            {
        //                size = m_compressedStream.Read(writeData, 0, size);
        //                totalSize += size;
        //                if (size > 0)
        //                {
        //                    decompressedStream.Write(writeData, 0, size);
        //                }
        //                else
        //                {
        //                    break;
        //                }
        //            }
        //            decompressedStream.Seek(0, SeekOrigin.Begin);
        //            return decompressedStream;
        //        }

        //        private void FreeCompressedResponse()
        //        {
        //            if (m_compressedStream != null)
        //            {
        //#if !PocketPC
        //                m_compressedStream.Close();
        //                m_compressedStream.Dispose();
        //#endif
        //                m_compressedStream = null;
        //            }
        //        }

        private async Task<byte[]> CompressImageForDatabaseLimit(byte[] imageData)
        {
            // Target size slightly below the limit (e.g., 60KB)
            const int targetSize = 120 * 2048;

            try
            {
                using var originalImageStream = new MemoryStream(imageData);
                var skBitmap = SKBitmap.Decode(originalImageStream);
                if (skBitmap == null)
                {
                    Console.WriteLine("Failed to decode image");
                    return imageData;
                }
                // Start with reasonable quality
                int quality = 80;
                byte[] result = imageData;

                // Try increasingly aggressive compression until size is acceptable
                while (result.Length > targetSize && quality > 5)
                {
                    // Calculate appropriate dimensions
                    // Start with 50% reduction and increase as needed
                    double scaleFactor = Math.Min(0.5, Math.Sqrt((double)targetSize / imageData.Length));
                    int newWidth = (int)(skBitmap.Width * scaleFactor);
                    int newHeight = (int)(skBitmap.Height * scaleFactor);

                    // Ensure minimum dimensions
                    newWidth = Math.Max(newWidth, 300);
                    newHeight = Math.Max(newHeight, 300);

                    // Resize image using SkiaSharp
                    var imageInfo = new SKImageInfo(newWidth, newHeight);
                    using var resizedBitmap = skBitmap.Resize(imageInfo, SKFilterQuality.Medium);
                    using var image = SKImage.FromBitmap(resizedBitmap);
                    using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
                    // Get the compressed bytes
                    result = data.ToArray();
                    // Reduce quality for next iteration if needed
                    quality -= 15;
                }

                Console.WriteLine($"Compressed image from {imageData.Length} to {result.Length} bytes");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image compression failed: {ex.Message}");
                return imageData;
            }
        }
    }
}
