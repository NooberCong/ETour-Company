using Client.Models;
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
    public class DiscountController : Controller
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly ITripDiscountRepository _tripDiscountRepository;
        private readonly IETourLogger _eTourLogger;
        private readonly IUnitOfWork _unitOfWork;

        public DiscountController(IDiscountRepository discountRepository, ITripDiscountRepository tripDiscountRepository, IETourLogger eTourLogger, IUnitOfWork unitOfWork)
        {
            _discountRepository = discountRepository;
            _tripDiscountRepository = tripDiscountRepository;
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
        public async Task<IActionResult> New(Discount discount)
        {
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
            return RedirectToAction("Index");
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
        public async Task<IActionResult> Edit(Discount discount)
        {
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
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, string returnUrl)
        {
            Discount discount = await _discountRepository.Queryable
                .Include(d => d.TripDiscounts)
                .FirstOrDefaultAsync(d => d.ID == id);


            if (discount == null)
            {
                return NotFound();
            }

            discount.TripDiscounts.Clear();

            await _discountRepository.UpdateAsync(discount);
            await _discountRepository.DeleteAsync(discount);
            await _eTourLogger.LogAsync(Log.LogType.Deletion, $"{User.Identity.Name} deleted discount {discount.Title}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Discount deleted successfully";
            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleApply(int id, int[] tripIDs)
        {

            Discount discount = await _discountRepository.Queryable
                .Include(d => d.TripDiscounts)
                .ThenInclude(td => td.Trip)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (discount == null)
            {
                return NotFound();
            }

            var tripDiscountsToDelete = discount.TripDiscounts.Where(td => !tripIDs.Contains(td.Trip.ID));

            foreach (var tripDisc in tripDiscountsToDelete)
            {
                await _tripDiscountRepository.DeleteAsync(tripDisc);
            }

            await _eTourLogger.LogAsync(Log.LogType.Deletion, $"{User.Identity.Name} removed discount {discount.Title} from trips #[{string.Join(", ", tripDiscountsToDelete.Select(trd => Convert.ToString(trd.TripID)).ToArray())}]");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Applied trip list updated successfully";
            return RedirectToAction("Index");
        }


    }
}
