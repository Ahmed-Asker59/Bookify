namespace Bookify.Web.Core.Consts
{
    public static class Errors
    {
        public const string RequiredField = "Required Field!";
        public const string MaxLength = "Length cannot be more than {1} characters.";
        public const string MaxMinLength = "The {0} must be at least {2} and at max {1} characters long.";
        public const string Duplicated = "Another record with the same {0} already exists!";
        public const string DuplicatedBook = "Book with the same name already exists with the same author!";
        public const string NotAllowedExtension = "Only .jpg, .jpeg, .png files are allowed!";
        public const string MaxSize = "File cannot be more than 2 MB!";
        public const string FutureDate = "The date cannot be in the future!";
        public const string InvalidRange = "Value shoud be between {0} and {1}.";
        public const string ConfirmedPasswordNotMatch = "The password and confirmation password do not match.";
        public const string WeakPassword = "Password should contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least eight characters long.";
        public const string InvalidUsername = "Username must be of these charachters\n[a_to_z, A_to_Z, 0_to_9, -, ., _, @, +]";
        public const string OnlyEnglishLetters = "Only English letters are allowed!";
		public const string DenySpecialCharacters = "Special characters are not allowed.";
		public const string InvalidMobileNumber = "Invalid Mobile Number!";
        public const string InvalidNationalId = "Invalid National Id!";
        public const string InvalidSerialNumber = "Invalid serial number!";
        public const string NotAvailableRental = "This book/copy is not available for rental!";
        public const string EmptyImage = "Image is Required!";
        public const string BlackListedSubscriber = "This subscriber is blacklisted.";
        public const string InactiveSubscriber = "This subscriber is inactive.";
        public const string MaxCopiesReached = "This subscriber has reached max number of copies for rentals.";
        public const string CopyInRental = "This copy is already rented.";
    }
}
