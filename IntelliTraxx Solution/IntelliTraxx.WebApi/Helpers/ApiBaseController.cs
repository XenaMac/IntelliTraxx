﻿using System.Web.Http;
using IntelliTraxx.Shared.Contracts;

namespace IntelliTraxx.WebApi.Helpers
{
    public class ApiBaseController : ApiController
    {
        public readonly IUserSecurity UserSecurity;

        public ApiBaseController(IAppBaseContracts appBaseContracts)
        {          
            UserSecurity = appBaseContracts.GetUserSecurity();
        }     

    }
}
