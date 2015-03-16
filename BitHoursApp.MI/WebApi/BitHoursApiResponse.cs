using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.MI.WebApi
{
    /// <summary>
    /// Generic response object
    /// </summary>
    /// <typeparam name="T">Response object type</typeparam>
    public class BitHoursApiResponse<T>
      where T : class
    {
        public BitHoursApiResponse()
        {
            ErrorCode = BitHoursApiErrors.None;
        }

        public bool HasError
        {
            get
            {
                return ErrorCode != BitHoursApiErrors.None;
            }
        }

        public BitHoursApiErrors ErrorCode { get; set; }

        public string Error { get; set; }

        public BitHoursApiJsonResponse<T> Result { get; set; }
    }

    public class BitHoursApiJsonResponse<T>
    {
        public bool success { get; set; }

        public string message { get; set; }

        public T data { get; set; }
    }
}