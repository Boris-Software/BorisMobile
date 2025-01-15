using SQLite;

namespace BorisMobile.Models
{
    public class ReleasedAudits
    {
        [PrimaryKey]
        public Guid IdGuid { get; set; }
        public string XmlResults { get; set; }
        public int Status { get; set; }
        public DateTime DateTimeReleased { get; set; }
        public int KeepWorkOrderOpen { get; set; }

    }
}
