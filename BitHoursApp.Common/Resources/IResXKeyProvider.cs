using System.Collections.Generic;

namespace BitHoursApp.Common.Resources
{
    public interface IResXKeyProvider
    {
        string ProvideKey(IEnumerable<object> parameters);
    }
}
