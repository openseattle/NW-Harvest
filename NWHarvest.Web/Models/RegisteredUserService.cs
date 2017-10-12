using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Web;

namespace NWHarvest.Web.Models
{
    public class RegisteredUser
    {
        public int GrowerId { get; set; }
        public int FoodBankId { get; set; }
        public string Role { get; set; }
        public string UserName { get; set; }
    }

    public class RegisteredUserService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public RegisteredUser GetRegisteredUser(System.Security.Principal.IPrincipal user)
        {
            var registeredUser = new RegisteredUser();

            var userId = user.Identity.GetUserId();

            var growersResults = (from b in db.Growers
                                 where b.UserId == userId
                                 select b).ToList();
            
            var foodBankResults = (from b in db.FoodBanks
                                  where b.UserId == userId
                                  select b).ToList();

            if (user.IsInRole(UserRoles.AdministratorRole))
            {
                registeredUser.Role = UserRoles.AdministratorRole;
            }

            else if (growersResults.Any())
            {
                registeredUser.Role = UserRoles.GrowerRole;
                registeredUser.GrowerId = growersResults.FirstOrDefault().Id;
                registeredUser.UserName = growersResults.FirstOrDefault().name;
            }

            else if (foodBankResults.Any())
            {
                registeredUser.Role = UserRoles.FoodBankRole;
                registeredUser.FoodBankId = foodBankResults.FirstOrDefault().Id;
                registeredUser.UserName = foodBankResults.FirstOrDefault().name;
            }

            return registeredUser;
        }

        public bool IsValidUserNameForLoginType(UserManager<ApplicationUser> userManager, string email, string loginType)
        {
            var userIsValid = false;

            if (loginType == UserRoles.FoodBankRole)
            {
                userIsValid = UserIsFoodBank(email);
            }

            else if (loginType == UserRoles.GrowerRole)
            {
                userIsValid = UserIsGrower(email);
            }

            else if (loginType == UserRoles.AdministratorRole)
            {
                userIsValid = UserIsAdministrator(userManager, email);
            }

            return userIsValid;
        }

        public bool IsEmailConfirmed(string email)
        {
            var results = db.Users.Where(b => b.Email == email);

            return results.FirstOrDefault().EmailConfirmed;
        }

        private bool UserIsGrower(string email)
        {
            var results = db.Growers.Where(b => b.email == email).ToList();

            return results.Count > 0;
        }

        private bool UserIsFoodBank(string email)
        {
            var results = db.FoodBanks.Where(b => b.email == email).ToList();

            return results.Count > 0;
        }

        private bool UserIsAdministrator(UserManager<ApplicationUser> userManager, string email)
        {
            var user = userManager.FindByEmail(email);
            if (user == null)
            {
                return false;
            }

            return userManager.IsInRole(user.Id, UserRoles.AdministratorRole);
            //return email == UserRoles.Administrator;
        }

        public bool IsUserActive(string email, string loginType)
        {
            var result = false;

            if (loginType == UserRoles.FoodBankRole)
            {
                result = db.FoodBanks.Where(f => f.email == email).FirstOrDefault().IsActive;
            }

            else if (loginType == UserRoles.GrowerRole)
            {
                result = db.Growers.Where(f => f.email == email).FirstOrDefault().IsActive;
            }

            else if (loginType == UserRoles.AdministratorRole)
            {
                result = true;
            }

            return result;
        }
    }
}