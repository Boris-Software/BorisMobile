using SQLite;

namespace BorisMobile.Models
{
    public class WorkOrderDefinitions
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string XmlDoc { get; set; }
        public string Desc { get; set; }
    }
}
