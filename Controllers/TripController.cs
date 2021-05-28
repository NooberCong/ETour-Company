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
        private readonly ITripDiscountRepository _tripDiscountRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TripController(ITourRepository tourRepository, ITripRepository tripRepository, IDiscountRepository discountRepository, ITripDiscountRepository tripDiscountRepository, IUnitOfWork unitOfWork)
        {
            _tourRepository = tourRepository;
            _tripRepository = tripRepository;
            _discountRepository = discountRepository;
            _tripDiscountRepository = tripDiscountRepository;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> New(int tourID, string returnUrl)
        {
            Tour tour = await _tourRepository.FindAsync(tourID);

            if (tour == null || !tour.IsOpen)
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

        private IEnumerable<Discount> GetAvailableDiscounts()
        {
            return _discountRepository.Queryable.AsEnumerable().Where(d => d.IsValid(DateTime.Now));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(Trip trip, int[] discounts, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            Tour tour = await _tourRepository.FindAsync(trip.TourID);

            if (tour == null || !tour.IsOpen)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(new TripFormModel
                {
                    Tour = tour,
                    Trip = trip,
                    Discounts = GetAvailableDiscounts(),
                    AppliedDiscounts = discounts,
                    ReturnUrl = returnUrl
                });
            }

            foreach (var discountID in discounts)
            {
                trip.TripDiscounts.Add(new TripDiscount { TripID = trip.ID, DiscountID = discountID });
            }

            await _tripRepository.AddAsync(trip);
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

            if (trip == null || !trip.IsOpen || !trip.Tour.IsOpen)
            {
                return NotFound();
            }

            return View(new TripFormModel
            {
                Tour = trip.Tour,
                Trip = trip,
                Discounts = GetAvailableDiscounts(),
                AppliedDiscounts = trip.TripDiscounts.Select(td => td.DiscountID).ToArray(),
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Trip trip, int[] discounts, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            Trip existingTrip = await _tripRepository.Queryable
                            .Include(t => t.Tour)
                            .Include(t => t.TripDiscounts)
                            .FirstOrDefaultAsync(t => t.ID == id);

            if (id != trip.ID || existingTrip == null || !existingTrip.IsOpen || !existingTrip.Tour.IsOpen)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(new TripFormModel
                {
                    Tour = existingTrip.Tour,
                    Trip = trip,
                    Discounts = GetAvailableDiscounts(),
                    AppliedDiscounts = discounts,
                    ReturnUrl = returnUrl
                });
            }

            foreach (var tripDisc in existingTrip.TripDiscounts)
            {
                tripDisc.Trip = trip;
                await _tripDiscountRepository.DeleteAsync(tripDisc);
            }

            foreach (var discountID in discounts)
            {
                await _tripDiscountRepository.AddAsync(new TripDiscount { DiscountID = discountID, TripID = trip.ID });
            }

            await _tripRepository.UpdateAsync(trip);
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Trip updated successfully";

            return LocalRedirect(returnUrl);
        }
    }
}
