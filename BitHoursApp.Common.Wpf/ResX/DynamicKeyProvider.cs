using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using BitHoursApp.Common.Resources;

namespace BitHoursApp.Common.Wpf.ResX
{
    /// <summary>
    /// KeyProvider - обходной манёвр для невозможности указать Key={Binding MyDynamicKey}
    /// </summary>
    public class DynamicKeyProvider : MarkupExtension, IResXKeyProvider
    {
        /// <summary>
        /// Возвращает ключ, переданный в качестве параметра - обходной манёвр для невозможности указать Key={Binding}
        /// </summary>
        /// <param name="parameters">Первым параметром (parameters[0]) ожидает строку, которую нужно вернуть.</param>
        /// <returns>Значение ключа для переданного параметра.</returns>
        public string ProvideKey(IEnumerable<object> parameters)
        {
            if (parameters != null)
            {
                var firstParam = parameters.FirstOrDefault();
                if (firstParam != null && firstParam != DependencyProperty.UnsetValue)
                    return firstParam.ToString();
            }
            return "Key_Unassigned";
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}