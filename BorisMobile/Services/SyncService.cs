using BorisMobile.DataHandler;
using BorisMobile.DataTransferController;
using BorisMobile.Helper;
using BorisMobile.Models;
using BorisMobile.Repository;
using BorisMobile.Utilities;
using BorisMobile.XML;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Xml;
using static BorisMobile.DataHandler.SyncDataHandler;
using RequestState = BorisMobile.DataTransferController.RequestState;

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
                    string sourceFile = Path.Combine(FilesHelper.GetAttachmentDirectoryMAUI(Helper.Constants.APP_NAME), oneAttachment.FileName);
                    if (File.Exists(sourceFile)) // may have been copied already
                    {
                        if (entityType != "MD")
                        {
                            string destFile = Path.Combine(destDir, oneAttachment.FileName);
                            IO.SafeFileDelete(destFile);
                            File.Copy(sourceFile, destFile, true); // even using "true" on Android causes ERROR_ALREADY_EXISTS. Possibly.
                            syncDataHandler.DeleteGenericAttachment(FilesHelper.GetAttachmentDirectoryMAUI(Helper.Constants.APP_NAME), oneAttachment.IdGuid);
                            
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
                string attachmentsDir = FilesHelper.GetAttachmentDirectoryMAUI(Helper.Constants.APP_NAME);
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
            IRepo<Models.DataTransferParameters> dataTransferParameterRepo = new Repo<Models.DataTransferParameters>(App.Database);
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
                IRepo<Models.DataTransferParameters> dataTransferParameterRepo = new Repo<Models.DataTransferParameters>(App.Database);
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
            IRepo<Models.DataTransferParameters> dataTransferParameterRepo = new Repo<Models.DataTransferParameters>(App.Database);
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
                    IRepo<Models.Customers> customerRepo = new Repo<Models.Customers>(App.Database);
                    await customerRepo.Delete(customer);
                    break;

                case "Locations":
                    var location = ParseFromXml<Models.Locations>(dataElement);
                    IRepo<Models.Locations> locationRepo = new Repo<Models.Locations>(App.Database);
                    await locationRepo.Delete(location);
                    break;
                case "Contacts":
                    var contact = ParseFromXml<Models.Contacts>(dataElement);
                    IRepo<Models.Contacts> conrepo = new Repo<Models.Contacts>(App.Database);
                    await conrepo.Delete(contact);
                    break;
                case "Users":
                    var user = ParseFromXml<Models.Users>(dataElement);
                    IRepo<Models.Users> userrepo = new Repo<Models.Users>(App.Database);
                    await userrepo.Delete(user);
                    break;
                case "GenericListDefinitions":
                    var genericListDefinitions = ParseFromXml<Models.GenericListDefinitions>(dataElement);
                    IRepo<Models.GenericListDefinitions> genlistrepo = new Repo<Models.GenericListDefinitions>(App.Database);
                    await genlistrepo.Delete(genericListDefinitions);
                    break;
                case "GenericLists":
                    var gen = ParseFromXml<Models.GenericLists>(dataElement);
                    IRepo<Models.GenericLists> genrepo = new Repo<Models.GenericLists>(App.Database);
                    await genrepo.Delete(gen);
                    break;
                case "AuditsForCustomer":
                    var auditcustomer = ParseFromXml<Models.AuditsForCustomer>(dataElement);
                    IRepo<Models.AuditsForCustomer> auditcustomerrepo = new Repo<Models.AuditsForCustomer>(App.Database);
                    await auditcustomerrepo.Delete(auditcustomer);
                    break;
                case "CustomersForGroup":
                    var customergroup = ParseFromXml<Models.CustomersForGroup>(dataElement);
                    IRepo<Models.CustomersForGroup> customergrouprepo = new Repo<Models.CustomersForGroup>(App.Database);
                    await customergrouprepo.Delete(customergroup);
                    break;
                case "GroupsForUser":
                    var groupsForUser = ParseFromXml<Models.GroupsForUser>(dataElement);
                    IRepo<Models.GroupsForUser> groupsForUserrepo = new Repo<Models.GroupsForUser>(App.Database);
                    await groupsForUserrepo.Delete(groupsForUser);
                    break;
                case "Audits":
                    var audits = ParseFromXml<Models.Audits>(dataElement);
                    IRepo<Models.Audits> auditsrepor = new Repo<Models.Audits>(App.Database);
                    await auditsrepor.Delete(audits);
                    break;
                case "WorkOrderDefinitions":
                    var workOrderDefinitions = ParseFromXml<Models.WorkOrderDefinitions>(dataElement);
                    IRepo<Models.WorkOrderDefinitions> workOrderDefinitionsRepo = new Repo<Models.WorkOrderDefinitions>(App.Database);
                    await workOrderDefinitionsRepo.Delete(workOrderDefinitions);
                    break;
                case "WorkOrders":
                    var workOrders = ParseFromXml<Models.WorkOrders>(dataElement);
                    IRepo<Models.WorkOrders> workOrdersRepo = new Repo<Models.WorkOrders>(App.Database);
                    await workOrdersRepo.Delete(workOrders);
                    break;
                case "WorkOrderAttachments":
                    var workOrderAttachments = ParseFromXml<Models.WorkOrderAttachments>(dataElement);
                    IRepo<Models.WorkOrderAttachments> workOrderAttachmentsRepo = new Repo<Models.WorkOrderAttachments>(App.Database);
                    await workOrderAttachmentsRepo.Delete(workOrderAttachments);
                    break;
                case "GenericAttachments":
                    var genericAttachments = ParseFromXml<Models.GenericAttachments>(dataElement);
                    IRepo<Models.GenericAttachments> genericAttachmentsRepo = new Repo<Models.GenericAttachments>(App.Database);
                    await genericAttachmentsRepo.Delete(genericAttachments);
                    break;
                case "AuditsForLocation":
                    var auditsForLocation = ParseFromXml<Models.AuditsForLocation>(dataElement);
                    IRepo<Models.AuditsForLocation> auditsForLocationRepo = new Repo<Models.AuditsForLocation>(App.Database);
                    await auditsForLocationRepo.Delete(auditsForLocation);
                    break;
                case "DataOrganisations":
                    var dataOrganisations = ParseFromXml<Models.DataOrganisations>(dataElement);
                    IRepo<Models.DataOrganisations> dataOrganisationsRepo = new Repo<Models.DataOrganisations>(App.Database);
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
                    IRepo<Models.Customers> customerRepo = new Repo<Models.Customers>(App.Database);
                    await customerRepo.Update(customer);
                    break;
                case "Locations":
                    var location = ParseFromXml<Models.Locations>(dataElement);
                    IRepo<Models.Locations> locationRepo = new Repo<Models.Locations>(App.Database);
                    await locationRepo.Update(location);
                    break;
                case "Contacts":
                    var contact = ParseFromXml<Models.Contacts>(dataElement);
                    IRepo<Models.Contacts> conrepo = new Repo<Models.Contacts>(App.Database);
                    await conrepo.Update(contact);
                    break;
                case "Users":
                    var user = ParseFromXml<Models.Users>(dataElement);
                    IRepo<Models.Users> userrepo = new Repo<Models.Users>(App.Database);
                    await userrepo.Update(user);
                    break;
                case "GenericListDefinitions":
                    var genericListDefinitions = ParseFromXml<Models.GenericListDefinitions>(dataElement);
                    IRepo<Models.GenericListDefinitions> genlistrepo = new Repo<Models.GenericListDefinitions>(App.Database);
                    await genlistrepo.Update(genericListDefinitions);
                    break;
                case "GenericLists":
                    var gen = ParseFromXml<Models.GenericLists>(dataElement);
                    IRepo<Models.GenericLists> genrepo = new Repo<Models.GenericLists>(App.Database);
                    await genrepo.Update(gen);
                    break;
                case "AuditsForCustomer":
                    var auditcustomer = ParseFromXml<Models.AuditsForCustomer>(dataElement);
                    IRepo<Models.AuditsForCustomer> auditcustomerrepo = new Repo<Models.AuditsForCustomer>(App.Database);
                    await auditcustomerrepo.Update(auditcustomer);
                    break;
                case "CustomersForGroup":
                    var customergroup = ParseFromXml<Models.CustomersForGroup>(dataElement);
                    IRepo<Models.CustomersForGroup> customergrouprepo = new Repo<Models.CustomersForGroup>(App.Database);
                    await customergrouprepo.Update(customergroup);
                    break;
                case "GroupsForUser":
                    var groupsForUser = ParseFromXml<Models.GroupsForUser>(dataElement);
                    IRepo<Models.GroupsForUser> groupsForUserrepo = new Repo<Models.GroupsForUser>(App.Database);
                    await groupsForUserrepo.Update(groupsForUser);
                    break;
                case "Audits":
                    var audits = ParseFromXml<Models.Audits>(dataElement);
                    IRepo<Models.Audits> auditsrepor = new Repo<Models.Audits>(App.Database);
                    await auditsrepor.Update(audits);
                    break;
                case "WorkOrderDefinitions":
                    var workOrderDefinitions = ParseFromXml<Models.WorkOrderDefinitions>(dataElement);
                    IRepo<Models.WorkOrderDefinitions> workOrderDefinitionsRepo = new Repo<Models.WorkOrderDefinitions>(App.Database);
                    await workOrderDefinitionsRepo.Update(workOrderDefinitions);
                    break;
                case "WorkOrders":
                    var workOrders = ParseFromXml<Models.WorkOrders>(dataElement);
                    IRepo<Models.WorkOrders> workOrdersRepo = new Repo<Models.WorkOrders>(App.Database);
                    await workOrdersRepo.Update(workOrders);
                    break;
                case "WorkOrderAttachments":
                    var workOrderAttachments = ParseFromXml<Models.WorkOrderAttachments>(dataElement);
                    IRepo<Models.WorkOrderAttachments> workOrderAttachmentsRepo = new Repo<Models.WorkOrderAttachments>(App.Database);
                    await workOrderAttachmentsRepo.Update(workOrderAttachments);
                    break;
                case "GenericAttachments":
                    var genericAttachments = ParseFromXml<Models.GenericAttachments>(dataElement);
                    IRepo<Models.GenericAttachments> genericAttachmentsRepo = new Repo<Models.GenericAttachments>(App.Database);
                    await genericAttachmentsRepo.Update(genericAttachments);
                    break;
                case "AuditsForLocation":
                    var auditsForLocation = ParseFromXml<Models.AuditsForLocation>(dataElement);
                    IRepo<Models.AuditsForLocation> auditsForLocationRepo = new Repo<Models.AuditsForLocation>(App.Database);
                    await auditsForLocationRepo.Update(auditsForLocation);
                    break;
                case "DataOrganisations":
                    var dataOrganisations = ParseFromXml<Models.DataOrganisations>(dataElement);
                    IRepo<Models.DataOrganisations> dataOrganisationsRepo = new Repo<Models.DataOrganisations>(App.Database);
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
                    IRepo<Models.Customers> repo = new Repo<Models.Customers>(App.Database);
                    lastId = await repo.Insert(customers);
                    break;
                case "Locations":
                    var location = ParseFromXml<Models.Locations>(dataElement);
                    IRepo<Models.Locations> locrepo = new Repo<Models.Locations>(App.Database);
                    lastId = await locrepo.Insert(location);
                    break;
                case "Contacts":
                    var contact = ParseFromXml<Models.Contacts>(dataElement);
                    IRepo<Models.Contacts> conrepo = new Repo<Models.Contacts>(App.Database);
                    lastId = await conrepo.Insert(contact);
                    break;
                case "Users":
                    var user = ParseFromXml<Models.Users>(dataElement);
                    IRepo<Models.Users> userrepo = new Repo<Models.Users>(App.Database);
                    lastId = await userrepo.Insert(user);
                    break;
                case "GenericListDefinitions":
                    var genericListDefinitions = ParseFromXml<Models.GenericListDefinitions>(dataElement);
                    IRepo<Models.GenericListDefinitions> genlistrepo = new Repo<Models.GenericListDefinitions>(App.Database);
                    lastId = await genlistrepo.Insert(genericListDefinitions);
                    break;
                case "GenericLists":
                    var gen = ParseFromXml<Models.GenericLists>(dataElement);
                    IRepo<Models.GenericLists> genrepo = new Repo<Models.GenericLists>(App.Database);
                    lastId = await genrepo.Insert(gen);
                    break;
                case "AuditsForCustomer":
                    var auditcustomer = ParseFromXml<Models.AuditsForCustomer>(dataElement);
                    IRepo<Models.AuditsForCustomer> auditcustomerrepo = new Repo<Models.AuditsForCustomer>(App.Database);
                    lastId = await auditcustomerrepo.Insert(auditcustomer);
                    break;
                case "CustomersForGroup":
                    var customergroup = ParseFromXml<Models.CustomersForGroup>(dataElement);
                    IRepo<Models.CustomersForGroup> customergrouprepo = new Repo<Models.CustomersForGroup>(App.Database);
                    lastId = await customergrouprepo.Insert(customergroup);
                    break;
                case "GroupsForUser":
                    var groupsForUser = ParseFromXml<Models.GroupsForUser>(dataElement);
                    IRepo<Models.GroupsForUser> groupsForUserrepo = new Repo<Models.GroupsForUser>(App.Database);
                    lastId = await groupsForUserrepo.Insert(groupsForUser);
                    break;
                case "Audits":
                    var audits = ParseFromXml<Models.Audits>(dataElement);
                    IRepo<Models.Audits> auditsrepor = new Repo<Models.Audits>(App.Database);
                    lastId = await auditsrepor.Insert(audits);
                    break;
                case "WorkOrderDefinitions":
                    var workOrderDefinitions = ParseFromXml<Models.WorkOrderDefinitions>(dataElement);
                    IRepo<Models.WorkOrderDefinitions> workOrderDefinitionsRepo = new Repo<Models.WorkOrderDefinitions>(App.Database);
                    lastId = await workOrderDefinitionsRepo.Insert(workOrderDefinitions);
                    break;
                case "WorkOrders":
                    var workOrders = ParseFromXml<Models.WorkOrders>(dataElement);
                    IRepo<Models.WorkOrders> workOrdersRepo = new Repo<Models.WorkOrders>(App.Database);
                    lastId = await workOrdersRepo.Insert(workOrders);
                    break;
                case "WorkOrderAttachments":
                    var workOrderAttachments = ParseFromXml<Models.WorkOrderAttachments>(dataElement);
                    IRepo<Models.WorkOrderAttachments> workOrderAttachmentsRepo = new Repo<Models.WorkOrderAttachments>(App.Database);
                    lastId = await workOrderAttachmentsRepo.Insert(workOrderAttachments);
                    break;
                case "GenericAttachments":
                    var genericAttachments = ParseFromXml<Models.GenericAttachments>(dataElement);
                    genericAttachments.NeedToDownload = 1;
                    IRepo<Models.GenericAttachments> genericAttachmentsRepo = new Repo<Models.GenericAttachments>(App.Database);
                    lastId = await genericAttachmentsRepo.Insert(genericAttachments);
                    break;
                case "AuditsForLocation":
                    var auditsForLocation = ParseFromXml<Models.AuditsForLocation>(dataElement);
                    IRepo<Models.AuditsForLocation> auditsForLocationRepo = new Repo<Models.AuditsForLocation>(App.Database);
                    lastId = await auditsForLocationRepo.Insert(auditsForLocation);
                    break;
                case "DataOrganisations":
                    var dataOrganisations = ParseFromXml<Models.DataOrganisations>(dataElement);
                    IRepo<Models.DataOrganisations> dataOrganisationsRepo = new Repo<Models.DataOrganisations>(App.Database);
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

            IRepo<Models.DataTransferParameters> dataTransferParametersRepo = new Repo<Models.DataTransferParameters>(App.Database);
            dataTransferParametersRepo.Insert(dataTransferParameters);

        }
    }
}
