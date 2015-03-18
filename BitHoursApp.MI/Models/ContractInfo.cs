using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.MI.Models
{
    public class ContractInfo
    {
        public string ContractTitle { get; set; }

        public string ContractOwner { get; set; }

        public int ContractId { get; set; }

        public TimeSpan ElapsedTime { get; set; }

        public int HoursPerWeek { get; set; }

        public string Memo { get; set; }
    }
}
