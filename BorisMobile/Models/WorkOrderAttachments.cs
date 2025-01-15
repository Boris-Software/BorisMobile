using SQLite;

namespace BorisMobile.Models
{
    public class WorkOrderAttachments
    {
       
        [PrimaryKey]
        public Guid IdGuid { get; set; }
        public int WorkOrderId { get; set; }
        public int NeedToDownload { get; set; }
        public string ShortFileName { get; set; }
        public int CopiedToAttachments { get; set; }
        public int ServerId { get; set; }
    }
}
