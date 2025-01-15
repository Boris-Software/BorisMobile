using SQLite;

namespace BorisMobile.Models
{
    public class Translations
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string Locale { get; set; }
        public string Desc { get; set; }

    }
}
