namespace Bookify.Web.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(100,ErrorMessage = "Maximum length cannot be more than 100 characters")]
        public string Name { get; set; } = null!;
    }
}
