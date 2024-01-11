namespace Bookify.Web.Core.Consts
{
    public static class RegexPatterns
    {
        public const string Password = "(?=(.*[0-9]))(?=.*[\\!@#$%^&*()\\\\[\\]{}\\-_+=~`|:;\"'<>,./?])(?=.*[a-z])(?=(.*[A-Z]))(?=(.*)).{8,}";
        public const string Username = "^[a-zA-Z0-9-._@+]*$";
        public const string CharactersOnly_EN = "^[a-zA-Z-_ ]*$";
        public const string DenySpecialCharacters = "^[^<>!#%$]*$";
        public const string MobileNumber = "^01[0,1,2,5]{1}[0-9]{8}$";
		public const string NationalId = "^[2,3]{1}[0-9]{13}$";
	}
}
