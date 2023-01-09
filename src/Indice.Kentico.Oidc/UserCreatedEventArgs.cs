using CMS.Membership;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Indice.Kentico.Oidc
{

    /// <summary>
    /// Fired once if user is created in Kentico.
    /// </summary>
    public class UserCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// The kentico user reference
        /// </summary>
        public UserInfo User { get; set; }
        /// <summary>
        /// The incoming Principal claim values
        /// </summary>
        public IEnumerable<Claim> Claims { get; set; }
    }
}
