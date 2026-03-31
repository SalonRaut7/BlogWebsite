namespace MyBlogApp.Models.ViewModels
{
    public class AdminUsersViewModel
    {
        public IEnumerable<AdminUserRowViewModel> Users { get; set; } = Enumerable.Empty<AdminUserRowViewModel>();
    }
}