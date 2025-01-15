
namespace BorisMobile.DataTransferController
{
    public enum WebRequestResult : int
    {
        AWAITING_RESPONSE,
        REQUEST_ERROR,
        RESPONSE_ERROR,
        HAVE_DATA,
        USER_ABORTED,
        AUTH_FAILED,
        TIMEOUT,
        HAVE_WEB_CHECK_DATA
    }
}
