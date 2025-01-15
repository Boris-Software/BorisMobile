using BorisMobile.DataTransferController;
using BorisMobile.Helper;
using BorisMobile.XML;

namespace BorisMobile.Services
{
    public class SigninService
    {
        public async Task<string> Signin(string username, string Password)
        {
            WebApiServices webApiService = new WebApiServices();
            var m_configXml = new XmlConfigDoc(Helper.Constants.APPLICATION_CONFIG_FILE);
            var urlList = XMLHelper.URLList(m_configXml);
            var respaylod  = await XMLHelper.GetSignInPayload();
            var response = await webApiService.SendRequest(username + "\r\n" + Password + "\r\n" + respaylod.OuterXml, urlList[0]+"/MD_" + Helper.Constants.SIGN_IN_FILENAME + "", "PUT");

            if (response != null && !response.Equals("Exception"))
            {
                return response;// WebResponseState.Success;
            }
            return WebResponseState.Failed.ToString();
        }
    }
}
