using BorisMobile.Helper;
using BorisMobile.Repository;

namespace BorisMobile.Services
{
    public class AboutService
    {
        public async Task<Models.DataOrganisations> GetCompanyDetails()
        {
            IRepo<Models.DataOrganisations> genericAttachmentRepo = new Repo<Models.DataOrganisations>(DBHelper.Database);
            List<Models.DataOrganisations> list = await genericAttachmentRepo.Get();

            return list.Count > 0 ? list[0] : new Models.DataOrganisations();
        }
    }
}
