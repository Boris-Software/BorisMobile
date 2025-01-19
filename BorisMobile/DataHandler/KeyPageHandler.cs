using BorisMobile.DataHandler.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorisMobile.DataHandler
{
    public class KeyPageHandler : CommonDataHandler
    {

        public async Task<string> GetAttributeForWODefDoc(int workOrderDefinitionId, string attName)
        {
            return await GetAttributeFromDataSql("SELECT XmlDoc FROM WorkOrderDefinitions WHERE Id = " + workOrderDefinitionId, attName);
        }

        public IdAndDescriptionCollection GetListEntriesList(int iconListId)
        {
            return GetListEntriesForList(iconListId,null,null);
        }
    }
}
