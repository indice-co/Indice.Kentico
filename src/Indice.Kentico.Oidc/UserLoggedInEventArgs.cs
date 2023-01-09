using CMS.Membership;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Indice.Kentico.Oidc
{
    /// <summary>
    /// Fired every time a user is succesfully logged in.
    /// </summary>
    public class UserLoggedInEventArgs : EventArgs
    {
        /// <summary>
        /// The kentico user reference
        /// </summary>
        public UserInfo User { get; internal set; }
        /// <summary>
        /// The incoming Principal claim values
        /// </summary>
        public IEnumerable<Claim> Claims { get; set; }
    }
}
