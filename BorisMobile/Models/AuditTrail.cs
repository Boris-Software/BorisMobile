using SQLite;

namespace BorisMobile.Models
{
    public class AuditTrail
    {
        [PrimaryKey]
        public Guid IdGuid { get; set; }
        public int UserId { get; set; }
        public int CustomerId { get; set; }
        public int AuditId { get; set; }
        public string XmlResults { get; set; }
        public int LocationId { get; set; }
        public DateTime DateOfAudit { get; set; }
        public DateTime LastSaveTime { get; set; }
        public DateTime DateDeleted { get; set; }
        public int AuditTrailReason { get; set; }
    }
}
