using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BitHoursApp.Common.Resources;

namespace BitHoursApp.Common.Resources
{
    public class ResourceRegistrator
    {
        public static void Initialization()
        {
            RegisterResources(CommonResourceManager.Instance);
        }

        static int resourcesRegistered;
        static readonly FileResourceManager commonResourceManager = new FileResourceManager("Common_", "BitHoursApp.Common.Resources.Common", Assembly.GetExecutingAssembly());
        static readonly FileResourceManager errorsResourceManager = new FileResourceManager("Error_", "BitHoursApp.Common.Resources.Errors", Assembly.GetExecutingAssembly());
        static readonly FileResourceManager enumsResourceManager = new FileResourceManager("Enum_", "BitHoursApp.Common.Resources.Enums", Assembly.GetExecutingAssembly());

        public static void RegisterResources(CommonResourceManager resourceManager)
        {
            if (Interlocked.Exchange(ref resourcesRegistered, 1) == 1)
                return;

            resourceManager.RegisterResourceManager(commonResourceManager);
            resourceManager.RegisterResourceManager(errorsResourceManager);
            resourceManager.RegisterResourceManager(enumsResourceManager);
        }
    }
}