﻿using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using System.Threading.Tasks;
using System.Security.Principal;
using Microsoft.Azure.Mobile.Server.Authentication;
using System.Linq;
using Newtonsoft.Json.Linq;
using ContosoMoments.MobileServer.Models;
using System;
using ContosoMoments.Common.Queue;
using ContosoMoments.Common;
using ContosoMoments.MobileServer.DataLogic;

namespace ContosoMoments.MobileServer.Controllers.WebAPI
{
    [MobileAppController]
    public class ManageUserController : ApiController
    {
        // GET api/ManageUser
        public async Task<string> Get()
        {
            var fedLogic = new FederationLogic();
            Web.Models.ConfigModel config = new Web.Models.ConfigModel();
            string retVal = config.DefaultUserId;

            // Get the credentials for the logged-in user.
            var fbCredentials = await this.User.GetAppServiceIdentityAsync<FacebookCredentials>(this.Request);
    
            if (null != fbCredentials && fbCredentials.Claims.Count > 0)
            {
                retVal = await fedLogic.GetFacebookUserInfo(fbCredentials);
                return retVal;
            }

            var aadCredentials = await this.User.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(this.Request);
            if (null != aadCredentials && aadCredentials.Claims.Count > 0)
            {
                string email = aadCredentials.UserId;

                retVal = CheckAddEmailToDB(email);
            }

            return retVal;
        }

        private static string CheckAddEmailToDB(string email)
        {
            string retVal;
            var ctx = new MobileServiceContext();
            var user = ctx.Users.Where(x => x.Email == email);

            if (user.Count() == 0)
            {
                var u = ctx.Users.Add(new Common.Models.User() { Id = Guid.NewGuid().ToString(), Email = email, IsEnabled = true });
                try
                {
                    ctx.SaveChanges();
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    //
                }

                retVal = u.Id;
            }
            else
            {
                var u = user.First();
                retVal = u.Id;
            }

            return retVal;
        }
    }
}