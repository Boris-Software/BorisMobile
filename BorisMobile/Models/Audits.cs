using SQLite;

namespace BorisMobile.Models
{
    public class Audits
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Desc { get; set; }
        public string XmlDoc { get; set; }
        public string Credits { get; set; }
    }
}
