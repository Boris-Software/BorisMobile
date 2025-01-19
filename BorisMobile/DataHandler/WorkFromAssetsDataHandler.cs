using BorisMobile.DataHandler.Data;
using BorisMobile.DataHandler.Helper;
using BorisMobile.Models;
using System.Xml;

namespace BorisMobile.DataHandler
{
    public class WorkFromAssetsDataHandler : CommonDataHandler
    {
        //public WorkFromAssetsDataHandler()
        //{

        //}
        //public async void GetInitData(WorkOrderList workOrder)
        //{
        //    try
        //    {
        //        ListEntry woPageOptions = GetListEntry(Convert.ToInt32(workOrder.workOrder.DefinitionId));
        //        XmlDocument additionalSettings = AdditionalSettingsHelper.AdditionalSettings(woPageOptions);

        //        //m_assetConfigId = listItem.Id; // save so we can load the config
        //        bool allowLocalDelete = XML.XmlUtils.BoolAtt(additionalSettings.DocumentElement, "AllowLocalAssetDelete", false);
        //        bool suppressTotal = XML.XmlUtils.BoolAtt(additionalSettings.DocumentElement, "SuppressTotal", false);
        //        string filterCondition = XML.XmlUtils.StringAtt(additionalSettings.DocumentElement, "FilterCondition");
        //        // First check for simple case, there's just a list of assets. This is not the default, we default to a list of lists, via the drawings
        //        int simpleAssetListToUse = AssetListToUse(workOrder, additionalSettings);
        //        if (simpleAssetListToUse > 0)
        //        {
        //            SetupAssetList(section, simpleAssetListToUse, allowLocalDelete, suppressTotal, filterCondition);
        //        }
        //        else
        //        {
        //            int listOfAssetsLists = ListOfAssetListsToUse(workOrder, additionalSettings); //
        //            XmlElement drawingChoice = CreateElement("Combo");
        //            drawingChoice.SetAttribute("text", "");
        //            drawingChoice.SetAttribute("mandatory", "yes");
        //            drawingChoice.SetAttribute("listid", listOfAssetsLists.ToString());
        //            drawingChoice.SetAttribute("uniquename", "drawing");
        //            drawingChoice.SetAttribute("allowlocaldelete", allowLocalDelete.ToString()); // 011123 This is nothing to do with my changes but looks like it should be there
        //            drawingChoice.SetAttribute("filterConditionForAsset", filterCondition); // 011123 Just hold condition here for when a drawing is selected and we need to show the assets
        //            drawingChoice.SetAttribute("assetListIdAttributeName", XML.XmlUtils.StringAtt(additionalSettings.DocumentElement, "AssetsListIdAttributeName", "drawingAssetsListId"));
        //            ComboNugget regNugget = (ComboNugget)section.AddNugget(drawingChoice);
        //            regNugget.GetComboBoxEx().SetAdditionalHandler(this.DrawingSelectedForAsset);
        //            int drawingLocation = ConfiguredApplication.theApp.DataAccess.ReadIntSetting(ConfiguredApplication.theApp.CURRENT_WODS_ASSET_DRAWING);
        //            regNugget.SetResult(drawingLocation.ToString());
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }
        //}
        //private int ListOfAssetListsToUse(WorkOrderList workOrder, XmlDocument additionalSettings)
        //{
        //    string entity = XML.XmlUtils.StringAtt(additionalSettings.DocumentElement, "ListOfAssetsListsEntity", "LO");
        //    string attribute = XML.XmlUtils.StringAtt(additionalSettings.DocumentElement, "ListOfAssetsListsAttribute", "drawingListId");
        //    return GeneralEntityList(workOrder, entity, attribute);
        //}
        //private void SetupAssetList(Section section, int assetListId, bool allowLocalDelete, bool hideTotal, string filterCondition)
        //{
        //    RecordFilterFunction filterFunction = null;
        //    if (!string.IsNullOrEmpty(filterCondition))
        //    {
        //        filterFunction = new SimpleConditionFilter(filterCondition).SimpleConditionFunction;
        //    }
        //    Collection<object> fullAndLocalItems = ConfiguredApplication.Application.DataAccess.GetListEntriesAndLocalListEntriesForList(assetListId, filterFunction);
        //    XmlElement switcherItem = GetXmlCharsForAssetList();
        //    string configXml = ConfiguredApplication.Application.DataAccess.GetAttributeForListEntry(m_assetConfigId, "xmlConfig");
        //    WorkOrder workOrder = ConfiguredApplication.Application.DataAccess.WorkOrder(m_workOrderId);
        //    ConfigurableOwnerListNugget nugget = new ConfigurableOwnerListNugget(section, switcherItem, fullAndLocalItems, AssetSelected, configXml, true, IconListForWorkOrder(workOrder), allowLocalDelete, hideTotal);
        //    section.Children.Add(nugget);
        //}

        //private int AssetListToUse(WorkOrderList workOrder, XmlDocument additionalSettings)
        //{
        //    string entity = XML.XmlUtils.StringAtt(additionalSettings.DocumentElement, "AssetListEntity", "");
        //    string attribute = XML.XmlUtils.StringAtt(additionalSettings.DocumentElement, "AssetListAttribute", "");
        //    return GeneralEntityList(workOrder, entity, attribute);
        //}

        //private int GeneralEntityList(WorkOrderList workOrder, string entity, string attribute)
        //{
        //    // Attribute could be hard-coded
        //    if (string.IsNullOrEmpty(attribute)) // check against errors in set up
        //    {
        //        return -1;
        //    }
        //    int attInt = UIUtils.IntFromString(attribute, -1);
        //    if (attInt > 0)
        //    {
        //        return attInt;
        //    }
        //    if (entity == "LO")
        //    {
        //        return ConfiguredApplication.Application.DataAccess.GetAttributeForLocation_int(GetLocationId(), attribute, -1);
        //    }
        //    else if (entity == "CU")
        //    {
        //        ConfiguredApplication.Application.DataAccess.GetAttributeForCustomer_int(GetCustomerId(), attribute, -1);
        //    }
        //    else if (entity == "US")
        //    {
        //        ConfiguredApplication.Application.DataAccess.GetAttributeForUser_int(ConfiguredApplication.Application.User.UserId, attribute, -1);
        //    }
        //    return -1;
        //}
    }
    }
