using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using OpFlow.Service.Models;

namespace OpFlow.Service.App_Start
{
    //https://github.com/MikeWasson/LocalAccountsApp
    public class ApplicationUserManager : IdentityUser
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore(context.Get<ApplicationDbContext>()));
            //// Configure validation logic for usernames
            //manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            //{
            //    AllowOnlyAlphanumericUserNames = false,
            //    RequireUniqueEmail = true
            //};
            //// Configure validation logic for passwords
            //manager.PasswordValidator = new PasswordValidator
            //{
            //    RequiredLength = 6,
            //    RequireNonLetterOrDigit = true,
            //    RequireDigit = true,
            //    RequireLowercase = true,
            //    RequireUppercase = true,
            //};
            //var dataProtectionProvider = options.DataProtectionProvider;
            //if (dataProtectionProvider != null)
            //{
            //    manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            //}
            return manager;
        }

        internal Task<ApplicationUser> FindAsync(string userName, string password)
        {
            throw new NotImplementedException();
        }

        internal Task<ClaimsIdentity> CreateIdentityAsync(ApplicationUser applicationUser, string authenticationType)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityUser> FindByIdAsync(string getUserId)
        {
            throw new NotImplementedException();
        }

        internal Task<IdentityResult> ChangePasswordAsync(string v, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        internal Task<IdentityResult> AddPasswordAsync(string v, string newPassword)
        {
            throw new NotImplementedException();
        }

        internal Task<IdentityResult> AddLoginAsync(string v, UserLoginInfo userLoginInfo)
        {
            throw new NotImplementedException();
        }

        internal Task<IdentityResult> RemovePasswordAsync(string v)
        {
            throw new NotImplementedException();
        }

        internal Task<IdentityResult> RemoveLoginAsync(string v, UserLoginInfo userLoginInfo)
        {
            throw new NotImplementedException();
        }

        internal Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            throw new NotImplementedException();
        }

        internal Task<ApplicationUser> FindAsync(UserLoginInfo userLoginInfo)
        {
            throw new NotImplementedException();
        }

        internal Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }
    }
}