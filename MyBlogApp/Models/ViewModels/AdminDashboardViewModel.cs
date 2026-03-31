namespace MyBlogApp.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalPosts { get; set; }

        public int TotalComments { get; set; }

        public int TotalUsers { get; set; }

        public int TotalCategories { get; set; }

        public IEnumerable<Post> RecentPosts { get; set; } = Enumerable.Empty<Post>();
    }
}