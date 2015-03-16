using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.MI.WebApi
{       
    /// <summary>
    /// Response object from login service
    /// </summary>
    public class BitHoursLoginObject
    {
        public int contractor_id { get; set; }
        public string name { get; set; }
        public string profile_img { get; set; }
    }
}