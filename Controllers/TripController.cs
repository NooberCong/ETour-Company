using Company.Models;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Controllers
{
    public class TripController : Controller
    {
        private readonly ITourRepository _tourRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IDiscountRepository _discountRepository;
        private readonly IETourLogger _eTourLogger;
        private readonly IUnitOfWork _unitOfWork;

        public TripController(ITourRepository tourRepository, ITripRepository tripRepository, IDiscountRepository discountRepository, IETourLogger eTourLogger, IUnitOfWork unitOfWork)
        {
            _tourRepository = tourRepository;
            _tripRepository = tripRepository;
            _discountRepository = discountRepository;
            _eTourLogger = eTourLogger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(bool showClosed = false)
        {
            var tripList = await _tripRepository.Queryable
                .Include(tr => tr.Tour)
                .Include(tr => tr.TripDiscounts)
                .ThenInclude(trd => trd.Discount)
                .Where(tr => showClosed || tr.IsOpen).ToArrayAsync();

            var tourList = _tourRepository.QueryFiltered(t => t.IsOpen);

            return View(new TripListModel
            {
                Trips = tripList,
                Tours = tourList
            });
        }

        public async Task<IActionResult> New(int tourID, string returnUrl)
        {
            Tour tour = await _tourRepository.FindAsync(tourID);

            if (tour == null)
            {
                return NotFound();
            }

            return View(new TripFormModel
            {
                Tour = tour,
                Discounts = GetAvailableDiscounts(),
                ReturnUrl = returnUrl
            });
        }

        private IEnumerable<Discount> GetAvailableDiscounts(Trip trip = default)
        {
            return _discountRepository.Queryable
                .Include(d => d.TripDiscounts)
                .AsEnumerable()
                .Where(d => d.TripDiscounts.Any(trd =>
                (trip == null && !d.IsValid(DateTime.Now))
                || trd.TripID == trip.ID)
                || !d.IsValid(DateTime.Now));
        }

        [HttpPost]
        public async Task<IActionResult> New(Trip trip, int[] discounts, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            Tour tour = await _tourRepository.FindAsync(trip.TourID);

            if (tour == null)
            {
                return NotFound();
            }

            var errors = trip.GetValidationErrors();

            if (!ModelState.IsValid || errors.Any())
            {
                ModelState.AddModelErrors(errors);

                return View(new TripFormModel
                {
                    Tour = tour,
                    Trip = trip,
                    Discounts = GetAvailableDiscounts(),
                    AppliedDiscounts = discounts,
                    ReturnUrl = returnUrl
                });
            }

            trip.IsOpen = tour.IsOpen;

            await _tripRepository.AddAsync(trip, discounts);
            await _eTourLogger.LogAsync(Log.LogType.Creation, $"{User.Identity.Name} created trip #{trip.ID} - {tour.Title}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Trip created successfully";
            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> Edit(int id, string returnUrl)
        {
            Trip trip = await _tripRepository.Queryable
                .Include(t => t.Tour)
                .Include(t => t.TripDiscounts)
                .FirstOrDefaultAsync(t => t.ID == id);

            if (trip == null)
            {
                return NotFound();
            }

            return View(new TripFormModel
            {
                Tour = trip.Tour,
                Trip = trip,
                Discounts = GetAvailableDiscounts(trip),
                AppliedDiscounts = trip.TripDiscounts.Select(td => td.DiscountID).ToArray(),
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Trip trip, int[] discounts, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            Trip existingTrip = await _tripRepository.Queryable
                            .Include(t => t.Tour)
                            .FirstOrDefaultAsync(t => t.ID == trip.ID);

            if (existingTrip == null)
            {
                return NotFound();
            }

            var errors = trip.GetValidationErrors();

            if (!ModelState.IsValid || errors.Any())
            {
                ModelState.AddModelErrors(errors);

                return View(new TripFormModel
                {
                    Tour = existingTrip.Tour,
                    Trip = trip,
                    Discounts = GetAvailableDiscounts(trip),
                    AppliedDiscounts = discounts,
                    ReturnUrl = returnUrl
                });
            }

            await _tripRepository.UpdateAsync(trip, discounts);
            await _eTourLogger.LogAsync(Log.LogType.Modification, $"{User.Identity.Name} updated trip #{trip.ID} - {existingTrip.Tour.Title}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Trip updated successfully";

            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleClose(int id, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            Trip trip = await _tripRepository.Queryable
                .Include(tr => tr.Tour)
                .FirstOrDefaultAsync(tr => tr.ID == id);

            if (trip == null)
            {
                return NotFound();
            }

            if (trip.IsOpen)
            {
                trip.Close();
            }
            else if (trip.CanOpen())
            {
                trip.Open();
            }
            else
            {
                // Trip cannot be open when it's tour is closed
                TempData["StatusMessage"] = "Error: Trip cannot be open when it's tour is closed";
                return LocalRedirect(returnUrl);
            }

            await _tripRepository.UpdateAsync(trip);
            await _eTourLogger.LogAsync(Log.LogType.Deletion, $"{User.Identity.Name} {(trip.IsOpen ? "open" : "closed")} trip #{trip.ID} - {trip.Tour.Title}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = trip.IsOpen ? "Trip opened successfully" : "Trip closed successfully";

            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var trip = await _tripRepository.Queryable
                .Include(tr => tr.Tour)
                .Include(tr => tr.TripDiscounts)
                .ThenInclude(trd => trd.Discount)
                .Include(tr => tr.Bookings)
                .Include(tr => tr.Itineraries)
                .FirstOrDefaultAsync(tr => tr.ID == id);

            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }
    }
}
