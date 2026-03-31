using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyBlogApp.Models.ViewModels
{
    public class PostViewModel
    {
        public Post Post { get; set; } = new Post();  //for binding Post data which contains Title, Content, Author, CategoryId, etc.
        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
        public IFormFile? FeatureImage { get; set; } // For uploading feature image
        public string? TagInput { get; set; }
    }
}