using Company.Models;
using Core.Entities;
using Core.Interfaces;
using Core.Validation_Attributes;
using Infrastructure.InterfaceImpls;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Company.Controllers
{
    public class BlogController : Controller
    {
        private readonly IPostRepository<Post, Employee> _blogRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IETourLogger _eTourLogger;

        public BlogController(IPostRepository<Post, Employee> blogRepository, IUnitOfWork unitOfWork, IETourLogger eTourLogger)
        {
            _blogRepository = blogRepository;
            _unitOfWork = unitOfWork;
            _eTourLogger = eTourLogger;
        }

        public IActionResult Index(IPost<Employee>.PostCategory? category, bool showHidden = false)
        {
            IEnumerable<Post> bloglist = _blogRepository.Queryable.Include(p => p.Owner)
                .Where(post => showHidden || !post.IsSoftDeleted && category == null || post.Category == category).AsEnumerable();

            return View(new BlogListModel
            {
                Posts = bloglist,
                ShowHidden = showHidden
            });
        }

        public IActionResult New()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> New(Post post, [AllowedIFormFileExtensions(new string[] { ".jpg", ".png", ".jpeg" })] IFormFile coverImg, string commaSeparatedTags, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            string empID = User.Claims.First(cl => cl.Type == ClaimTypes.NameIdentifier).Value;
            post.OwnerID = empID;
            post.Tags = commaSeparatedTags.Split(",", System.StringSplitOptions.TrimEntries | System.StringSplitOptions.RemoveEmptyEntries).ToList();

            if (!ModelState.IsValid)
            {
                return View(new BlogFormModel
                {
                    Post = post,
                    CoverImg = coverImg
                });
            }
            await _blogRepository.AddAsync(post, coverImg);
            await _eTourLogger.LogAsync(Log.LogType.Creation, $"{User.Identity.Name} created post {post.Title}");
            await _unitOfWork.CommitAsync();
            TempData["StatusMessage"] = "Blog created sucessfully";

            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> Detail(int id)
        {
            Post post = await _blogRepository.Queryable.Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }


        public async Task<IActionResult> Edit(int id)
        {
            Post post = await _blogRepository.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            return View(new BlogFormModel
            {
                Post = post,
                CommaSeparatedTags = string.Join(", ", post.Tags)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Post post, IFormFile coverImg, string commaSeparatedTags, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            var existingPost = await _blogRepository.FindAsync(post.ID);

            if (existingPost == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(new BlogFormModel
                {
                    Post = post,
                    CoverImg = coverImg,
                    CommaSeparatedTags = commaSeparatedTags
                });
            }

            post.OwnerID = existingPost.OwnerID;
            post.Tags = commaSeparatedTags.Split(",", System.StringSplitOptions.TrimEntries | System.StringSplitOptions.RemoveEmptyEntries).ToList();

            await _blogRepository.UpdateAsync(post, coverImg);
            await _eTourLogger.LogAsync(Log.LogType.Modification, $"{User.Identity.Name} updated post {post.Title}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Blog updated sucessfully";

            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleVisibility(int id, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            var post = await _blogRepository.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            if (post.IsSoftDeleted)
            {
                post.Show();
            }
            else
            {
                post.Hide();
            }

            await _blogRepository.UpdateAsync(post, null);
            await _eTourLogger.LogAsync(post.IsSoftDeleted ? Log.LogType.Deletion : Log.LogType.Creation, $"{User.Identity.Name} changed state to {(post.IsSoftDeleted ? "hidden" : "visible")} for post {post.Title}");
            await _unitOfWork.CommitAsync();
            TempData["StatusMessage"] = post.IsSoftDeleted ? "Post hidden successfully" : "Post shown successfullly";

            return LocalRedirect(returnUrl);
        }


    }
}
