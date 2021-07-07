using Client.Models;
using Company.Models;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Controllers
{
    [Authorize(Roles = "Admin,Travel")]
    public class DiscountController : Controller
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IETourLogger _eTourLogger;
        private readonly IUnitOfWork _unitOfWork;

        public DiscountController(IDiscountRepository discountRepository, IETourLogger eTourLogger, IUnitOfWork unitOfWork)
        {
            _discountRepository = discountRepository;
            _eTourLogger = eTourLogger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(bool showExpired = false)
        {
            IEnumerable<Discount> discounts = _discountRepository.Queryable
                .Include(d => d.TripDiscounts)
                .ThenInclude(trd => trd.Trip)
                .ThenInclude(t => t.Tour)
                .AsEnumerable()
                .Where(d => showExpired || !d.IsExpired(DateTime.Now));

            return View(new DiscountListModel
            {
                Discounts = discounts,
                ShowExpired = showExpired
            });
        }

        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> New(Discount discount, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            var errors = discount.GetValidationErrors();

            if (!ModelState.IsValid || errors.Any())
            {
                ModelState.AddModelErrors(errors);
                return View();
            }

            await _discountRepository.AddAsync(discount);
            await _eTourLogger.LogAsync(Log.LogType.Creation, $"{User.Identity.Name} created discount {discount.Title}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Discount created successfully";
            return LocalRedirect(returnUrl);
        }


        public async Task<IActionResult> Edit(int id)
        {
            Discount discount = await _discountRepository.FindAsync(id);

            if (discount == null)
            {
                return NotFound();
            }

            return View(discount);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Discount discount, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            var errors = discount.GetValidationErrors();

            if (!ModelState.IsValid || errors.Any())
            {
                ModelState.AddModelErrors(errors);
                return View(discount);
            }

            await _discountRepository.UpdateAsync(discount);
            await _eTourLogger.LogAsync(Log.LogType.Modification, $"{User.Identity.Name} updated discount {discount.Title}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Discount updated successfully";
            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            Discount discount = await _discountRepository.FindAsync(id);

            if (discount == null)
            {
                return NotFound();
            }

            await _discountRepository.DeleteAsync(discount);
            await _eTourLogger.LogAsync(Log.LogType.Deletion, $"{User.Identity.Name} deleted discount {discount.Title}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Discount deleted successfully";
            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleApply(int id, int[] tripIDs, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            var discount = await _discountRepository.Queryable
                .Include(d => d.TripDiscounts)
                .ThenInclude(td => td.Trip)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (discount == null)
            {
                return NotFound();
            }

            _discountRepository.UpdateTripApplications(discount, tripIDs);
            await _eTourLogger.LogAsync(Log.LogType.Deletion, $"{User.Identity.Name} removed discount {discount.Title} from some of the applied trips");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Applied trip list updated successfully";
            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> AppliedTrips(int id, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            var discount = await _discountRepository.Queryable
                .Include(d => d.TripDiscounts)
                .ThenInclude(td => td.Trip)
                .ThenInclude(tr => tr.Tour)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (discount == null)
            {
                return NotFound();
            }

            return View(new AppliedTripsModel { 
                ReturnUrl = returnUrl,
                Discount = discount
            });
        }
    }
}
