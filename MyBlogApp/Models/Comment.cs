using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MyBlogApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "The User Name is required")]
        [MaxLength(100, ErrorMessage = "The user name cannot exceed 100 characters")]
        public string UserName { get; set; } = string.Empty;
        [DataType(DataType.Date)]
        public DateTime CommentDate { get; set; }
        [Required(ErrorMessage = "The content is required")]
        public string Content { get; set; } = string.Empty;

        public string? ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }

        public bool IsEdited { get; set; }

        public DateTime? EditedAt { get; set; }

        [ForeignKey("Post")]
        public int PostId { get; set; }
        public Post Post { get; set; } = null!; //navigation property to represent the many-to-one relationship with Post

    }
}