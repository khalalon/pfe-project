using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PFE_Dahboard.Models
{
    public class EmailModel
    {
        public int EmailID { get; set; }
        public int OpCode { get; set; }
        public int CategoryID { get; set; }
        public int SenderProfileID { get; set; }
        public int Status { get; set; }
        public int Error { get; set; }
        public int CtcID { get; set; }
        public DateTime SendDate { get; set; }
        public int NbClicks { get; set; }
        public int NbViews { get; set; }
        public int Unsubscribe { get; set; }
        public float CostPerItem { get; set; }
    }

}