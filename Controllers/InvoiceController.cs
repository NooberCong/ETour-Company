using Company.Models;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public InvoiceController(IInvoiceRepository invoiceRepository, IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
        {
            _invoiceRepository = invoiceRepository;
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var invoices = _invoiceRepository.Queryable
                .Include(inv => inv.Booking).ThenInclude(bk => bk.Owner).AsEnumerable();

            return View(new InvoiceListModel
            {
                Invoices = invoices
            });
        }

        public async Task<IActionResult> New(int bookingID)
        {
            var booking = await _bookingRepository.FindAsync(bookingID);

            if (booking == null)
            {
                return NotFound();
            }

            return View(new InvoiceFormModel
            {
                BookingID = bookingID
            });
        }

        [HttpPost]
        public async Task<IActionResult> New(Invoice invoice, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            if (_bookingRepository.FindAsync(invoice.BookingID) == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(new InvoiceFormModel
                {
                    BookingID = invoice.BookingID,
                    Invoice = invoice
                });
            }

            invoice.LastUpdated = DateTime.Now;

            await _invoiceRepository.AddAsync(invoice);
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Invoice Added Successfuly";
            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var invoice = await _invoiceRepository.FindAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(new InvoiceFormModel
            {
                BookingID = invoice.BookingID,
                Invoice = invoice
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Invoice invoice, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            var existingInvoice = await _invoiceRepository.FindAsync(invoice.ID);

            if (existingInvoice == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(new InvoiceFormModel
                {
                    BookingID = invoice.BookingID,
                    Invoice = invoice
                });
            }

            invoice.LastUpdated = DateTime.Now;

            await _invoiceRepository.UpdateAsync(invoice);
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Invoice Updated Successfully";
            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            var invoice = await _invoiceRepository.FindAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            await _invoiceRepository.DeleteAsync(invoice);
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Invoice Deleted Successfully";
            return LocalRedirect(returnUrl);
        }


        public async Task<IActionResult> Detail(int id)
        {
            var invoice = await _invoiceRepository.FindAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            invoice.Booking = await _bookingRepository.Queryable
                .Include(bk => bk.Owner)
                .Include(bk => bk.Trip)
                .ThenInclude(tr => tr.Tour)
                .FirstOrDefaultAsync(bk => bk.ID == invoice.BookingID);

            return View(invoice);
        }
    }
}
