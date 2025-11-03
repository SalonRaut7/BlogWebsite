using Microsoft.AspNetCore.Identity;

namespace MyBlogApp.Models
{
    // This extends IdentityUser which already has:
    // - Id (string)
    // - UserName (string)
    // - Email (string)
    // - PasswordHash (string) - automatically hashed by Identity
    // - PhoneNumber, EmailConfirmed, etc.
    
    public class ApplicationUser : IdentityUser
    {
        // Custom properties as:
        public DateTime RegisteredDate { get; set; } = DateTime.Now;
        
        // Navigation property - to track which posts this user created
        public ICollection<Post>? Posts { get; set; }
    }
}