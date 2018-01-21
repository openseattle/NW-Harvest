using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Web;

namespace NWHarvest.Web.Helper
{
    public class NotificationManager
    {
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // todo: implement enum Notifications
        // see Account/Registration for list of valid values
        // both
        // emailNote
        // textNote
        public void SendNotification(NotificationMessage message, string notificationPreference)
        {
            switch (notificationPreference)
            {
                case "both":
                    SendSmsNotification(message);
                    SendEmailNotification(message);
                    break;

                case "emailNote":
                    SendEmailNotification(message);
                    break;

                case "textnote":
                    SendSmsNotification(message);
                    break;

                default:
                    return;
            }
        }

        private void SendSmsNotification(NotificationMessage message)
        {
            if(string.IsNullOrEmpty(message.DestinationPhoneNumber))
            {
                return;
            }

            var identityMessage = new IdentityMessage
            {
                Destination = message.DestinationPhoneNumber,
                Subject = string.Empty,
                Body = message.Body
            };

            UserManager.SmsService.SendAsync(identityMessage).Wait();
        }

        private void SendEmailNotification(NotificationMessage message)
        {
            if (string.IsNullOrEmpty(message.DestinationPhoneNumber))
            {
                return;
            }

            var identityMessage = new IdentityMessage
            {
                Destination = message.DestinationEmailAddress,
                Subject = message.Subject,
                Body = message.Body
            };

            UserManager.SmsService.SendAsync(identityMessage).Wait();
        }

        public string GetUserPhoneNumber(string userId)
        {
            return UserManager.GetPhoneNumber(userId);
        }
    }
}