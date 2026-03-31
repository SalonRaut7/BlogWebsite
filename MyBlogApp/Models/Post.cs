using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MyBlogApp.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "The title is required")]
        [MaxLength(400, ErrorMessage = "The title cannot exceed 400 characters")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "The content is required")]
        public string Content { get; set; } = string.Empty;
        
        [MaxLength(100, ErrorMessage = "The author cannot exceed 100 characters")]
        public string? Author { get; set; }
        
        // Foreign key to track which user created this post
        [ValidateNever]
        public string? ApplicationUserId { get; set; }
        
        [ValidateNever]
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }
        
        [ValidateNever]
        public string FeatureImagePath { get; set; } = string.Empty;
        
        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("Category")]
        [DisplayName("Category")]
        public int CategoryId { get; set; }
        
        [ValidateNever]
        public Category Category { get; set; } = null!;
        
        [ValidateNever]
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        [ValidateNever]
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();

        [ValidateNever]
        public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    }
}