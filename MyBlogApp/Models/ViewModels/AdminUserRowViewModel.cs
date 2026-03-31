namespace MyBlogApp.Models.ViewModels
{
    public class AdminUserRowViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime RegisteredDate { get; set; }

        public bool IsCurrentUser { get; set; }

        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    }
}