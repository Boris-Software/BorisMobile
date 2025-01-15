using SQLite;

namespace BorisMobile.Models
{
    public class Extensions
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Description { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public DateTime ExtensionDate { get; set; }

    }
}
