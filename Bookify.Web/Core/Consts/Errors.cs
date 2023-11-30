namespace Bookify.Web.Core.Consts
{
    public static class Errors
    {
        public const string MaxLength = "Length cannot be more than {1} characters";
        public const string Duplicated = "{0} with the same name already exists!";
        public const string DuplicatedBook = "Book with the same name already exists with the same author!";
        public const string NotAllowedExtension = "Only .jpg, .jpeg, .png files are allowed!";
        public const string MaxSize = "File cannot be more than 2 MB!";
        public const string FutureDate = "The date cannot be in the future!";
    }
}
