using BorisMobile.DataHandler;
using BorisMobile.DataHandler.Data;
using BorisMobile.Models;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace BorisMobile.Services
{
    public class WorkFromAssetsService
    {
        WorkFromAssetsDataHandler dataHandler;
        public WorkFromAssetsService()
        {
            dataHandler = new WorkFromAssetsDataHandler();
        }

        public async Task<List<WorkFromAssets>> GetData(WorkOrderList SelectedItem,int drawingListId)
        {
            var list = await dataHandler.GetAssetData(drawingListId);
            foreach (var item in list) 
                {
                    // Load XML
                    XDocument doc = XDocument.Parse(item.genericList.XmlDoc);

                    // Extract required attributes
                    XElement listItem = doc.Root;

                    string lDlocation = listItem?.Attribute("lDlocation")?.Value ?? "N/A";
                    string penS = listItem?.Attribute("penS")?.Value ?? "N/A";
                    string totalCostBreakdownS = listItem?.Attribute("totalCostBreakdownS")?.Value ?? "N/A";

                    // Remove values after ':'
                    if (totalCostBreakdownS.Contains(":"))
                    {
                        totalCostBreakdownS = totalCostBreakdownS.Split(':')[0].Trim();
                    }

                    item.Pens = penS;
                    item.Location = lDlocation;
                    item.InstallType = totalCostBreakdownS;
                }
            ////_lDlocation
            //_penS
            //_totalCostBreakdownS
            return list;
        }


        
    }
}
