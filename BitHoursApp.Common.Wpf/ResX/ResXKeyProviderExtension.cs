using System;
using System.Collections.Generic;
using System.Windows.Markup;
using BitHoursApp.Common.Resources;

namespace BitHoursApp.Common.Wpf.ResX
{  
    public class ResXKeyProviderExtension<T> : MarkupExtension, IResXKeyProvider where T : class, IResXKeyProvider, new()
    {
        protected internal readonly T ResXKeyProvider = new T();

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public string ProvideKey(IEnumerable<object> parameters)
        {
            return ResXKeyProvider.ProvideKey(parameters);
        }
    }
}