using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PFE_Dahboard.Models
{
    public class VisitModel
    {
        public int VisitsFactID { get; set; }
        public int CtcID { get; set; }
        public DateTime VisitDateTime { get; set; }
        public int SiteID { get; set; }
        public string SiteName { get; set; }
        public double DirectSpent { get; set; }
        public double IndirectSpent { get; set; }
        public string Source { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}