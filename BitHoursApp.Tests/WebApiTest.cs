using System;
using System.Threading.Tasks;
using BitHoursApp.MI.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitHoursApp.Tests
{
    [TestClass]
    public class WebApiTest
    {
        [TestMethod]
        public async Task TestLogin()
        {
            var email = "aleksander.v.danilov@gmail.com";
            var password = "ortionst";

            IBitHoursApi api = BitHoursApi.Instance;
            var result = await api.LoginAsync(email, password);

            Assert.IsNotNull(result);
        }
    }
}
