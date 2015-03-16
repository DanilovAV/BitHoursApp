﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.MI.WebApi
{
    public static class BitHoursUrlHelper
    {
        public const string BaseServiceUrl = "http://bithours.com/development/dev/webservice/";

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
    }
}