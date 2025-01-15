using BorisMobile.Utilities;
using System.Xml;

namespace BorisMobile.XML
{
    public class XmlConfigAttachmentDoc
    {
        private XmlDocument m_xmlDoc = null;
        private XmlElement m_headerSection;

        private const string HEADER_SECTION_NAME = "Header";

        public XmlConfigAttachmentDoc(string configFilename)
        {
            string configDirectory = Misc.GetImagesDirectoryMAUI();
            m_xmlDoc = new XmlDocument();
#if DEBUG
            if (configFilename.ToLower().IndexOf("main") != -1)
            {
                configFilename = configFilename.ToLower().Replace("main", "");
            }
#endif
            m_xmlDoc.Load(Path.Combine(configDirectory, configFilename.ToLower()));
            Init();
        }

        public XmlConfigAttachmentDoc(XmlDocument config)
        {
            m_xmlDoc = config;
            Init();
        }

        private void Init()
        {
            m_headerSection = (XmlElement)m_xmlDoc.SelectSingleNode("//Header"); // (XmlElement)m_xmlDoc.GetElementsByTagName("Header").Item(0);
        }

        public string GetHeaderString(string xPath)
        {
            if (m_headerSection == null)
            {
                return "";
            }
            XmlNode headerNode = m_headerSection.SelectSingleNode(xPath);
            if (headerNode == null)
            {
                return "";
            }
            string text = headerNode.InnerText;
            if (text == "")
            {
                text = ((XmlElement)headerNode).GetAttribute("desc", "");
            }
            return text;
        }

        public string GetLocalisableHeaderString(string xPath, string overrideLocale)
        {
            if (m_headerSection == null)
            {
                return "";
            }
            XmlElement headerNode = m_headerSection.SelectSingleNode(xPath) as XmlElement;
            if (headerNode == null)
            {
                return "";
            }
            // text__locale attribute trumps the inner text
            string text = XmlUtils.LocalisedStringAtt(headerNode, "text", headerNode.InnerText, overrideLocale);
            if (string.IsNullOrEmpty(text))
            {
                text = XmlUtils.LocalisedStringAtt(headerNode, "desc", "", overrideLocale);
            }
            return text;
        }

        //public string GetHeaderAtt(string xPath)
        //{
        //    if (m_headerSection == null)
        //    {
        //        return "";
        //    }
        //    return m_headerSection.SelectSingleNode(xPath).InnerXml;
        //}

        public string GetAtt(string xPath)
        {
            XmlNode node = m_xmlDoc.SelectSingleNode(xPath);
            if (node != null)
            {
                return node.InnerXml;
            }
            return "";
        }

        public XmlNodeList GetNodeList(string xPath)
        {
            return m_xmlDoc.SelectNodes(xPath);
        }

        public XmlElement GetElement(string tagName)
        {
            //return (XmlElement) m_xmlDoc.GetElementsByTagName(tagName).Item(0);
            return (XmlElement)m_xmlDoc.SelectSingleNode(tagName);
        }


        public XmlElement CreateElement(string tagName)
        {
            return m_xmlDoc.CreateElement(tagName);
        }

        public XmlDocument XmlDocument
        {
            get
            {
                return m_xmlDoc;
            }
        }
    
}
}
