using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace BA.Framework.IMLib
{
    public class UserIdentity : IIdentity
    {
        public string Token { get; set; }

        public string UserAgent { get; set; }

        public string UserType { get; set; }

        public string AuthenticationType { get; set; }

        public bool IsAuthenticated { get; set; }

        public string Name { get; set; }

    }
}
