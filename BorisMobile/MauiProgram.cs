using BorisMobile.Controls;
using BorisMobile.ViewModels;
using BorisMobile.Views;
using Microsoft.Extensions.Logging;
using BorisMobile.Services.Interfaces;
using BorisMobile.Services;
using Syncfusion.Maui.Toolkit.Hosting;

#if ANDROID
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#endif

using Microsoft.Extensions.Logging;

namespace BorisMobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("fa-brands-400.ttf", "FaBrands");
                    fonts.AddFont("fa-regular-400.ttf", "FaRegular");
                    fonts.AddFont("fa-solid-900.ttf", "FaSolid");

                    //fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    //fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemiBold");
                });
            builder.RemoveBorders();
            builder.Services.AddTransient<CustomerCodePage>();
            builder.Services.AddTransient<SigninPage>();
            builder.Services.AddTransient<CustomerCodePageViewModel>();
            builder.Services.AddTransient<SigninPageViewModel>();
            builder.Services.AddTransient<SyncPage>();
            builder.Services.AddTransient<SyncPageViewModel>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<HomePageViewModel>();
            builder.Services.AddTransient<AboutPage>();
            builder.Services.AddTransient<AboutPageViewModel>();
            builder.Services.AddTransient<MyDiaryPage>();
            builder.Services.AddTransient<MyDiaryPageViewModel>();
            builder.Services.AddTransient<ScheduledWorkPage>();
            builder.Services.AddTransient<ScheduledWorkViewModel>();
            builder.Services.AddTransient<JobDetailsPage>();
            builder.Services.AddTransient<JobDetailsPageViewModel>();
            builder.Services.AddTransient<WorkFromDrawingPage>();
            builder.Services.AddTransient<WorkFromDrawingPageViewModel>();
            builder.Services.AddTransient<CreateNewFormPage>();
            builder.Services.AddTransient<CreateNewFormPageViewModel>();
            builder.Services.AddTransient<DocumentsPage>();
            builder.Services.AddTransient<DocumentsPageViewModel>();
            builder.Services.AddTransient<InProgressPage>();
            builder.Services.AddTransient<InProgressPageViewModel>();
            builder.Services.AddTransient<WorkFromAssetsPage>();
            builder.Services.AddTransient<WorkFromAssetsPageViewModel>();
            builder.Services.AddTransient<KeyPage>();
            builder.Services.AddTransient<KeyPageViewModel>();
            builder.Services.AddTransient<AssetsPage>();
            builder.Services.AddTransient<AssetsPageViewModel>();
            builder.Services.AddTransient<UserSupportPage>();
            builder.Services.AddTransient<UserSupportPageViewModel>();
            builder.Services.AddTransient<ScanNFCPage>();
            builder.Services.AddTransient<ScanNFCPageViewModel>();
            builder.Services.AddTransient<BulletinsPage>();
            builder.Services.AddTransient<BulletinsPageViewModel>();
            builder.Services.AddTransient<SupportPage>();
            builder.Services.AddTransient<SupportPageViewModel>();
            builder.Services.AddTransient<DocumentStorePage>();
            builder.Services.AddTransient<DocumentStorePageViewModel>();


            builder.Services.AddSingleton<IXmlParserService, XmlParserService>();
            builder.Services.AddSingleton<IFormGenerationService, FormGenerationService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
        public static MauiAppBuilder RemoveBorders(this MauiAppBuilder mauiAppBuilder)
        {
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
            {
                if (view is NoBorderEntry)
                {
#if ANDROID
                handler.PlatformView.Background = null;
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
#elif IOS
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                handler.PlatformView.Layer.BorderWidth = 0;
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
                }
            });

            Microsoft.Maui.Handlers.SearchBarHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
            {
                if (view is NoBorderSearchBar)
                {
#if ANDROID
                //handler.PlatformView.Background = null;
                //handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                //handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
                Android.Widget.LinearLayout linearLayout =  handler.PlatformView.GetChildAt(0) as Android.Widget.LinearLayout;  
               linearLayout = linearLayout.GetChildAt(2) as Android.Widget.LinearLayout;  
               linearLayout = linearLayout.GetChildAt(1) as Android.Widget.LinearLayout;  
               linearLayout.Background = null; 
#elif IOS
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                handler.PlatformView.Layer.BorderWidth = 0;
               // handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
                }
            });

            Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("MyCustomization", (handler, view) =>
            {

                if (view is NoBorderDatePicker)
                {

#if ANDROID
                    handler.PlatformView.BackgroundTintList =
        Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
                }

            });

            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
            {
                if (view is NoBorderPicker)
                {
#if ANDROID
               handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif IOS
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                handler.PlatformView.Layer.BorderWidth = 0;
               // handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
                }
            });
            return mauiAppBuilder;
        }
    }
}
