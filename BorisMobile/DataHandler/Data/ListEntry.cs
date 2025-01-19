using BorisMobile.Utilities;
using System.Collections;
using System.Xml;

namespace BorisMobile.DataHandler.Data
{
    public class ListEntry
    {
        private string m_description;

        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        private Hashtable m_attributes;

        public Hashtable Attributes
        {
            get
            {
                return m_attributes;
            }
            set
            {
                m_attributes = value;
            }
        }

        private int m_listId;

        public int ListId
        {
            get { return m_listId; }
            set { m_listId = value; }
        }
        public XmlDocument XmlDocument { get { return m_xmlDocument; } set { m_xmlDocument = value; } }
        private XmlDocument m_xmlDocument;

        public ListEntry(string description)
        {
            m_description = description;
        }

        public int IntAtt(string attName, int defaultValue)
        {
            if (Attributes != null)
            {
                string stringVal = (string)Attributes[attName];
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

        public string StringAtt(string attName, string defaultValue)
        {
            if (Attributes != null)
            {
                string stringVal = (string)Attributes[attName];
                if (!string.IsNullOrEmpty(stringVal))
                {
                    return stringVal;
                }
            }
            return defaultValue;
        }

        public bool BoolAtt(string attName, bool defaultValue)
        {
            if (Attributes != null && Attributes[attName] != null)
            {
                string stringVal = ((string)Attributes[attName]).ToLower();
                if (stringVal == "true" || stringVal == "yes")
                {
                    return true;
                }
                else if (stringVal == "false")
                {
                    return false;
                }
            }
            return defaultValue;
        }

        public DateTime DateTimeAtt(string attName, DateTime defaultValue)
        {
            if (Attributes != null && Attributes[attName] != null)
            {
                string stringVal = (string)Attributes[attName];
                if (!string.IsNullOrEmpty(stringVal))
                {

                    return Misc.ConvertISOStringToDateTime(stringVal);

                }
            }
            return defaultValue;
        }
    }
}
