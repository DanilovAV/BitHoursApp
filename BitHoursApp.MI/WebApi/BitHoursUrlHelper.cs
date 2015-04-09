using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.MI.WebApi
{
    public static class BitHoursUrlHelper
    {
        public const string BaseServiceUrl = "https://bithours.com/development/dev/webservice/";

        public const string LoginPoint = "login";
        public const string ContractsListPoint = "list-contracts";
        public const string TrackTimePoint = "track-time";        

        public static string GetUrl(string servicePoint)
        {
            return String.Format("{0}{1}", BaseServiceUrl, servicePoint);
        }

        public static string GetLoginUrl()
        {
            return GetUrl(LoginPoint);
        }

        public static string GetContractsListUrl()
        {
            return GetUrl(ContractsListPoint);
        }

        public static string GetTrackTimeUrl()
        {
            return GetUrl(TrackTimePoint);
        }
    }

    public static class BitHoursReqParams
    {
        public const string EmailParameter = "email";
        public const string PasswordParameter = "password";
        public const string ContractorId = "contractor_id";
        public const string ContractId = "contract_id";
        public const string Screenshot = "screenshot";
        public const string StartTime = "start_time";
        public const string EndTime = "end_time";
        public const string Memo = "memo";        

        public const string ScreenshotFormat = "{0}_{1}.{2}";
        public const string ScreenshotExtension = "jpg";
        public const string FormDataHeader = "form-data";
        public const string JpegContentType = "image/jpeg";
        public const string SessionParameter = "laravel_session";

        public const string RequestTimeFormat = "yyyy-MM-dd HH:mm:ss";        
    }
}