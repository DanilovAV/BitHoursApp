using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitHoursApp.Common;
using BitHoursApp.MI.WebApi;

namespace BitHoursApp.MI.Models
{
    public class UserInfo
    {
        private readonly BitHoursLoginObject loginObject;
        private readonly string sessionId;

        public UserInfo(BitHoursLoginObject loginObject, string sessionId)
        {
            Check.Require(loginObject != null);
            this.loginObject = loginObject;
            this.sessionId = sessionId;
        }

        public int UserId
        {
            get
            {
                return loginObject.contractor_id;                    
            }
        }

        public string UserName
        {
            get
            {
                return loginObject.name;
            }
        }

        public string SessionId
        {
            get
            {
                return sessionId;
            }
        }
    }
}