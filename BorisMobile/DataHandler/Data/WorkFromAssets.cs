using BorisMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorisMobile.DataHandler.Data
{
    public class WorkFromAssets
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public GenericLists genericList { get; set; }
        public string Location { get; set; }
        public string Pens { get; set; }
        public string InstallType { get; set; }
    }
}
