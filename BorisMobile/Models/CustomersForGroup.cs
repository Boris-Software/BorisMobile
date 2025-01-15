using SQLite;

namespace BorisMobile.Models
{
    public class CustomersForGroup
    {
       [PrimaryKey]
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int CustomerId { get; set; }
    }
}
