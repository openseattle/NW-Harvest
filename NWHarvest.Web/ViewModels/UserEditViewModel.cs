using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class UserEditViewModel : UserViewModel
    {
        [Required]
        public override string NotificationPreference { get; set; }
    }
}