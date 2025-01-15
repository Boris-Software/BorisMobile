using BorisMobile.Utilities;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace BorisMobile.XML
{
    public class XmlUtils
    {
        public enum JobAllocationTypeEnum : int
        {
            BOTH = 0,
            USER_ONLY = 1,
            GROUP_ONLY = 2
        }
        public class JobAllocationTypeHelper
        {
            public static JobAllocationTypeEnum AllocationTypeFromConfig(XmlElement config)
            {
                JobAllocationTypeEnum jobAllocationType = JobAllocationTypeEnum.BOTH;
                string allocationType = XmlUtils.GetChildTextFromElement(config, "AllocationType");
                if (allocationType == "User")
                {
                    jobAllocationType = JobAllocationTypeEnum.USER_ONLY;
                }
                else if (allocationType == "Group")
                {
                    jobAllocationType = JobAllocationTypeEnum.GROUP_ONLY;
                }
                return jobAllocationType;
            }
        }

        public static bool VerifyName(string xmlName)
        {
            try
            {
                XmlConvert.VerifyName(xmlName); // Rubbish, why doesn't this return a bool instead of throwing an exception?
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static string GetChildTextFromElement(XmlElement xml, string tagName)
        {
            XmlNode node = xml.SelectSingleNode(tagName);
            if (node != null)
            {
                return node.InnerText;
            }
            return "";
        }

        public static string GetChildTextFromNode(XmlNode xml, string tagName)
        {
            XmlNode node = xml.SelectSingleNode(tagName);
            if (node != null)
            {
                return node.InnerText;
            }
            return "";
        }

        public static string GetGlobalAttribute(XmlDocument xmlDoc, string attName)
        {
            XmlNode item = xmlDoc.SelectSingleNode(string.Format("//@{0}", attName));
            if (item != null && !string.IsNullOrEmpty(item.Value))
            {
                return item.Value;
            }
            return "";
        }

        public static void CopyNonEmptyAttribute(XmlElement targetElement, string attributeName, XmlDocument sourceDocument)
        {
            string sourceValue = GetGlobalAttribute(sourceDocument, attributeName);
            if (!string.IsNullOrEmpty(sourceValue))
            {
                targetElement.SetAttribute(attributeName, sourceValue);
            }
        }

        public static XmlElement PrependChildElement(XmlDocument xmlDoc, XmlElement parentElement, XmlElement elementToAdd)
        {
            return CopyElementBetweenDocuments(parentElement, elementToAdd, false);
        }

        public static XmlElement CopyElementBetweenDocuments(XmlElement targetParent, XmlElement sourceElement)
        {
            return CopyElementBetweenDocuments(targetParent, sourceElement, true);
        }

        public static XmlElement CopyElementBetweenDocuments(XmlElement targetParent, XmlElement sourceElement, bool appendOrPrepend)
        {
            XmlElement newElement = targetParent.OwnerDocument.CreateElement(sourceElement.Name);
            newElement.InnerXml = sourceElement.InnerXml;
            foreach (XmlAttribute att in sourceElement.SelectNodes("@*"))
            {
                newElement.SetAttribute(att.Name, att.Value);
            }
            if (appendOrPrepend)
            {
                targetParent.AppendChild(newElement);
            }
            else
            {
                targetParent.PrependChild(newElement);
            }
            return newElement;
        }

        public static XmlElement CopyElementButNoChildrenBetweenDocuments(XmlElement targetParent, XmlElement sourceElement)
        {
            XmlElement newElement = targetParent.OwnerDocument.CreateElement(sourceElement.Name);
            foreach (XmlAttribute att in sourceElement.SelectNodes("@*"))
            {
                newElement.SetAttribute(att.Name, att.Value);
            }
            targetParent.AppendChild(newElement);
            return newElement;
        }

        public static XmlElement XmlElementFromXPathNavigator(XPathNavigator nav)
        {
            if (nav != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(nav.OuterXml);
                return doc.DocumentElement;
            }
            return null;
        }

        public static Hashtable GetAllAttributes(XmlDocument xmlDoc, string description)
        {
            Hashtable htAtts = new Hashtable();
            if (xmlDoc != null)
            {
                XmlNodeList atts = xmlDoc.SelectNodes("//@*");
                foreach (XmlNode oneAtt in atts)
                {
                    if (!htAtts.ContainsKey(oneAtt.Name))
                    {
                        htAtts.Add(oneAtt.Name, oneAtt.Value);
                    }
                }
            }
            if (htAtts["desc"] == null) // add in description for ComboBoxExPanels which are part of ExternalWorkOrderNugget (otherwise we can't display the actual extension!)
            {
                htAtts["desc"] = description;
            }
            return htAtts;
        }

        public static bool BoolAtt(XmlElement xml, string attName, bool defaultValue)
        {
            if (xml != null)
            {
                string stringVal = xml.GetAttribute(attName).ToLower();
                if (stringVal == "true" || stringVal == "yes" || stringVal == "1")
                {
                    return true;
                }
                else if (stringVal == "false" || stringVal == "0")
                {
                    return false;
                }
            }
            return defaultValue;
        }

        public static decimal DecimalAtt(XmlElement xml, string attName, decimal defaultValue)
        {
            if (xml != null)
            {
                string stringVal = xml.GetAttribute(attName);
                // 130324
                //if (!string.IsNullOrEmpty(stringVal))
                //{
                //    return Decimal.Parse(stringVal);
                //}
                if (!string.IsNullOrEmpty(stringVal))
                {
                    decimal decValue = defaultValue;
                    if (Decimal.TryParse(stringVal, out decValue))
                    {
                        return decValue;
                    }
                }
            }
            return defaultValue;
        }

        public static int IntAtt(XmlElement xml, string attName, int defaultValue)
        {
            if (xml != null)
            {
                string stringVal = xml.GetAttribute(attName);
                if (!string.IsNullOrEmpty(stringVal))
                {
                    int intValue = defaultValue;
                    if (Int32.TryParse(stringVal, out intValue))
                    {
                        return intValue;
                    }
                }
            }
            return defaultValue;
        }

        public static Guid GuidAtt(XmlElement xml, string attName)
        {
            if (xml != null)
            {
                string stringVal = xml.GetAttribute(attName);
                if (!string.IsNullOrEmpty(stringVal))
                {
                    return new Guid(stringVal);
                }
            }
            return Guid.Empty;
        }

        public static Color ColourAtt(XmlElement xml, string attName, Color defaultValue)
        {
            if (xml != null)
            {
                string stringValue = xml.GetAttribute(attName);
                if (!string.IsNullOrEmpty(stringValue))
                {
                    //return Misc.GetColourFromColourName(stringValue);
                }
            }
            return defaultValue;
        }


        //public static System.Drawing.Pen PenAtt(XmlElement xml, Pen defaultPen)
        //{
        //    if (xml != null && (xml.HasAttribute("penColour") || xml.HasAttribute("penWidth")))
        //    {
        //        //int argb = IntAtt(xml, "penColour", defaultPen.Color.ToArgb());
        //        //float width = FloatAtt(xml, "penWidth", defaultPen.Width);
        //        //return new Pen(Color.FromArgb(argb), width);
        //    }
        //    return defaultPen;
        //}


        public static float FloatAtt(XmlElement xml, string attName, float defaultValue)
        {
            if (xml != null)
            {
                string stringVal = xml.GetAttribute(attName);
                if (stringVal != "")
                {
                    return (float)Decimal.Parse(stringVal);
                }
            }
            return defaultValue;
        }

        public static string StringAtt(XmlElement xml, string attName)
        {
            return StringAtt(xml, attName, null);
        }

        public static string StringAtt(XmlElement xml, string attName, string defaultValue)
        {
            if (xml != null)
            {
                string stringVal = xml.GetAttribute(attName);
                if (!string.IsNullOrEmpty(stringVal))
                {
                    return stringVal;
                }
            }
            return defaultValue;
        }

        public static string LocalisedStringAtt(XmlElement xml, string attName, string defaultValue, string overrideLocale)
        {
            string useAtt = !string.IsNullOrEmpty(overrideLocale) ? attName + "__" + overrideLocale : attName;
            return StringAtt(xml, useAtt, StringAtt(xml, attName, defaultValue));
        }

        public static TimeSpan TimeAtt(XmlElement xml, string attName, string defaultValue)
        {
            return Chrono.TimeSpanFromString(StringAtt(xml, attName, defaultValue));
        }

        public static XmlElement AddChildElement(XmlDocument xmlDoc, XmlElement parentElement, string newElementName, int intValue)
        {
            return AddChildElement(xmlDoc, parentElement, newElementName, intValue.ToString());
        }

        public static XmlElement AddChildElement(XmlDocument xmlDoc, XmlElement parentElement, string newElementName)
        {
            return AddChildElement(xmlDoc, parentElement, newElementName, null);
        }

        public static XmlElement AddChildElement(XmlDocument xmlDoc, XmlElement parentElement, string newElementName, string stringValue)
        {
            XmlElement newElement = xmlDoc.CreateElement(newElementName);
            if (!string.IsNullOrEmpty(stringValue))
            {
                newElement.InnerText = stringValue;
            }
            parentElement.AppendChild(newElement);
            return newElement;
        }

        public static XmlElement AddChildElement(XmlDocument xmlDoc, XmlElement parentElement, string newElementName, string stringValue, string namespaceURI)
        {
            XmlElement newElement = xmlDoc.CreateElement(newElementName, namespaceURI);
            newElement.InnerText = stringValue;
            parentElement.AppendChild(newElement);
            return newElement;
        }

        public static void RemoveChildren(XmlElement element, string xPathForRemoval)
        {
            Collection<XmlElement> deleteElements = new Collection<XmlElement>();
            foreach (XmlElement oneDel in element.SelectNodes(xPathForRemoval))
            {
                deleteElements.Add(oneDel);
            }
            foreach (XmlElement deleteElement in deleteElements)
            {
                deleteElement.ParentNode.RemoveChild(deleteElement);
            }
        }

        public static XmlElement ReplaceOrCreateChildElementByName(XmlElement parentElement, XmlElement newElementFromAnotherDocument)
        {
            XmlElement removeThis = null;
            //XmlElement newElementNotAppended = CopyElementBetweenDocuments(parentElement, newElementFromAnotherDocument, true); // append to end

            XmlElement newElement = parentElement.OwnerDocument.CreateElement(newElementFromAnotherDocument.Name);
            newElement.InnerXml = newElementFromAnotherDocument.InnerXml;
            foreach (XmlAttribute att in newElementFromAnotherDocument.SelectNodes("@*"))
            {
                newElement.SetAttribute(att.Name, att.Value);
            }

            XmlNodeList children = parentElement.SelectNodes("*");
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].Name == newElementFromAnotherDocument.Name)
                {
                    removeThis = (XmlElement)children[i];
                    XmlElement newOneInserted = parentElement.InsertAfter(newElement, removeThis) as XmlElement;
                    parentElement.RemoveChild(removeThis);
                    return newOneInserted;
                }
            }
            return parentElement.AppendChild(newElement) as XmlElement;
        }

        public static XmlElement SelectOrCreateChildElement(XmlDocument xmlDoc, XmlElement parentElement, string childElementName, string namespaceURI, string prefix, XmlNamespaceManager mgr)
        {
            string fullName = !string.IsNullOrEmpty(prefix) ? prefix + ":" : "";
            fullName += childElementName;
            XmlElement childElement = parentElement.SelectSingleNode(fullName, mgr) as XmlElement;
            if (childElement == null)
            {
                childElement = xmlDoc.CreateElement(childElementName, namespaceURI);
                parentElement.AppendChild(childElement);
            }
            return childElement;
        }

        public static XmlElement AddChildElementWithAttribute(XmlDocument xmlDoc, XmlElement parentElement, string newElementName, string attName, string stringValue)
        {
            XmlElement newElement = xmlDoc.CreateElement(newElementName);
            newElement.SetAttribute(attName, stringValue);
            parentElement.AppendChild(newElement);
            return newElement;
        }

        public static void AddAttribute(StringBuilder sb, XmlDocument xmlDoc, string attName, string formatSpecifier)
        {
            if (xmlDoc != null & !string.IsNullOrEmpty(attName))
            {
                XmlNode item = xmlDoc.SelectSingleNode(string.Format("//@{0}", attName));
                if (item != null && !string.IsNullOrEmpty(item.Value))
                {
                    if (!string.IsNullOrEmpty(formatSpecifier))
                    {
                        sb.Append(string.Format(formatSpecifier, item.Value));
                    }
                    else
                    {
                        sb.Append(item.Value);
                    }
                }
            }
        }

        public static void ReplaceAndArchiveAttribute(XmlElement xml, string attName, string newAttValue)
        {
            if (xml != null)
            {
                if (xml.HasAttribute(attName))
                {
                    xml.SetAttribute("old" + attName + "_" + Chrono.ConvertDateTimeToISOString(DateTime.Now).Replace(':', '_'), XmlUtils.StringAtt(xml, attName));
                }
                xml.SetAttribute(attName, newAttValue);
            }
        }

        public static Collection<XmlDocument> SplitXmlDocumentIntoChunks(XmlDocument sourceDoc, int elementsPerDocument)
        {
            Collection<XmlDocument> outputDocs = new Collection<XmlDocument>();
            int numElements = sourceDoc.DocumentElement.SelectNodes("*").Count;
            XmlDocument currentDoc = null;
            int count = 0;
            foreach (XmlElement oneSourceElement in sourceDoc.DocumentElement.SelectNodes("*"))
            {
                if (count % elementsPerDocument == 0)
                {
                    currentDoc = new XmlDocument();
                    currentDoc.LoadXml("<" + sourceDoc.DocumentElement.Name + "/>");
                    foreach (XmlAttribute att in sourceDoc.DocumentElement.Attributes)
                    {
                        currentDoc.DocumentElement.SetAttribute(att.Name, att.Value);
                    }
                    outputDocs.Add(currentDoc);
                }
                CopyElementBetweenDocuments(currentDoc.DocumentElement, oneSourceElement);
                count++;
            }
            return outputDocs;
        }
    }
}
