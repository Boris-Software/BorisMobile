using BorisMobile.DataHandler.Data;
using System.Collections.Specialized;
using System.Xml;

namespace BorisMobile.DataHandler.Helper
{
    public class AdditionalSettingsHelper
    {
        public static XmlDocument AdditionalSettings(ListEntry listEntry)
        {
            string additionalSettingsText = listEntry != null ? listEntry.StringAtt("additionalSettings", "") : "";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<Settings/>");
            StringCollection additionalSettingsEntries = FromDelimetedString(additionalSettingsText, "\r", "", '\n', true);
            if (additionalSettingsEntries != null && additionalSettingsEntries.Count > 0)
            {
                foreach (string oneSettingsLine in additionalSettingsEntries)
                {
                    if (!string.IsNullOrEmpty(oneSettingsLine))
                    {
                        string[] bits = oneSettingsLine.Split('=');
                        if (bits.Length >= 1)
                        {
                            string attName = bits[0].Trim();
                            doc.DocumentElement.SetAttribute(attName, (bits.Length > 1) ? bits[1].Trim() : "");
                        }
                    }
                }
            }
            return doc;
        }

        public static StringCollection FromDelimetedString(string rawString, string stripChars, string replaceChars, char delimeter, bool ignoreBlank)
        {
            StringCollection sc = new StringCollection();
            if (!string.IsNullOrEmpty(rawString))
            {
                if (!string.IsNullOrEmpty(stripChars))
                {
                    rawString = rawString.Replace(stripChars, replaceChars);
                }
                string[] bits = rawString.Split(delimeter);
                foreach (string one in bits)
                {
                    if (!string.IsNullOrEmpty(one) || !ignoreBlank)
                    {
                        sc.Add(one);
                    }
                }
            }
            return sc;
        }
    }
}
