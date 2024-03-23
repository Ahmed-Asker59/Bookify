namespace Bookify.Web.Core.ViewModels
{
    public class ResetPasswordFormViewModel
    {
        public string Id { get; set; } = null!;
        [StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8),
            Display(Name = "Password"), DataType(DataType.Password),
            RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.WeakPassword)]
        public string Password { get; set; } = null!;

        [Compare("Password", ErrorMessage = Errors.ConfirmedPasswordNotMatch), Display(Name = "Confirm password"),
          DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
    }
}
