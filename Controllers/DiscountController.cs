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
        private readonly IUnitOfWork _unitOfWork;

        public DiscountController(IDiscountRepository discountRepository, ITripDiscountRepository tripDiscountRepository, IUnitOfWork unitOfWork)
        {
            _discountRepository = discountRepository;
            _tripDiscountRepository = tripDiscountRepository;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(Discount discount)
        {
            var discountErrors = discount.GetValidationErrors();

            if (!ModelState.IsValid || discountErrors.Any())
            {
                AddCustomModelErrors(discountErrors);
                return View();
            }

            await _discountRepository.AddAsync(discount);
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Discount created successfully";
            return RedirectToAction("Index");
        }

        private void AddCustomModelErrors(IReadOnlyDictionary<string, List<string>> discountErrors)
        {
            foreach (var item in discountErrors)
            {
                foreach (var error in item.Value)
                {
                    ModelState.AddModelError(item.Key, error);
                }
            }
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Discount discount)
        {
            var discountErrors = discount.GetValidationErrors();

            if (!ModelState.IsValid || discountErrors.Any())
            {
                AddCustomModelErrors(discountErrors);
                return View(discount);
            }

            await _discountRepository.UpdateAsync(discount);
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Discount updated successfully";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Discount deleted successfully";
            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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


            foreach (var tripDisc in discount.TripDiscounts.Where(td => !tripIDs.Contains(td.Trip.ID)))
            {
                await _tripDiscountRepository.DeleteAsync(tripDisc);
            }

            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Applied trip list updated successfully";
            return RedirectToAction("Index");
        }


    }
}
