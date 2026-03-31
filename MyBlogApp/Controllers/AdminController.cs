using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlogApp.Data;
using MyBlogApp.Models;
using MyBlogApp.Models.ViewModels;

namespace MyBlogApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var dashboardViewModel = new AdminDashboardViewModel
            {
                TotalPosts = await _context.Posts.CountAsync(),
                TotalComments = await _context.Comments.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                RecentPosts = await _context.Posts
                    .AsNoTracking()
                    .Include(post => post.Category)
                    .OrderByDescending(post => post.PublishedDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboardViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var currentUserId = _userManager.GetUserId(User);
            var users = await _userManager.Users
                .AsNoTracking()
                .OrderBy(user => user.UserName)
                .ToListAsync();

            var userRows = new List<AdminUserRowViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRows.Add(new AdminUserRowViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    RegisteredDate = user.RegisteredDate,
                    IsCurrentUser = user.Id == currentUserId,
                    Roles = roles
                });
            }

            var viewModel = new AdminUsersViewModel
            {
                Users = userRows
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(error => error.Description));
            }
            else
            {
                TempData["SuccessMessage"] = $"Deleted user {user.UserName}.";
            }

            return RedirectToAction(nameof(Users));
        }
    }
}