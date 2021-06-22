using Company.Models;
using Core.Entities;
using Core.Interfaces;
using Core.Validation_Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Controllers
{
    public class TourController : Controller
    {
        private readonly ITourRepository _tourRepository;
        private readonly IETourLogger _eTourLogger;
        private readonly IUnitOfWork _unitOfWork;

        public TourController(ITourRepository tourRepository, IETourLogger eTourLogger, IUnitOfWork unitOfWork)
        {
            _tourRepository = tourRepository;
            _eTourLogger = eTourLogger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(bool showClosed = false)
        {

            var tourList = _tourRepository.QueryFiltered(tour => showClosed || tour.IsOpen == true);
            return View(new TourListModel
            {
                Tours = tourList,
                ShowClosed = showClosed
            });
        }

        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> New(Tour tour, [AllowedIFormFileCollectionExtensions(new string[] { ".jpg", ".png", ".jpeg" })] IFormFileCollection images, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            if (!ModelState.IsValid)
            {
                return View(new TourFormModel
                {
                    Tour = tour,
                    Images = images
                });
            }

            await _tourRepository.AddAsync(tour, images);
            await _eTourLogger.LogAsync(Log.LogType.Creation, $"{User.Identity.Name} created tour {tour.Title}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Tour created sucessfully";

            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> Edit(int id)
        {
            Tour tour = await _tourRepository.FindAsync(id);

            if (tour == null)
            {
                return NotFound();
            }

            return View(new TourFormModel
            {
                Tour = tour
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Tour tour, IFormFileCollection images, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            if ((await _tourRepository.FindAsync(tour.ID)) == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(new TourFormModel
                {
                    Tour = tour
                });
            }

            await _tourRepository.UpdateAsync(tour, images);
            await _eTourLogger.LogAsync(Log.LogType.Modification, $"{User.Identity.Name} updated tour {tour.Title}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Tour updated sucessfully";

            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> Detail(int id)
        {
            Tour tour = await _tourRepository.Queryable
                .Include(t => t.Trips)
                .ThenInclude(tr => tr.TripDiscounts)
                .ThenInclude(trd => trd.Discount)
                .FirstOrDefaultAsync(t => t.ID == id);

            if (tour == null)
            {
                return NotFound();
            }
            return View(tour);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleClose(int id, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            Tour tour = await _tourRepository.Queryable
                .Include(t => t.Trips)
                .FirstOrDefaultAsync(t => t.ID == id);

            if (tour == null)
            {
                return NotFound();
            }

            if (tour.IsOpen)
            {
                tour.Close();
            }
            else
            {
                tour.Open();
            }

            await _tourRepository.UpdateAsync(tour);
            await _eTourLogger.LogAsync(tour.IsOpen ? Log.LogType.Creation : Log.LogType.Deletion, $"{User.Identity.Name} {(tour.IsOpen ? "opened" : "closed")} tour {tour.Title}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = tour.IsOpen ? "Tour opened successfully" : "Tour closed successfully";

            return LocalRedirect(returnUrl);
        }
    }
}
