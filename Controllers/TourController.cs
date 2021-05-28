using Company.Models;
using Core.Entities;
using Core.Interfaces;
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
        private readonly IRemoteFileStorageHandler _remoteFileStorageHandler;
        private readonly IUnitOfWork _unitOfWork;

        public TourController(ITourRepository tourRepository, IRemoteFileStorageHandler remoteFileStorageHandler, IUnitOfWork unitOfWork)
        {

            _tourRepository = tourRepository;
            _remoteFileStorageHandler = remoteFileStorageHandler;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(bool showClosed = false)
        {

            var tourList = _tourRepository.QueryFiltered(tour => showClosed || tour.IsOpen == true);
            return View(new TourListModel
            {
                Tours = tourList.Select(TourListItem.FromTourEntity),
                ShowClosed = showClosed
            });
        }

        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New([Bind("Title", "StartPlace", "Destination", "Description", "Type")] Tour tour, IFormFileCollection images)
        {
            if (!ModelState.IsValid)
            {
                return View(new TourFormModel
                {
                    Tour = tour,
                    Images = images
                });
            }
            foreach (var file in images.AsEnumerable())
            {
                using var stream = file.OpenReadStream();
                string url = await _remoteFileStorageHandler.UploadAsync(stream, "jpg");
                tour.ImageUrls.Add(url);
            }
            await _tourRepository.AddAsync(tour);
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Tour created sucessfully";

            return RedirectToAction("Index");
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID", "Title", "StartPlace", "Destination", "Description", "Type")] Tour tour, IFormFileCollection images)
        {
            Tour existingTour;

            if (id != tour.ID || (existingTour = await _tourRepository.FindAsync(id)) == null)
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

            if (images.Any())
            {
                foreach (var imageUri in existingTour.ImageUrls)
                {
                    await _remoteFileStorageHandler.DeleteAsync(imageUri);
                }

                foreach (var file in images.AsEnumerable())
                {
                    using var stream = file.OpenReadStream();
                    string url = await _remoteFileStorageHandler.UploadAsync(stream, "jpg");
                    tour.ImageUrls.Add(url);
                }
            }
            else
            {
                tour.ImageUrls = existingTour.ImageUrls;
            }

            await _tourRepository.UpdateAsync(tour);
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Tour updated sucessfully";

            return RedirectToAction("Index");
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleClose(int id, string returnUrl = "~/")
        {
            Tour tour = await _tourRepository.FindAsync(id);
            if (tour == null)
            {
                return NotFound();
            }

            tour.IsOpen = !tour.IsOpen;

            await _tourRepository.UpdateAsync(tour);

            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = tour.IsOpen ? "Tour opened successfully" : "Tour closed successfully";

            return LocalRedirect(returnUrl);
        }
    }
}
