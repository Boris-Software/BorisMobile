using SQLite;

namespace BorisMobile.Models
{
    public class Customers
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Desc { get; set; }
        public string XmlDoc { get; set; }
    }
}
