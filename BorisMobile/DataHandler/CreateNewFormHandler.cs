using BorisMobile.DataHandler.Data;
using BorisMobile.DataHandler.Helper;
using BorisMobile.Models;
using static BorisMobile.DataHandler.Data.DataEnum;
using SqlCeCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SqlCeDataReader = Microsoft.Data.Sqlite.SqliteDataReader;

namespace BorisMobile.DataHandler
{
    public class CreateNewFormHandler : CommonDataHandler
    {
        public CreateNewFormHandler() 
        {
        }
        
        public async Task<List<TemplateDocument>> GetData(WorkOrderList workOrder)
        {
            List<TemplateDocument> templates = new List<TemplateDocument>();
            string dontCreateDuplicates = await GetAttributeForWODefDoc(Convert.ToInt32(workOrder.workOrder.DefinitionId), "dontCreateDuplicateTemplates");

            for (int i = 0; i < 50; i++) // was 10
            {
                string template = await GetAttributeForWODefDoc(Convert.ToInt32(workOrder.workOrder.DefinitionId), "additionalTemplate_" + i);
                if (!string.IsNullOrEmpty(template))
                {
                    int templateId = DataHandlerHelper.IntFromString(template, -1);
                    if (templateId > 0)
                    {
                        if (false && !LocationHasTemplate(workOrder.workOrder.LocationId, workOrder.workOrder.CustomerId, templateId))
                        {
                            continue;
                        }
                        TemplateDocument templateDoc = await TemplateDocument.GetTemplate(templateId);

                        templates.Add(templateDoc);
                        //if (templateDoc != null)
                        //{
                        //    string templateName = templateDoc.TemplateName;
                        //    if (!string.IsNullOrEmpty(templateName))
                        //    {
                        //        XmlElement dummy = m_configXml.CreateElement("AddForm");
                        //        dummy.SetAttribute("buttonText", templateName);
                        //        dummy.SetAttribute("audit", template);
                        //        dummy.SetAttribute("dontCreateDuplicateTemplates", dontCreateDuplicates);

                        //    }
                        //}
                    }
                }
            }
            return templates;
        }

        public bool LocationHasTemplate(int locationId, int customerId, int templateId)
        {
            if (CustomerHasTemplate(customerId, templateId))
            {
                return true;
            }
            return GetIntFromDataSql("SELECT COUNT(*) FROM AuditsForLocation WHERE LocationId = " + locationId + " AND AuditId = " + templateId) > 0;
        }
        public bool CustomerHasTemplate(int customerId, int templateId)
        {
            return GetIntFromDataSql("SELECT COUNT(*) FROM AuditsForCustomer WHERE CustomerId = " + customerId + " AND AuditId = " + templateId) > 0;
        }

        public async Task<string> GetAttributeForWODefDoc(int workOrderDefinitionId, string attName)
        {
            return await GetAttributeFromDataSql("SELECT XmlDoc FROM WorkOrderDefinitions WHERE Id = " + workOrderDefinitionId, attName);
        }

        public async Task<AuditsInProgress> GetInsertCommandForNewAudit(WorkOrderList wot)
        {
            Guid newGuid = Guid.NewGuid();
            DateTime nowDateTime = DateTime.Now;
            SqlCeCommand command = GetCommandObject(
                        @"INSERT INTO AuditsInProgress (IdGuid, UserId, CustomerId, AuditId, DateOfAudit, StatusId, LocationId, DateTimeStarted, WorkOrderId, LastSaveTime)
                            VALUES (@IdGuid, @UserId, @CustomerId, @AuditId, @DateOfAudit, @StatusId, @LocationId, @DateTimeStarted, @WorkOrderId, @LastSaveTime)"); // XmlResults ignored - null at the moment
            AddGuidParam(command, "@IdGuid", newGuid);
            AddIntParam(command, "@UserId", wot.workOrder.UserId);
            AddIntParam(command, "@CustomerId", wot.workOrder.CustomerId);
            AddIntParam(command, "@AuditId", wot.audit.Id);
            AddDateTimeParam(command, "@DateOfAudit", nowDateTime);
            AddIntParam(command, "@StatusId", (int)ResultStatusEnum.IN_PROGRESS);
            AddIntParam(command, "@LocationId", wot.workOrder.LocationId);
            AddDateTimeParam(command, "@DateTimeStarted", DateTime.Now);
            if (wot.workOrder.Id != -1)
            {
                AddIntParam(command, "@WorkOrderId", wot.workOrder.Id);
            }
            else
            {
                AddIntParamNull(command, "@WorkOrderId");
            }
            AddDateTimeParam(command, "@LastSaveTime", DateTime.MinValue);

            command.Prepare();

            
            command.ExecuteNonQuery();
            
            AuditsInProgress inProgress = new AuditsInProgress();
            inProgress.IdGuid = newGuid;
            inProgress.DateTimeStarted= nowDateTime;
            inProgress.DateOfAudit= nowDateTime;
            inProgress.UserId = wot.workOrder.UserId;
            inProgress.CustomerId = wot.workOrder.CustomerId;
            inProgress.AuditId = wot.audit.Id;
            inProgress.StatusId = (int)ResultStatusEnum.IN_PROGRESS;
            inProgress.LocationId = wot.workOrder.LocationId;
            inProgress.WorkOrderId = wot.workOrder.Id;
            inProgress.LastSaveTime = DateTime.MinValue;
            //inProgress.XmlResults = DateTime.MinValue;
            return inProgress;
        }
    }
}
