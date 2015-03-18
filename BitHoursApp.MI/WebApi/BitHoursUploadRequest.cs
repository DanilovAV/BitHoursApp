using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitHoursApp.MI.Models;

namespace BitHoursApp.MI.WebApi
{
    public class BitHoursUploadRequest
    {
        public UserInfo UserInfo { get; set; }
        public int ContractId { get; set; }
        public Image Snapshot { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Memo { get; set; }
     
    }   
}