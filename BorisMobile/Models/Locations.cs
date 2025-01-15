using SQLite;

namespace BorisMobile.Models
{
    public class Locations
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Desc { get; set; }
        public int CustomerId { get; set; }
        public int FilterListEntryId { get; set; }
        public string XmlDoc { get; set; }
    }
}
