using System.Collections.ObjectModel;
using System.Xml;
using SqlCeCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SqlCeDataReader = Microsoft.Data.Sqlite.SqliteDataReader;

namespace BorisMobile.DataHandler.Data
{
    public class TemplateDocument : XmlDocument
    {
        private int m_templateId;
        public int TemplateId
        {
            get { return m_templateId; }
            set { m_templateId = value; }
        }

        public string GetDocLevelAttValue(string attName)
        {
            XmlAttribute att = (XmlAttribute)this.SelectSingleNode("/Config/Document/@" + attName);
            if (att == null)
            {
                return null;
            }
            return att.Value;
        }
#if ClientBuild
        public ImageResolution GetImageResolution()
        {
            return new ImageResolution(DocLevelAttId("imageResizePixels"), DocLevelAttId("imageResizeLongestDimension"), DocLevelAttId("imageResizeCompression"), DocLevelAttId("imageResizeSizeKB"), IsDocLevelBoolAttSet("imageResizeLongestDimensionStrict"), DocLevelAttId("imageResizeCompressionIncrement"), DocLevelAttId("imageResizeMaxCompression"), DocLevelAttId("postRotationJpgCompression"), IsDocLevelBoolAttSet("reorientateIfRequired"));
        }

        public ExternalInvokeParameters GetAnnotateParameters()
        {
            string appName = GetDocLevelAttValue("applicationName");
            string exeName = GetDocLevelAttValue("exeName");
            if (!string.IsNullOrEmpty(appName) && !string.IsNullOrEmpty(exeName))
            {
                return new ExternalInvokeParameters(appName, exeName);
            }
            return null;
        }
#endif
        public string TemplateName
        {
            get
            {
                return this.GetDocLevelAttValue("desc");
            }
        }

        public bool CanAnnotate
        {
            get
            {
                string appName = GetDocLevelAttValue("applicationName");
                if (!string.IsNullOrEmpty(appName))
                {
                    return !string.IsNullOrEmpty(GetDocLevelAttValue("exeName"));
                }
                return false;
            }
        }

        public bool AllowIncompleteSave()
        {
            string disallowIncompleteSave = GetDocLevelAttValue("disallowIncompleteSave");
            if (string.IsNullOrEmpty(disallowIncompleteSave)) // default to allowing
            {
                return true;
            }
            return (disallowIncompleteSave.ToLower() != "yes");
        }

        public bool AllowEdit()
        {
            string disallowMobileEdit = GetDocLevelAttValue("disallowMobileEdit");
            if (string.IsNullOrEmpty(disallowMobileEdit)) // default to allowing
            {
                return true;
            }
            return (disallowMobileEdit.ToLower() != "yes");
        }

        public bool AllowIncompleteSend()
        {
            string disallowIncompleteSend = GetDocLevelAttValue("disallowIncompleteSend");
            if (string.IsNullOrEmpty(disallowIncompleteSend)) // default to allowing
            {
                return true;
            }
            return (disallowIncompleteSend.ToLower() != "yes");
        }

        public bool DisallowRelease()
        {
            string neverReleaseForm = GetDocLevelAttValue("neverReleaseForm");
            if (!string.IsNullOrEmpty(neverReleaseForm) && neverReleaseForm.ToLower() == "yes")
            {
                return true;
            }
            return false;
        }

        public bool AutoInterimReport()
        {
            string sendInterimReportsOnSave = GetDocLevelAttValue("sendInterimReportsOnSave");
            if (!string.IsNullOrEmpty(sendInterimReportsOnSave) && sendInterimReportsOnSave.ToLower() == "yes")
            {
                return true;
            }
            return false;
        }

        public bool AllowWebEdit()
        {
            string disallowWebEdit = GetDocLevelAttValue("disallowWebEdit");
            if (string.IsNullOrEmpty(disallowWebEdit)) // default to allowing
            {
                return true;
            }
            return (disallowWebEdit.ToLower() != "yes");
        }

        public bool TreatAsReadOnly()
        {
            string treatAsReadOnly = GetDocLevelAttValue("treatAsReadOnly");
            if (string.IsNullOrEmpty(treatAsReadOnly))
            {
                return false;
            }
            return (treatAsReadOnly.ToLower() == "yes");
        }

        public bool AutoSendOnRelease()
        {
            string autoSend = GetDocLevelAttValue("autoSendOnRelease");
            if (string.IsNullOrEmpty(autoSend)) // default to allowing
            {
                return false;
            }
            return (autoSend.ToLower() == "yes");
        }

        //public static bool AutoSendOnRelease(int templateId)
        //{
        //    TemplateDocument doc = GetTemplate(templateId);
        //    return doc.AutoSendOnRelease();
        //}

        public bool SuppressAutoLoadAllPages()
        {
            string suppressAutoLoadAllPages = GetDocLevelAttValue("suppressAllPagesLoad");
            if (!string.IsNullOrEmpty(suppressAutoLoadAllPages) && suppressAutoLoadAllPages.ToLower() == "yes")
            {
                return true;
            }
            return false;
        }

        public bool AskClearFindings()
        {
            string askClearFindings = GetDocLevelAttValue("askClearFindings");
            if (!string.IsNullOrEmpty(askClearFindings) && askClearFindings.ToLower() == "yes")
            {
                return true;
            }
            return false;
        }

        public List<string> SignOffElementUniqueNames
        {
            get
            {
                List<string> uniqueNames = new List<string>();
                foreach (string soAtt in SIGNOFF_DOC_ATTRIBUTES)
                {
                    string attValue = GetDocLevelAttValue(soAtt);
                    if (!string.IsNullOrEmpty(attValue) && (attValue != "-1"))
                    {
                        uniqueNames.Add(attValue);
                    }
                }
                return uniqueNames;
            }
        }

        public static string[] SIGNOFF_DOC_ATTRIBUTES =
            {
                "soCustName",
                "soCustComment",
                "soCustSig",
                "soUserComment",
                "soUserSig"
            };

        public static TemplateDocument GetTrimmedAuditDefinitionFromId(int auditId)
        {
            //if (m_cachedAuditDefinitions == null || m_dataHandler.HaveDoneUpdate)
            //{
            //    m_cachedAuditDefinitions = new Dictionary<int, TemplateDocument>();
            //}
            //m_dataHandler.HaveDoneUpdate = false;

            //if (!m_cachedAuditDefinitions.ContainsKey(auditId))
            //{
                string baseResult = new CommonDataHandler().GetStringFromDataSql("SELECT XmlDoc FROM Audits WHERE Id = " + auditId); //Events 100 and 150
                if (baseResult == "")
                {
                    return null;
                }
                TemplateDocument auditDoc = new TemplateDocument();
                auditDoc.LoadXml(baseResult);
                for (int i = 0; i < auditDoc.SelectNodes(string.Format("//Event")).Count; i++)
                {
                    XmlElement element = auditDoc.SelectNodes(string.Format("//Event"))[i] as XmlElement;
                    if (element != null)
                    {
                        int eventId = XML.XmlUtils.IntAtt(element, "id", -1);
                        if (eventId < 100)
                        {
                            element.ParentNode.RemoveChild(element);
                            i--;
                        }
                    }
                }
                //m_cachedAuditDefinitions.Add(auditId, auditDoc);
                return auditDoc;
            //}
            //else
            //{
            //    return m_cachedAuditDefinitions[auditId];
            //}
        }
        public static TemplateDocument GetTemplate(int templateId)
        {
            TemplateDocument doc = GetTrimmedAuditDefinitionFromId(templateId);
            if (doc != null)
            {
                doc.TemplateId = templateId;
            }
            return doc;
        }

        public Collection<string> GetSequenceNumbers()
        {
            Collection<string> seqNumbers = null;
            XmlNodeList seqNodes = this.SelectNodes("//Score/@calculation");
            foreach (XmlNode seqNode in seqNodes)
            {
                XmlAttribute seqAsAtt = (XmlAttribute)seqNode;
                string calcValue = seqAsAtt.Value;
                string[] items = calcValue.Split('$');
                foreach (string item in items)
                {
                    if (item.StartsWith("seq:"))
                    {
                        string seqName = item.Split(':')[1];
                        if (!string.IsNullOrEmpty(seqName))
                        {
                            if (seqNumbers == null)
                            {
                                seqNumbers = new Collection<string>();
                            }
                            else if (seqNumbers.IndexOf(seqName) != -1)
                            {
                                continue;
                            }
                            seqNumbers.Add(seqName);
                        }
                    }
                }
            }
            return seqNumbers;
        }

        public bool AllowDeleteUnreleasable()
        {
            string allowDelete = GetDocLevelAttValue("allowDelete");
            if (string.IsNullOrEmpty(allowDelete))
            {
                return false;
            }
            return (allowDelete.ToLower() == "yes");
        }

        public bool AllowDeleteReleasable()
        {
            string allowDelete = GetDocLevelAttValue("allowDeleteReleasable");
            if (string.IsNullOrEmpty(allowDelete))
            {
                return false;
            }
            return (allowDelete.ToLower() == "yes");
        }

        public bool IsDocLevelBoolAttSet(string attName)
        {
            string attValue = GetDocLevelAttValue(attName);
            if (string.IsNullOrEmpty(attValue))
            {
                return false;
            }
            return (attValue.ToLower() == "yes");
        }

        public int DocLevelAttId(string attName)
        {
            string attValue = GetDocLevelAttValue(attName);
            if (!string.IsNullOrEmpty(attValue))
            {
                return Int32.Parse(attValue);
            }
            return -1;
        }

        public string GetItemType(string uniquename)
        {
            XmlElement item = this.SelectSingleNode("//*[@uniquename = '" + uniquename + "']") as XmlElement;
            if (item != null)
            {
                return item.Name;
            }
            return null;
        }

        public bool WantGPS()
        {
            return this.SelectNodes("//*[starts-with(name(), 'GPS')]").Count > 0 || this.SelectNodes("//*[@forceGPS = 'yes']").Count > 0;
        }
    }

    public class TemplateDocumentCollection : Collection<TemplateDocument>
    {
        public TemplateDocumentCollection(List<Guid> resultGuids)

        {
            List<int> templatesInvolved = new List<int>();
            foreach (Guid oneGuid in resultGuids)
            {

                int templateId = GetAuditIdFromAuditInProgressGuid(oneGuid);

                if (templatesInvolved.IndexOf(templateId) == -1)
                {
                    templatesInvolved.Add(templateId);
                }
            }

            foreach (int templateId in templatesInvolved)
            {

                this.Add(TemplateDocument.GetTemplate(templateId));

            }
        }
        public int GetAuditIdFromAuditInProgressGuid(Guid auditInProgressId)
        {
            CommonDataHandler c = new CommonDataHandler();
            SqlCeCommand command = c.GetCommandObject("SELECT AuditId FROM AuditsInProgress WHERE IdGuid = @idGuid");
            c.AddGuidParam(command, "idGuid", auditInProgressId);
            command.Prepare();
            object baseResult = command.ExecuteScalar();
            if (baseResult == null || string.IsNullOrEmpty(baseResult.ToString()))
            {
                return -1;
            }
            return Int32.Parse(baseResult.ToString());
        }
        public bool WantSignOffElement(string elementName)
        {
            foreach (TemplateDocument oneDoc in this.Items)
            {
                string attValue = oneDoc.GetDocLevelAttValue(elementName);
                if (!string.IsNullOrEmpty(attValue) && (attValue != "-1"))
                {
                    return true;
                }
            }
            return false;
        }

        public string SignOffText
        {
            get
            {
                foreach (TemplateDocument oneDoc in this.Items)
                {
                    string attValue = oneDoc.GetDocLevelAttValue("soText");
                    if (!string.IsNullOrEmpty(attValue))
                    {
                        return attValue;
                    }
                }
                return "";
            }
        }
    }
}
