using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BitHoursApp.Common.Wpf.Converters
{   
    [ValueConversion(typeof(TimeSpan), typeof(string))]
    public class TimeSpanToStringConverter : MarkupExtensionConverterBase
    {        
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan? timeSpan = value as TimeSpan?;

            if (timeSpan == null)
                return String.Empty;

            if(timeSpan == TimeSpan.Zero)
                return "--:--:--";

            return timeSpan.ToString();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}