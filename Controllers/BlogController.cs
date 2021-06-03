using Company.Models;
using Company.Models.Blog;
using Core.Interfaces;
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
        private readonly IRemoteFileStorageHandler _remoteFileStorageHandler;
        private readonly IUnitOfWork _unitOfWork;


        public BlogController(IPostRepository<Post, Employee> blogRepository, IRemoteFileStorageHandler remoteFileStorageHandler, IUnitOfWork unitOfWork)
        {
            _blogRepository = blogRepository;
            _remoteFileStorageHandler = remoteFileStorageHandler;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(bool showClosed = false)
        {
            IEnumerable<Post> bloglist = _blogRepository.Queryable.Include(p => p.Author)
                .Where(post => showClosed || post.IsDeleted == true).AsEnumerable();
            //  .QueryFiltered(post => showClosed || post.IsDeleted == true);

            return View(new BlogListModel
            {
                Posts = bloglist,
                ShowClosed = showClosed
            });
        }

        public IActionResult New()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> New(Post post, IFormFileCollection images)
        {
            //Lấy ID
            string empID = User.Claims.First(cl => cl.Type == ClaimTypes.NameIdentifier).Value;
            post.AuthorID = empID;
            if (!ModelState.IsValid)
            {
                return View(new BlogFormModel
                {
                    Post = post,
                    Images = images
                });
            }
            await _blogRepository.AddAsync(post);
            await _unitOfWork.CommitAsync();
            TempData["StatusMessage"] = "Blog created sucessfully";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Detail(int id)
        {
            Post post = await _blogRepository.FindAsync(id);
            return View(new BlogFormModel
            {
                Post = post
            });
        }


        public async Task<IActionResult> Edit(int id)
        {
            Post post = await _blogRepository.FindAsync(id);

            //Post post = _blogRepository.Queryable.Include(p => p.Author)
            //    .Where().AsEnumerable();

            if (post == null)
            {
                return NotFound();
            }

            return View(new BlogFormModel
            {
                Post = post,

            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Post post)
        {
            string returnUrl = Url.Action("Index");

            if ((await _blogRepository.FindAsync(post.ID)) == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(new BlogFormModel
                {
                    Post = post,
                    //Images = images
                });
            }

            await _blogRepository.UpdateAsync(post);
            // await _eTourLogger.LogAsync(Log.LogType.Modification, $"{User.Identity.Name} updated tour {tour.Title}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Blog updated sucessfully";

            return LocalRedirect(returnUrl);
        }
        //[HttpPost]
        // public Task<IActionResult> Delete(int id)
        //{

        //return RedirectToAction("Index");
        //}


    }
}
