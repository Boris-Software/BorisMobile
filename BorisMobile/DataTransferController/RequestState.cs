using System.Net;

namespace BorisMobile.DataTransferController
{
    public class RequestState
    {
        // ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.NETDEVFX.v20.en/cpref10/html/M_System_Net_HttpWebRequest_BeginGetResponse_1_2bb042b2.htm
        // This class stores the State of the request.
        public HttpWebRequest request; // need this for aborting things
        public Exception exception;
        public WebRequestResult result;


        public RequestState()
        {
        }

        public void ClearDown()
        {
            request = null;
        }
        public void Init(HttpWebRequest webRequest)
        {
            Init(webRequest, false);
        }

        public void Init(HttpWebRequest webRequest, bool ignoreExistingRequestBecauseTheresBeenAnErrorAndWeCantGetTheStreamAnyway)
        {
            if (request != null && request.Method != "GET") // downloading attachments doesn't have a stream that we want to close
            {
#if !PocketPC
                if (!ignoreExistingRequestBecauseTheresBeenAnErrorAndWeCantGetTheStreamAnyway)
                {
                    // 20140208 Added in try catch because all of a sudden the first transaction of an app's lifecycle bombs out
                    try
                    {
                        Stream stream = request.GetRequestStream();
                        if (stream != null)
                        {
                            stream.Close();
                            stream.Dispose();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
#endif
            }
            request = webRequest;
            //response = null;
            exception = null;
            result = WebRequestResult.AWAITING_RESPONSE;
        }
    }
}
