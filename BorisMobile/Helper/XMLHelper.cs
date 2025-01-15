using BorisMobile.Repository;
using BorisMobile.XML;
using System.Collections;
using System.Xml;

namespace BorisMobile.Helper
{
    public class XMLHelper
    {
        public static ArrayList URLList(XmlConfigDoc config)
        {
            ArrayList urls = new ArrayList();
            for (int i = 0; i < 5; i++)   //  !!!!!!!! 5 hard-coded !
            {
                string oneUrl = config.GetAtt("//IPAddresses/BaseURL[" + (i + 1) + "]/@url");
                if (oneUrl != "")
                {
                    urls.Add(oneUrl);
                }
            }
            return urls;
        }

        public static async Task<XmlElement> GetSignInPayload()
        {
            IRepo<Models.Settings> settingsRepo = new Repo<Models.Settings>(App.Database);
            List<Models.Settings> settingsList = await settingsRepo.Get();

            var m_configXml = new XmlConfigDoc(Helper.Constants.APPLICATION_CONFIG_FILE);
            XmlElement payload = m_configXml.XmlDocument.CreateElement("Payload");
            XmlElement dmi = m_configXml.XmlDocument.CreateElement("Device");
            dmi.SetAttribute("appName", Helper.Constants.APP_NAME);
            dmi.SetAttribute("appVer", AssemblyHelper.ApplicationVersion + "." + AssemblyHelper.ApplicationPlatform);
            dmi.SetAttribute("dbVer", settingsList.Where(x => x.KeyName == Helper.Constants.DB_VERSION_SETTING_NAME).FirstOrDefault().IntValue.ToString());
            payload.AppendChild(dmi);

            return payload;
        }
    }
}
