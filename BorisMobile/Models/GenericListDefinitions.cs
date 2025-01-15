using SQLite;

namespace BorisMobile.Models
{
    public class GenericListDefinitions
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Desc { get; set; }
        public string XmlDoc { get; set; }
        public int JobSpecific { get; set; }
    }
}
