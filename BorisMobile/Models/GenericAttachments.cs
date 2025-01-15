using SQLite;

namespace BorisMobile.Models
{
    public class GenericAttachments
    {
        [PrimaryKey]
        public Guid IdGuid { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public int NeedToDownload { get; set; }
        public string ShortFileName { get; set; }
        public int ServerId { get; set; }

    }
}
