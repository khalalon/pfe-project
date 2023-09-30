using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PFE_Dahboard.Models
{
    public class EmailsTimeModel
    {
        public string TimeOfDay { get; set; }
        public string EmailCount { get; set; }
    }
}
