using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ganss.Xss;
using MyBlogApp.Data;
using MyBlogApp.Models;
using MyBlogApp.Models.ViewModels;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace MyBlogApp.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly HtmlSanitizer _htmlSanitizer;
        private readonly string[] _allowedExtension = { ".jpg", ".jpeg", ".png" };
        private const int PageSize = 6;
        
        public PostController(AppDbContext context, IWebHostEnvironment webHostEnvironment, HtmlSanitizer htmlSanitizer)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _htmlSanitizer = htmlSanitizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? categoryId, string? searchTerm, int pageNumber = 1)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            var postQuery = _context.Posts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.PostTags)
                    .ThenInclude(postTag => postTag.Tag)
                .Include(p => p.Likes)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                postQuery = postQuery.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var trimmedSearchTerm = searchTerm.Trim();
                postQuery = postQuery.Where(p =>
                    p.Title.Contains(trimmedSearchTerm) ||
                    p.Content.Contains(trimmedSearchTerm) ||
                    (p.Author != null && p.Author.Contains(trimmedSearchTerm)));
            }

            postQuery = postQuery.OrderByDescending(p => p.PublishedDate);

            var totalCount = await postQuery.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            var posts = await postQuery
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var viewModel = new PostIndexViewModel
            {
                Posts = posts,
                Categories = await GetCategorySelectListAsync(),
                CategoryId = categoryId,
                SearchTerm = searchTerm,
                CurrentPage = pageNumber,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var post = await _context.Posts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.PostTags)
                    .ThenInclude(postTag => postTag.Tag)
                .Include(p => p.Comments.OrderByDescending(c => c.CommentDate))
                .Include(p => p.ApplicationUser)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (post == null)
            {
                return NotFound();
            }

            ViewBag.TagNames = post.PostTags
                .Select(postTag => postTag.Tag.Name)
                .Distinct()
                .ToList();
            ViewBag.LikeCount = await _context.PostLikes.CountAsync(postLike => postLike.PostId == id);
            ViewBag.IsLiked = User.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(currentUserId)
                && await _context.PostLikes.AnyAsync(postLike => postLike.PostId == id && postLike.ApplicationUserId == currentUserId);
            return View(post);
        }

        // Any logged-in user can create posts
        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            var postViewModel = new PostViewModel
            {
                Categories = GetCategorySelectList()
            };
            return View(postViewModel);
        }

        [HttpPost]
        [Authorize] 
        public async Task<IActionResult> Create(PostViewModel postViewModel)
        {
            postViewModel.Categories = GetCategorySelectList();

            if (postViewModel.FeatureImage == null || postViewModel.FeatureImage.Length == 0)
            {
                ModelState.AddModelError("FeatureImage", "Feature image is required.");
            }

            // Set Author and UserId before validation as they can be directly taken from logged-in user making easier to not having to write name of author time and again
            postViewModel.Post.Author = User.Identity?.Name;
            postViewModel.Post.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (ModelState.IsValid)
            {
                var inputFileExtension = Path.GetExtension(postViewModel.FeatureImage!.FileName).ToLowerInvariant();
                bool isAllowed = _allowedExtension.Contains(inputFileExtension);
                if (!isAllowed)
                {
                    ModelState.AddModelError("FeatureImage", "Only .jpg, .jpeg, .png files are allowed.");
                    return View(postViewModel);
                }
                
                postViewModel.Post.Content = _htmlSanitizer.Sanitize(postViewModel.Post.Content);
                postViewModel.Post.FeatureImagePath = await UploadFileToFolder(postViewModel.FeatureImage);
                
                await _context.Posts.AddAsync(postViewModel.Post);
                await _context.SaveChangesAsync();
                await SyncPostTagsAsync(postViewModel.Post.Id, postViewModel.TagInput);
                return RedirectToAction("Index");
            }

            return View(postViewModel);
        }

        // Users can edit their own posts, Admins can edit any post
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }
            
            var postFromDb = await _context.Posts
                .Include(post => post.PostTags)
                    .ThenInclude(postTag => postTag.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (postFromDb == null)
            {
                return NotFound();
            }
            
            // Check if user owns this post or is Admin
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (postFromDb.ApplicationUserId != currentUserId && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            
            EditViewModel editViewModel = new EditViewModel()
            {
                Post = postFromDb,
                Categories = GetCategorySelectList()
            };
            editViewModel.TagInput = string.Join(", ", postFromDb.PostTags.Select(postTag => postTag.Tag.Name));
            return View(editViewModel);
        }

        [HttpPost]
        [Authorize] 
        public async Task<IActionResult> Edit(EditViewModel editViewModel)
        {
            editViewModel.Categories = GetCategorySelectList();

            if (!ModelState.IsValid)
            {
                return View(editViewModel);
            }
            
            var postFromDb = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == editViewModel.Post.Id);
            if (postFromDb == null)
            {
                return NotFound();
            }
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (postFromDb.ApplicationUserId != currentUserId && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            
            if (editViewModel.FeatureImage != null)
            {
                var inputFileExtension = Path.GetExtension(editViewModel.FeatureImage.FileName).ToLower();
                bool isAllowed = _allowedExtension.Contains(inputFileExtension);
                if (!isAllowed)
                {
                    ModelState.AddModelError("FeatureImage", "Only .jpg, .jpeg, .png files are allowed.");
                    return View(editViewModel);
                }
                
                var existingFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", Path.GetFileName(postFromDb.FeatureImagePath));
                if (System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }
                editViewModel.Post.FeatureImagePath = await UploadFileToFolder(editViewModel.FeatureImage);
            }
            else
            {
                editViewModel.Post.FeatureImagePath = postFromDb.FeatureImagePath;
            }
            editViewModel.Post.Content = _htmlSanitizer.Sanitize(editViewModel.Post.Content);
            editViewModel.Post.ApplicationUserId = postFromDb.ApplicationUserId;
            
            _context.Posts.Update(editViewModel.Post);
            await _context.SaveChangesAsync();
            await SyncPostTagsAsync(editViewModel.Post.Id, editViewModel.TagInput);
            return RedirectToAction("Index");
        }

        // Users can delete their own posts, Admins can delete any post
        [HttpGet]
        [Authorize] 
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var postFromDb = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (postFromDb == null)
            {
                return NotFound();
            }
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (postFromDb.ApplicationUserId != currentUserId && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            
            return View(postFromDb);
        }

        [HttpPost]
        [Authorize] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var postFromDb = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (postFromDb == null)
            {
                return NotFound();
            }
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (postFromDb.ApplicationUserId != currentUserId && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            
            if (!string.IsNullOrEmpty(postFromDb.FeatureImagePath))
            {
                var existingFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", Path.GetFileName(postFromDb.FeatureImagePath));
                if (System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }
            }
            
            _context.Posts.Remove(postFromDb);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment([FromBody] Comment comment)
        {
            if (comment == null || comment.PostId <= 0 || string.IsNullOrWhiteSpace(comment.Content))
            {
                return BadRequest(new { message = "Comment content is required." });
            }

            comment.UserName = User.Identity?.Name ?? string.Empty;
            if (string.IsNullOrWhiteSpace(comment.UserName))
            {
                return Unauthorized();
            }

            comment.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            comment.CommentDate = DateTime.Now;
            comment.Content = _htmlSanitizer.Sanitize(comment.Content);
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            
            return Json(new
            {
                id = comment.Id,
                userName = comment.UserName,
                commentDate = comment.CommentDate.ToString("MMMM dd, yyyy"),
                content = comment.Content
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var existingLike = await _context.PostLikes.FirstOrDefaultAsync(postLike => postLike.PostId == id && postLike.ApplicationUserId == userId);
            var liked = false;

            if (existingLike == null)
            {
                await _context.PostLikes.AddAsync(new PostLike
                {
                    PostId = id,
                    ApplicationUserId = userId,
                    LikedAt = DateTime.UtcNow
                });
                liked = true;
            }
            else
            {
                _context.PostLikes.Remove(existingLike);
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.PostLikes.CountAsync(postLike => postLike.PostId == id);

            return Json(new
            {
                liked,
                likeCount
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment([FromBody] EditCommentRequest request)
        {
            if (request == null || request.Id <= 0 || string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new { message = "Comment content is required." });
            }

            var comment = await _context.Comments.FirstOrDefaultAsync(item => item.Id == request.Id);
            if (comment == null)
            {
                return NotFound();
            }

            if (!CanManageComment(comment))
            {
                return Forbid();
            }

            comment.Content = _htmlSanitizer.Sanitize(request.Content);
            comment.IsEdited = true;
            comment.EditedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new
            {
                id = comment.Id,
                content = comment.Content,
                editedAt = comment.EditedAt?.ToString("MMM dd, yyyy")
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var comment = await _context.Comments.FirstOrDefaultAsync(item => item.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            if (!CanManageComment(comment))
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Json(new { id = comment.Id });
        }

        private List<SelectListItem> GetCategorySelectList()
        {
            return _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();
        }

        private async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }

        private async Task SyncPostTagsAsync(int postId, string? tagInput)
        {
            var existingPostTags = await _context.PostTags.Where(postTag => postTag.PostId == postId).ToListAsync();
            if (existingPostTags.Count > 0)
            {
                _context.PostTags.RemoveRange(existingPostTags);
            }

            var tagNames = (tagInput ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(name => Regex.Replace(name, @"\s+", " "))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToList();

            foreach (var tagName in tagNames)
            {
                var slug = CreateSlug(tagName);
                var tag = await _context.Tags.FirstOrDefaultAsync(item => item.Slug == slug);
                if (tag == null)
                {
                    tag = new Tag
                    {
                        Name = tagName,
                        Slug = slug
                    };
                    await _context.Tags.AddAsync(tag);
                    await _context.SaveChangesAsync();
                }

                await _context.PostTags.AddAsync(new PostTag
                {
                    PostId = postId,
                    TagId = tag.Id
                });
            }

            await _context.SaveChangesAsync();
        }

        private static string CreateSlug(string value)
        {
            var slug = value.ToLowerInvariant();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"-+", "-");
            return slug.Trim('-');
        }

        private bool CanManageComment(Comment comment)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return User.IsInRole("Admin") || (!string.IsNullOrWhiteSpace(currentUserId) && comment.ApplicationUserId == currentUserId);
        }

        private async Task<string> UploadFileToFolder(IFormFile file)
        {
            var inputFileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = Guid.NewGuid().ToString() + inputFileExtension;
            var wwwRootPath = _webHostEnvironment.WebRootPath;
            var imagesFolderPath = Path.Combine(wwwRootPath, "images");
            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }
            var filePath = Path.Combine(imagesFolderPath, fileName);
            try
            {
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            catch
            {
                throw;
            }
            return "/images/" + fileName;
        }
    }
}