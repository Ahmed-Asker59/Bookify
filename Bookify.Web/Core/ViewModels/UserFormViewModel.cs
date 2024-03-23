using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class UserFormViewModel
    {
        public string? Id { get; set; }
        [MaxLength(100, ErrorMessage =Errors.MaxLength), Display(Name = "Full Name"),
            RegularExpression(RegexPatterns.CharactersOnly_EN, ErrorMessage = Errors.OnlyEnglishLetters)]
        public string FullName { get; set; } = null!;

        [MaxLength(20, ErrorMessage = Errors.MaxLength), Display(Name = "Username"), RegularExpression(RegexPatterns.Username, ErrorMessage = Errors.InvalidUsername)]
        [Remote("AllowUserName",null!,AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string UserName { get; set; } = null!;

        [MaxLength(200, ErrorMessage = Errors.MaxLength), EmailAddress]
        [Remote("AllowEmail", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string Email { get; set; } = null!;


        [StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8),
            Display(Name = "Password"), DataType(DataType.Password),
            RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.WeakPassword)]
        [RequiredIf("Id == null", ErrorMessage = Errors.RequiredField)]
        public string? Password { get; set; } = null!;

        [Compare("Password", ErrorMessage = Errors.ConfirmedPasswordNotMatch), Display(Name = "Confirm password"),
          DataType(DataType.Password)]
        [RequiredIf("Id == null", ErrorMessage = Errors.RequiredField)]
        public string? ConfirmPassword { get; set; } = null!;

        [Display(Name = "Roles")]
        public IList<string> SelectedRoles { get; set; } = new List<string>();

        //in order to render roles to select from while adding one
        public IEnumerable<SelectListItem>? Roles { get; set; }
    }
}
