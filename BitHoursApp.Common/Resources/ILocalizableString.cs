using System.Globalization;

namespace BitHoursApp.Common.Resources
{
    public interface ILocalizableString
    {
        string ToString(CultureInfo cultureInfo);
    }
}