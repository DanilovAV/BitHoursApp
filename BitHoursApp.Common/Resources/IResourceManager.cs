using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace BitHoursApp.Common.Resources
{  
    public interface IResourceManager
    {       
        string GetResourceString(string resourceKey);

        string GetResourceString(string resourceKey, CultureInfo cultureInfo);
      
        object GetResourceObject(string resourceKey);

        object GetResourceObject(string resourceKey, CultureInfo cultureInfo);
     
        bool Match(string resourceKey);
    }
}