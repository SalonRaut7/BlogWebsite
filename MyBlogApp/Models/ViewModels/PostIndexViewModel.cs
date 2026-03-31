using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyBlogApp.Models.ViewModels
{
    public class PostIndexViewModel
    {
        public IEnumerable<Post> Posts { get; set; } = Enumerable.Empty<Post>();

        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();

        public int? CategoryId { get; set; }

        public string? SearchTerm { get; set; }

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; } = 1;
    }
}