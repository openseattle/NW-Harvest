using NWHarvest.Web.Enums;
using System.ComponentModel.DataAnnotations;

namespace NWHarvest.Web.ViewModels
{
    public class RegisterViewModel : AddressViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "User Type")]
        public string UserType { get; set; }

        [Required]
        [Display(Name = "Program Name")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Confirm E-mail")]
        [Compare("Email", ErrorMessage = "The E-mail and confirmation E-mail do not match.")]
        public string ConfirmEmail { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public override string Address1 { get; set; }

        [Required]
        public override string City { get; set; }
        
        [Required]
        public override string County { get; set; }

        [Required]
        [DataType(DataType.PostalCode)]
        [RegularExpression(@"^\d{5}(?:[-\s]\d{4})?$", ErrorMessage = "The zip code is not valid.")]
        [Display(Name = "Zip Code")]
        public override string Zip { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Notification Method")]
        public UserNotification Notification { get; set; }
    }
}