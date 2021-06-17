using Core.Entities;
using System.Collections.Generic;

namespace Company.Models
{
    public class ValidUpdateStatusesModel
    {
        public string ReturnUrl { get; set; }
        public int BookingID { get; set; }
        public IEnumerable<Booking.BookingStatus> BookingStatuses { get; set; }
    }
}
