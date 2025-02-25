using BorisMobile.DataTransferController;
using BorisMobile.Helper;
using BorisMobile.Repository;
using BorisMobile.Utilities;
using BorisMobile.XML;
using System.Xml;

namespace BorisMobile.Services
{
    public class CustomerCodeService
    {
        DataTransferParameters dataTransferParameter = new DataTransferParameters();
        public async Task InitialiseDataParameters()
        {
            IRepo<Models.DataTransferParameters> dataTransferParameterRepo = new Repo<Models.DataTransferParameters>(DBHelper.Database);
            var res = await dataTransferParameterRepo.Get();
            var parameterObject = res.Where(X => X.EntityType == dataTransferParameter.EntityName).FirstOrDefault();
            if (parameterObject == null)
            {
                //fresh sync
                dataTransferParameter.LastTransactionDate = new DateTime();
                dataTransferParameter.LastId = -1;
                dataTransferParameter.LastGuid = Guid.Empty;
                dataTransferParameter.BaseTransactionDate = new DateTime();
                var insertObj = DataTransferParameterConversion(dataTransferParameter);
                var insertedId = await dataTransferParameterRepo.Insert(insertObj);
                dataTransferParameter.Id = insertedId;
            }
            else
            {
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
                var updateobj = DataTransferParameterConversion(dataTransferParameter);
                await dataTransferParameterRepo.Update(updateobj);
            }
        }
        public async Task SetNewDateAndId(string date, string lastProcessedId, bool getdata)
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
            var updateObject = DataTransferParameterConversion(dataTransferParameter);
            await dataTransferParameterRepo.Update(updateObject);
        }
        public async Task<WebResponseState> ProvisionCustomerCode(string customerCode,string provPassword)
        {
            WebResponseState responseProvisoinCustomer = WebResponseState.NotInitialized;
            WebApiServices webApiService = new WebApiServices();
            var m_configXml = new XmlConfigDoc(Helper.Constants.PROVISIONING_CONFIG_FILE);
            var urlList = XMLHelper.URLList(m_configXml);

            var response = await webApiService.SendRequest(customerCode + "\r\n" + provPassword + "\r\n", urlList[0]+"/W32_" + Helper.Constants.SIGN_IN_FILENAME + "", "PUT");

            if (response != null && !response.Equals("Exception"))
            {
                
                dataTransferParameter.EntityName = "GenericAttachments";
                bool getData = true;
                string maxDate = string.Empty;
                string lastIdProcessed = string.Empty;
                //await InitialiseDataParameters();
                while (getData == true)
                {
                    var filesResponse = await webApiService.GetFilesRequest(urlList[0] + Helper.Constants.TRANSACTIONS_FILENAME, response, dataTransferParameter);

                    if (filesResponse != null && !filesResponse.Equals("Exception"))
                    {
                        XmlDocument transactionDoc = new XmlDocument();
                        transactionDoc.LoadXml(filesResponse);

                        XmlElement headerElement = (XmlElement)transactionDoc.SelectSingleNode("Trans");
                        string minDate = headerElement.GetAttribute("minDate");
                        if (minDate != "")
                        {

                            //webApiService.ProcessTransactions(transactionDoc);
                            //await SetNewTransactionDate(minDate);
                            lastIdProcessed = await webApiService.ProcessTransactions(transactionDoc);
                            //string lastIdProcessed = oneEntity.ProcessTransactions(transactionDoc, SendMessageBackToParentUI);  // this also updates sync parameters table (last trans date etc)
                            string repeatFlag = headerElement.GetAttribute("repeat");
                            getData = (repeatFlag == "1");
                            maxDate = headerElement.GetAttribute("maxDate");
                            //await SetNewDateAndId(headerElement.GetAttribute("maxDate"), lastIdProcessed, getData == false);

                            responseProvisoinCustomer = WebResponseState.Success;
                        }
                        else
                        {
                            responseProvisoinCustomer = WebResponseState.Success;
                            getData = false;
                            //break;
                            //CreateProvisionFiles();
                            //return WebResponseState.Success;
                        }
                    }
                    else
                    {
                        getData = false; 
                        responseProvisoinCustomer = WebResponseState.Failed;
                        break;
                        //return WebResponseState.Failed;
                    }
                }

                if (responseProvisoinCustomer != WebResponseState.NotInitialized && responseProvisoinCustomer != WebResponseState.Failed)
                {
                    IRepo<Models.GenericAttachments> genericAttachmentRepo = new Repo<Models.GenericAttachments>(DBHelper.Database);
                    var genericAttachmentList = await genericAttachmentRepo.Get();
                    foreach (var attachment in genericAttachmentList)
                    {
                        await webApiService.RunDownload(attachment, response);
                    }
                    CreateProvisionFiles();
                    return WebResponseState.Success;
                }
                else
                {
                    return WebResponseState.Failed;
                }
            }
            else
            {
                return WebResponseState.Failed;
            }
        }

        private void CreateProvisionFiles()
        {
            //clear DB after provision
            IRepo<Models.GenericAttachments> genericAttachmentRepo = new Repo<Models.GenericAttachments>(DBHelper.Database);
            var recordsGenericAttachment = genericAttachmentRepo.DeleteAll();

            IRepo<Models.DataTransferParameters> dataTransferParametersRepo = new Repo<Models.DataTransferParameters>(DBHelper.Database);
            var recordsDataTransferParameters = dataTransferParametersRepo.DeleteAll();

            string provisionedMarkerFile = System.IO.Path.Combine(FilesHelper.GetCurrentDirectory(), "provisioned.dat");
            if (!File.Exists(provisionedMarkerFile))
                File.Create(provisionedMarkerFile);
            string authenticationMarkerFile = System.IO.Path.Combine(FilesHelper.GetCurrentDirectory(), "authenticated.dat");
            if (!File.Exists(authenticationMarkerFile))
                File.Create(authenticationMarkerFile);
        }

        public Models.DataTransferParameters DataTransferParameterConversion(DataTransferParameters controllerParam)
        {
            Models.DataTransferParameters param = new Models.DataTransferParameters();
            param.Id = controllerParam.Id;
            param.EntityType = controllerParam.EntityName;
            param.LastGuid = controllerParam.LastGuid;
            param.LastId = controllerParam.LastId;
            param.LastTransactionDate   = controllerParam.LastTransactionDate;
            param.BaseTransactionDate   = controllerParam.BaseTransactionDate;

            return param;
        }
    }
}
