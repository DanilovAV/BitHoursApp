using BitHoursApp.Common.Resources;

namespace BitHoursApp.Common.Wpf.ResX
{
    /// <summary>
    /// Формирует ключ по енуму следующим образом: "Enum_EnumeType_EnumValue"
    /// </summary>
    public class EnumResXKeyProviderExtension : ResXKeyProviderExtension<CompositeKeyProvider>
    {
        public EnumResXKeyProviderExtension()
        {
            ResXKeyProvider.BaseKey = "Enum";
        }
    }
}