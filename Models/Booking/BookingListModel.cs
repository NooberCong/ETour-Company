using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class BookingListModel
    {
        public Booking.BookingStatus? Status { get; set; }
        public IEnumerable<Booking> Bookings { get; set; }
    }
}
