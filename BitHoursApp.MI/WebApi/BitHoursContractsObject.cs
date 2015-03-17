using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.MI.WebApi
{
    //success => true, message => text, data => client_name,jobs_title, hours_per_week, contract_id
    public class BitHoursContractsObject
    {
        public string client_name { get; set; }
        public string jobs_title { get; set; }        
        public int hours_per_week { get; set; }
        public int contract_id { get; set; }
    }
}
