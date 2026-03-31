using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlogApp.Models
{
    public class PostLike
    {
        [Key]
        public int Id { get; set; }

        public int PostId { get; set; }

        [ForeignKey(nameof(PostId))]
        public Post Post { get; set; } = null!;

        public string ApplicationUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}