using NWHarvest.Web.Enums;
using System.ComponentModel.DataAnnotations;
using static NWHarvest.Web.Controllers.ManageController;

namespace NWHarvest.Web.ViewModels
{
    public class NotificationViewModel
    {
        public string UserName { get; set; }

        [Display(Name = "Notification Preference")]
        public UserNotification Method { get; set; }
        public ManageMessageId Message { get; set; }
    }
}