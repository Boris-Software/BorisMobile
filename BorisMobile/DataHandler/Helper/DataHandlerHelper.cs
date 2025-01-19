using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorisMobile.DataHandler.Helper
{
    public class DataHandlerHelper
    {
        public static int IntFromString(string textResult, int defaultValue)
        {
            int intValue;
            if (Int32.TryParse(textResult, out intValue))
            {
                return intValue;
            }
            return defaultValue;
        }
    }
}
