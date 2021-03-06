﻿using System;
using System.Globalization;
using System.Windows.Data;
using BitHoursApp.Common.Resources;

namespace BitHoursApp.Controls.Wpf
{
    public class TimeSpanToLongStringConverter : IValueConverter
    {
        public const string HoursAabbreviationResourceName = "Common_HoursAbbreviation";
        public const string MinutesAbbreviationResourceName = "Common_MinutesAbbreviation";
        public const string SecondsAbbreviationResourceName = "Common_SecondsAbbreviation";

        private const string hmsFormat = "{0:%h} {1} {0:%m} {2} {0:%s} {3}";
        private const string msFormat = "{0:%m} {2} {0:%s} {3}";
        private const string sFormat = "{0:%s} {3}";

        private IResourceManager resourceManager = CommonResourceManager.Instance;
        public IResourceManager ResourceManager
        {
            get { return resourceManager; }
            set { resourceManager = value; }
        }

        private static string hoursAbbreviation;
        private string HoursAbbreviation
        {
            get { return hoursAbbreviation ?? (hoursAbbreviation = ResourceManager.GetResourceString(HoursAabbreviationResourceName)); }
        }

        private static string minutesAbbreviation;
        private string MinutesAbbreviation
        {
            get { return minutesAbbreviation ?? (minutesAbbreviation = ResourceManager.GetResourceString(MinutesAbbreviationResourceName)); }
        }

        private static string secondsAbbreviation;
        private string SecondsAbbreviation
        {
            get { return secondsAbbreviation ?? (secondsAbbreviation = ResourceManager.GetResourceString(SecondsAbbreviationResourceName)); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan timeSpan = (TimeSpan)value;

            string format;
            if (timeSpan.TotalMinutes < 1)
                format = sFormat;
            else if (timeSpan.TotalHours < 1)
                format = msFormat;
            else
                format = hmsFormat;

            return string.Format(format, timeSpan, HoursAbbreviation, MinutesAbbreviation, SecondsAbbreviation);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
