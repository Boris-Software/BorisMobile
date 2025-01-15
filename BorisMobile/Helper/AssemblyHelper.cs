using System.Reflection;

namespace BorisMobile.Helper
{
    public class AssemblyHelper
    {
        public static string ApplicationVersion
        {
            get
            {
                Assembly ass = Assembly.GetExecutingAssembly();
                if (ass != null)
                {
                    AssemblyName assname = ass.GetName();
                    if (assname != null)
                    {
                        Version v = assname.Version;
                        if (v != null)
                        {
                            return v.ToString();
                        }
                    }
                }
                return "";
            }
        }

        public static string ApplicationPlatform
        {
            get
            {
#if ANDROID
                return "a";
#elif iOS
                return "i";
#else
                return "w";
#endif
            }
        }
    }
}
