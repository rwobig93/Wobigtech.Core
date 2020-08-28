using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Users;

namespace Wobigtech.Core.Tools
{
    public static class Profile
    {
        private static readonly Volo.Abp.Users.CurrentUser currentUser;

        public static CurrentUser GetCurrentUser()
        {
            return currentUser;
        }
    }
}
