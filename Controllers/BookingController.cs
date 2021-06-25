using Company.Models;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Controllers
{
    public class BookingController : Controller
    {
        private readonly ITripRepository _tripRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BookingController(IBookingRepository bookingRepository, ITripRepository tripRepository, IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _tripRepository = tripRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(Booking.BookingStatus? status)
        {
            var bookings = await _bookingRepository.Queryable
                .Where(bk => status == null || bk.Status == status)
                .Include(bk => bk.Owner).Include(bk => bk.Trip)
                .ThenInclude(tr => tr.Tour).ToListAsync();

            return View(new BookingListModel
            {
                Status = status,
                Bookings = bookings
            });
        }

        public async Task<IActionResult> Detail(int id)
        {
            var booking = await _bookingRepository.Queryable
                .Include(bk => bk.CustomerInfos)
                .FirstOrDefaultAsync(bk => bk.ID == id);

            if (booking == null)
            {
                return NotFound();
            }

            booking.Trip = await _tripRepository.Queryable
                .Include(tr => tr.Tour)
                .Include(tr => tr.TripDiscounts)
                .ThenInclude(td => td.Discount)
                .Include(tr => tr.Bookings)
                .ThenInclude(bk => bk.CustomerInfos)
                .FirstOrDefaultAsync(tr => tr.ID == booking.TripID);

            return View(booking);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var booking = await _bookingRepository.FindAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, Booking.BookingStatus status, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            var existingBooking = await _bookingRepository.Queryable
                .Include(bk => bk.Trip)
                .FirstOrDefaultAsync(bk => bk.ID == id);

            if (existingBooking == null)
            {
                return NotFound();
            }

            existingBooking.ChangeStatus(status);

            await _bookingRepository.UpdateAsync(existingBooking);
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Booking status updated successfully";

            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> ValidUpdateStatuses(int id, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            var booking = await _bookingRepository.FindAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(new ValidUpdateStatusesModel { 
                BookingID = booking.ID,
                BookingStatuses = booking.GetPossibleNextStatuses(),
                ReturnUrl = returnUrl
            });
        }

        public async Task<IActionResult> ShortDetail(int id) {
            var booking = await _bookingRepository.Queryable
                .Include(bk => bk.Owner)
                .Include(bk => bk.Trip)
                .ThenInclude(tr => tr.Tour)
                .FirstOrDefaultAsync(bk => bk.ID == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }
    }
}
