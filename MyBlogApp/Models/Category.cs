using System.ComponentModel.DataAnnotations;

namespace MyBlogApp.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "The Category Name is required")]
        [MaxLength(100, ErrorMessage = "The Category name cannot exceed 100 characters")]
        public string Name { get; set; }

        public string? Description { get; set; }
        public ICollection<Post> Posts { get; set; } //navigation property to represent the one-to-many relationship with Post/
    }
}   