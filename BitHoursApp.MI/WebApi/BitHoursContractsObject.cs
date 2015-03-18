using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitHoursApp.MI.Models;

namespace BitHoursApp.MI.WebApi
{    
    public class BitHoursContractsObject
    {
        public string client_name { get; set; }
        public string job_title { get; set; }        
        public int hours_per_week { get; set; }
        public int contract_id { get; set; }        
    }

    public static class BitHoursContractsObjectExtension
    {
        public static ContractInfo ToContractInfo(this BitHoursContractsObject contractObject)
        {
            return new ContractInfo
            {
                ContractId = contractObject.contract_id,
                ContractOwner = contractObject.client_name,
                ContractTitle = contractObject.job_title,
                HoursPerWeek = contractObject.hours_per_week
            };
        }
    }
}
