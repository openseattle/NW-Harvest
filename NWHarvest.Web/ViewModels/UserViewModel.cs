using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Display(Name = "Notification")]
        public virtual string NotificationPreference { get; set; }
        public bool HasPassword { get; set; }
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public string ProfileUrl { get; set; }
    }
}