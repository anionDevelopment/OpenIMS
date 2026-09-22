using GRYLibrary.Core.APIServer.Services.Interfaces;
using System;

namespace OpenIMSBackend.Core.Model.BusinessTypes
{
    public class User : GRYLibrary.Core.APIServer.CommonDBTypes.User
    {
        internal static User Create(string username, string passwordHash, ITimeService timeService)
        {
            User user = CreateNewUser(new User(), username, passwordHash, timeService);
            user.EMailAddress = null;
            user.TOTP = new GRYLibrary.Core.APIServer.MFA.TOTP() { IsActicated = false, SecretKey = Guid.NewGuid().ToString("N") };
            return user;
        }
    }
}
