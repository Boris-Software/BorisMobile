using SQLite;

namespace BorisMobile.Models
{
    public class DataOrganisations
    {

        [PrimaryKey]
        public int Id { get; set; }
        public string Desc { get; set; }
        public int Edition { get; set; }
        public string Add1 { get; set; }
        public string Add2 { get; set; }
        public string Add3 { get; set; }
        public string Add4 { get; set; }
        public string Web { get; set; }
        public string DefaultFromEmailAddress { get; set; }
        public string DefaultToEmailAddress { get; set; }
        public int OwningUserId { get; set; }
        public int DefaultLocationId { get; set; }
        public int DocumentLibraryListId { get; set; }
        public int Credits { get; set; }
    }
}
