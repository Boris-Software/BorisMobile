using BorisMobile.DataHandler;
using BorisMobile.DataHandler.Data;
using BorisMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorisMobile.Services
{

    public class KeyPageService
    {
        KeyPageHandler keyPageHandler;
        WorkOrderList workOrder;
        public KeyPageService(WorkOrderList workOrder)
        {
            this.workOrder = workOrder;
            keyPageHandler = new KeyPageHandler();
        }

        public async Task<IdAndDescriptionCollection> GetKeysList()
        {
            IdAndDescriptionCollection iconListItems = await IconListForWorkOrder(workOrder.workOrder);
            return iconListItems;

        }

        public async Task<IdAndDescriptionCollection> IconListForWorkOrder(WorkOrders workOrder)
        {
            int iconListId = await GetAttributeForWODefDoc_int(Convert.ToInt32(workOrder.DefinitionId), "option_extender_iconbox_list", -1);
            return await keyPageHandler.GetListEntriesList(iconListId);
        }

        public async Task<int> GetAttributeForWODefDoc_int(int workOrderDefinitionId, string attName, int defaultValue)
        {
            string strVal = await keyPageHandler.GetAttributeForWODefDoc(workOrderDefinitionId, attName);
            if (!string.IsNullOrEmpty(strVal))
            {
                if (!string.IsNullOrEmpty(strVal))
                {
                    int intValue;
                    if (Int32.TryParse(strVal, out intValue))
                    {
                        return intValue;
                    }
                }
            }
            return defaultValue;
        }

        
    }
}
