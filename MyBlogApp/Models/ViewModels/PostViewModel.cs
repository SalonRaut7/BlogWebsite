using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyBlogApp.Models.ViewModels
{
    public class PostViewModel
    {
        public Post Post { get; set; }  //for binding Post data which contains Title, Content, Author, CategoryId, etc.
        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IFormFile FeatureImage { get; set; } // For uploading feature image
    }
}