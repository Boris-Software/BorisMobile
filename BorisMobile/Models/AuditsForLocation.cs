using SQLite;

namespace BorisMobile.Models
{
    public class AuditsForLocation
    {
        
        [PrimaryKey]
        public int Id { get; set; }
        public int LocationId { get; set; }
        public int AuditId { get; set; }

    }
}
