using BorisMobile.DataHandler;
using BorisMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BorisMobile.Services
{
    public class DocumentsService
    {
        DocumentsHandler documentsHandler;
        WorkOrderList workOrder;
        public DocumentsService(WorkOrderList workOrder) {
            this.workOrder = workOrder;
            documentsHandler = new DocumentsHandler();
        }

        public async Task<List<Attachments>> GetDocumentsList()
        {
            List<Attachments> documentsList = new List<Attachments>();

            var luList = await GetDocumentListForEntityType("LO");
            var cuList = await GetDocumentListForEntityType("CU");

            documentsList.AddRange(luList);
            documentsList.AddRange(cuList);

            return documentsList;
        }

        public async Task<List<Attachments>> GetDocumentListForEntityType(String entityType)
        {
            List<Attachments> attachmentsForEntityType = new List<Attachments>();
            int workOrderId = workOrder.workOrder.Id;
            if (workOrderId > 0)
            {
                if (entityType == "CU")
                {
                    foreach (var item in documentsHandler.GetGenericAttachmentsForEntity(entityType, workOrder.workOrder.CustomerId))
                    {
                        attachmentsForEntityType.Add(item);
                    }
                }else if(entityType == "LO")
                {
                    foreach (var item in documentsHandler.GetGenericAttachmentsForEntity(entityType, workOrder.workOrder.LocationId))
                    {
                        attachmentsForEntityType.Add(item);
                    }
                }
            }
            return attachmentsForEntityType;
        }

        //public void GetDocumentListForCU(int entityNumber)
        //{
        //    Dictionary<Attachments, bool> attachmentsForEntityType = new Dictionary<Attachments, bool>();
        //    int workOrderId = workOrder.workOrder.Id;
        //    if (workOrderId > 0)
        //    {
        //        foreach (var item in documentsHandler.GetGenericAttachmentsForEntity("CU", workOrder.workOrder.LocationId))
        //        {
        //            attachmentsForEntityType.Add(item, false);
        //        }
        //    }
        //}

    }

    
}
