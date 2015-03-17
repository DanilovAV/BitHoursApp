using BitHoursApp.Common.Resources;

namespace BitHoursApp.Common.Wpf.ResX
{
    /// <summary>
    /// ��������� ���� �� ����� ��������� �������: "Enum_EnumeType_EnumValue"
    /// </summary>
    public class EnumResXKeyProviderExtension : ResXKeyProviderExtension<CompositeKeyProvider>
    {
        public EnumResXKeyProviderExtension()
        {
            ResXKeyProvider.BaseKey = "Enum";
        }
    }
}