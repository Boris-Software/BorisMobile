using SQLite;

namespace BorisMobile.Models
{
    public class SupportingWork
    {
        [PrimaryKey]
        public Guid IdGuid { get; set; }
        public int UserId { get; set; }
        public int CustomerId { get; set; }
        public int AuditId { get; set; }
        public string XmlResults { get; set; }
        public int StatusId { get; set; }
        public int LocationId { get; set; }
        public DateTime DateOfAudit { get; set; }
        public DateTime DateTimeStarted { get; set; }
        public int WorkOrderId { get; set; }
        public Guid CustomerLocalGuid { get; set; }
        public Guid LocationLocalGuid { get; set; }
        public int IsFromServer { get; set; }
        public string CachedXmlResults { get; set; }
        public int CachedSubDoc { get; set; }

    }
}
