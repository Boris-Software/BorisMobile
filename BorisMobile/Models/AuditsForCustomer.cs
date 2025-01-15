using SQLite;

namespace BorisMobile.Models
{
    public class AuditsForCustomer
    {
        [PrimaryKey]
        public int Id { get; set; }
        public int CustId { get; set; }
        public int AuditId { get; set; }

    }
}
