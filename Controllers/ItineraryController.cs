using System.Linq;
using System.Threading.Tasks;
using Company.Models;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Company.Controllers
{
    [Authorize(Roles = "Admin,Travel")]
    public class ItineraryController : Controller
    {
        private readonly ITripRepository _tripRepository;
        private readonly IItineraryRepository _itineraryRepository;
        private readonly IETourLogger _eTourLogger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUnitOfWork _unitOfWork;

        public ItineraryController(ITripRepository tripRepository, IItineraryRepository itineraryRepository, IETourLogger eTourLogger, IAuthorizationService authorizationService, IUnitOfWork unitOfWork)
        {
            _tripRepository = tripRepository;
            _itineraryRepository = itineraryRepository;
            _eTourLogger = eTourLogger;
            _authorizationService = authorizationService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> New(int tripID)
        {
            var trip = await _tripRepository.Queryable
                .Include(tr => tr.Tour)
                .FirstOrDefaultAsync(tr => tr.ID == tripID);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, trip, "OwnedTrip");

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            if (trip == null)
            {
                return NotFound();
            }

            return View(new ItineraryFormModel
            {
                Trip = trip
            });
        }

        [HttpPost]
        public async Task<IActionResult> New(Itinerary itinerary, string returnUrl)
        {
            returnUrl ??= Url.Action("Detail", "Trip", new { id = itinerary.TripID });

            var trip = await _tripRepository.Queryable
                .Include(tr => tr.Tour)
                .FirstOrDefaultAsync(tr => tr.ID == itinerary.TripID);

            if (trip == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, trip, "OwnedTrip");

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var errors = itinerary.GetValidationErrors(trip);

            if (!ModelState.IsValid || errors.Any())
            {
                ModelState.AddModelErrors(errors);

                return View(new ItineraryFormModel
                {
                    Trip = trip,
                    Itinerary = itinerary
                });
            }

            await _itineraryRepository.AddAsync(itinerary);
            await _eTourLogger.LogAsync(Log.LogType.Creation, $"{User.Identity.Name} created itinerary #{itinerary.Title} - Trip {trip.ID}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Itinerary created successfully";
            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var itinerary = await _itineraryRepository.Queryable
                .Include(it => it.Trip)
                .ThenInclude(tr => tr.Tour)
                .FirstOrDefaultAsync(tr => tr.ID == id);

            if (itinerary == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, itinerary.Trip, "OwnedTrip");

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return View(new ItineraryFormModel
            {
                Trip = itinerary.Trip,
                Itinerary = itinerary
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Itinerary itinerary, string returnUrl)
        {
            returnUrl ??= Url.Action("Detail", "Trip", new { id = itinerary.TripID });

            var existingItinerary = await _itineraryRepository.Queryable
                .Include(it => it.Trip)
                .ThenInclude(tr => tr.Tour)
                .FirstOrDefaultAsync(tr => tr.ID == itinerary.ID);

            if (existingItinerary == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, existingItinerary.Trip, "OwnedTrip");

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var errors = itinerary.GetValidationErrors(existingItinerary.Trip);

            if (!ModelState.IsValid || errors.Any())
            {
                ModelState.AddModelErrors(errors);

                return View(new ItineraryFormModel
                {
                    Trip = existingItinerary.Trip,
                    Itinerary = itinerary
                });
            }

            await _itineraryRepository.UpdateAsync(itinerary);
            await _eTourLogger.LogAsync(Log.LogType.Modification, $"{User.Identity.Name} updated itinerary #{itinerary.Title} - Trip {itinerary.TripID}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Itinerary updated successfully";
            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, string returnUrl)
        {
            var itinerary = await _itineraryRepository.Queryable
                .Include(itin => itin.Trip)
                .FirstOrDefaultAsync(itin => itin.ID == id);

            if (itinerary == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, itinerary.Trip, "OwnedTrip");

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            returnUrl ??= Url.Action("Detail", "Trip", new { id = itinerary.TripID });

            await _itineraryRepository.DeleteAsync(itinerary);
            await _eTourLogger.LogAsync(Log.LogType.Deletion, $"{User.Identity.Name} deleted itinerary #{itinerary.Title} - Trip {itinerary.TripID}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Itinerary deleted successfully";
            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Import(int destinationTripID, int sourceTripID, string returnUrl)
        {
            returnUrl ??= Url.Action("Detail", "Trip", new { id = destinationTripID });

            var sourceTrip = await _tripRepository.Queryable
                .Include(tr => tr.Itineraries)
                .FirstOrDefaultAsync(tr => tr.ID == sourceTripID);

            var destinationTrip = await _tripRepository.FindAsync(destinationTripID);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, destinationTrip, "OwnedTrip");

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            if (sourceTrip == null || destinationTrip == null)
            {
                return NotFound();
            }

            foreach (var itinerary in sourceTrip.Itineraries)
            {
                await _itineraryRepository.CopyTo(destinationTripID, itinerary);
            }

            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Itineraries imported successfully";
            return LocalRedirect(returnUrl);
        }

    }
}
