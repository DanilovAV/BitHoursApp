using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.MI.WebApi
{
    public class BitHoursUploadRequest
    {
        public int UserId { get; set; }
        public int ContractId { get; set; }
        public Image Snapshot { get; set; }
        public int ElapsedMinutes { get; set; }
        public string Memo { get; set; }
        public BitHoursUploadRequestStatus Status { get; set; }
    }

    public enum BitHoursUploadRequestStatus
    {
        Start,
        Stop
    }

    //public class BitHoursUploadRequest
    //{
    //    public int contractor_id { get; set; }
    //    public int contract_id { get; set; }
    //    public byte[] screenshot { get; set; }
    //    public int minutes { get; set; }
    //    public string memo { get; set; }
    //    public string status { get; set; }
    //}
}
