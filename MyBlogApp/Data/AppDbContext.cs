using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyBlogApp.Models;

namespace MyBlogApp.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet is used to represent collections of the specified entity types within the context (tables in the database)
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Roles into AspNetRoles table
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole 
                { 
                    Id = "1", 
                    Name = "Admin", 
                    NormalizedName = "ADMIN" 
                },
                new IdentityRole 
                { 
                    Id = "2", 
                    Name = "User", 
                    NormalizedName = "USER" 
                }
            );

            // Seed Categories by default we have these three categories:
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Technology" },
                new Category { Id = 2, Name = "Health" },
                new Category { Id = 3, Name = "Lifestyle" }
            );

            // Seed Posts by default to see if these post appear on the home page or not
            modelBuilder.Entity<Post>().HasData(
                new Post
                {
                    Id = 1,
                    Title = "Tech Post 1",
                    Content = "Content of Tech Post 1",
                    Author = "John Doe",
                    PublishedDate = new DateTime(2023, 1, 1),
                    CategoryId = 1,
                    FeatureImagePath = "tech_image.jpg",
                    ApplicationUserId = null  //first no user was assigned then the post was created as it had no authentication/authorization before , but now after all post creation the id will be created by default on who created the post
                },
                new Post
                {
                    Id = 2,
                    Title = "Health Post 1",
                    Content = "Content of Health Post 1",
                    Author = "Jane Doe",
                    PublishedDate = new DateTime(2023, 1, 1),
                    CategoryId = 2,
                    FeatureImagePath = "health_image.jpg",
                    ApplicationUserId = null
                },
                new Post
                {
                    Id = 3,
                    Title = "Lifestyle Post 1",
                    Content = "Content of Lifestyle Post 1",
                    Author = "Alex Smith",
                    PublishedDate = new DateTime(2023, 1, 1),
                    CategoryId = 3,
                    FeatureImagePath = "lifestyle_image.jpg",
                    ApplicationUserId = null
                }
            );

            modelBuilder.Entity<Tag>()
                .HasIndex(tag => tag.Slug)
                .IsUnique();

            modelBuilder.Entity<PostTag>()
                .HasKey(postTag => new { postTag.PostId, postTag.TagId });

            modelBuilder.Entity<PostTag>()
                .HasOne(postTag => postTag.Post)
                .WithMany(post => post.PostTags)
                .HasForeignKey(postTag => postTag.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostTag>()
                .HasOne(postTag => postTag.Tag)
                .WithMany(tag => tag.PostTags)
                .HasForeignKey(postTag => postTag.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostLike>()
                .HasIndex(postLike => new { postLike.PostId, postLike.ApplicationUserId })
                .IsUnique();

            modelBuilder.Entity<PostLike>()
                .HasOne(postLike => postLike.Post)
                .WithMany(post => post.Likes)
                .HasForeignKey(postLike => postLike.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostLike>()
                .HasOne(postLike => postLike.ApplicationUser)
                .WithMany()
                .HasForeignKey(postLike => postLike.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(comment => comment.ApplicationUser)
                .WithMany()
                .HasForeignKey(comment => comment.ApplicationUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Comment>()
                .Property(comment => comment.IsEdited)
                .HasDefaultValue(false);
        }
    }
}