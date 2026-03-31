namespace MyBlogApp.Models.ViewModels
{
    public class EditCommentRequest
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;
    }
}