using SqlCeCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SqlCeDataReader = Microsoft.Data.Sqlite.SqliteDataReader;

namespace BorisMobile.DataHandler.Data
{
    public class DataEnum
    {
        public delegate SqlCeCommand GetCommandObjectFunction();
        public delegate SqlCeCommand SearchCommandCallback();
        public delegate bool RecordFilterFunction(SqlCeDataReader reader);
        public enum UploadStatusEnum : int
        {
            PENDING = 1,
            UPLOADED_RESULT_ATTACHMENTS_WAITING = 2,
            UPLOADED_ALL = 3
        }

        public enum ResultStatusEnum : int
        {
            NOT_STARTED = 1,
            IN_PROGRESS = 2,
            COMPLETE = 3,
            RELEASED_FOR_USER_EDIT = 4,
            RELEASED_FOR_SIGN_OFF = 5,
            SIGNED_OFF = 6,
            DEVICE_CANCELLED = 7,
            FORWARDED = 8,
            SNAPSHOT = 9,
            RECEIVED_AS_FORWARD_AWAITING_INITIALISATION = 10
        }

        public enum LocalEntityStatusEnum : int
        {
            DEFAULT = 0,
            PENDING = 1,
            DELETED = 2
        }
        public enum AttachmentStatusEnum : int
        {
            PENDING = 1,
            SAVED = 2,
            UPLOADED = 3
        }
    }
}
