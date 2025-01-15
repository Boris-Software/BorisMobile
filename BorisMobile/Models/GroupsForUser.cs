using SQLite;

namespace BorisMobile.Models
{
    public class GroupsForUser
    {
        [PrimaryKey]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
    }
}
