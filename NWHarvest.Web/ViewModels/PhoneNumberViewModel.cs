using System.ComponentModel.DataAnnotations;
using static NWHarvest.Web.Controllers.ManageController;

namespace NWHarvest.Web.ViewModels
{
    public class PhoneNumberViewModel
    {
        [Required]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid Phone number")]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }

        public ManageMessageId Message { get; set; }
        public string Action { get; set; }
    }
}