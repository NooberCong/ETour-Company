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
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BookingController(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(Booking.BookingStatus? status)
        {
            var bookings = await _bookingRepository.Queryable
                .Where(bk => status == null || bk.Status == status)
                .Include(bk => bk.Author).Include(bk => bk.Trip)
                .ThenInclude(tr => tr.Tour).ToListAsync();

            return View(new BookingListModel
            {
                Status = status,
                Bookings = bookings
            });
        }
    }
}
