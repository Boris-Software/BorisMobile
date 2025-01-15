using SQLite;

namespace BorisMobile.Models
{
    public class AuditTrailDetails
    {
        [PrimaryKey]
        public int Id { get; set; }
        public Guid IdGuid { get; set; }
        public string XmlResults { get; set; }
        public DateTime Modified { get; set; }
        public int AuditTrailReason { get; set; }

    }
}
