using SQLite;

namespace BorisMobile.Models
{
    public class LocationsForUser
    {
        [PrimaryKey]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int LocationId { get; set; }
    }
}
