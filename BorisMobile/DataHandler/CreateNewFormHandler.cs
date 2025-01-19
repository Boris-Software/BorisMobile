using BorisMobile.DataHandler.Data;
using BorisMobile.DataHandler.Helper;
using BorisMobile.Models;

namespace BorisMobile.DataHandler
{
    public class CreateNewFormHandler
    {
        CommonDataHandler commonDataHandler;
        public CreateNewFormHandler() 
        {
            commonDataHandler = new CommonDataHandler();
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
                        TemplateDocument templateDoc = TemplateDocument.GetTemplate(templateId);

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
            return commonDataHandler.GetIntFromDataSql("SELECT COUNT(*) FROM AuditsForLocation WHERE LocationId = " + locationId + " AND AuditId = " + templateId) > 0;
        }
        public bool CustomerHasTemplate(int customerId, int templateId)
        {
            return commonDataHandler.GetIntFromDataSql("SELECT COUNT(*) FROM AuditsForCustomer WHERE CustomerId = " + customerId + " AND AuditId = " + templateId) > 0;
        }

        public async Task<string> GetAttributeForWODefDoc(int workOrderDefinitionId, string attName)
        {
            return await commonDataHandler.GetAttributeFromDataSql("SELECT XmlDoc FROM WorkOrderDefinitions WHERE Id = " + workOrderDefinitionId, attName);
        }

    }
}
