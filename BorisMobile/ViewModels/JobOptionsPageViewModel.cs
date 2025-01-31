using BorisMobile.Models;
using BorisMobile.Services.Interfaces;
using BorisMobile.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorisMobile.ViewModels
{
    public class JobOptionsPageViewModel :BaseViewModel
    {
        public async void HandleJob(string xmlElement, WorkOrderList workOrder)
        {
            //if audit progress has something for this day then open job details page.
            // else open jobformpage - dynamic form page

            //DependencyService.Get<IXmlParserService>(), DependencyService.Get<IFormGenerationService>()
            //if (true)
            //{
                 await App.Current.MainPage.Navigation.PushAsync(new JobFormPage(xmlElement,workOrder));
            //}
            //else
            //{
            //    await App.Current.MainPage.Navigation.PushAsync(new JobDetailsPage(new JobDetailsPageViewModel( item)));
            //}
        }
    }
}
