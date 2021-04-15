using CMS.Membership;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Indice.Kentico.Oidc
{
    public class UserLoggedInEventArgs : EventArgs
    {
        public UserInfo User { get; internal set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}
