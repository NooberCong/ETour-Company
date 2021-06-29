using ChartJSCore.Models;
using Core.Entities;

namespace Company.Models
{
    public class AnalyticModel
    {
        public Chart RevenueChart { get; set; }
        public Chart TicketSalesChart { get; set; }
        public Chart BookingStatusSegmentChart { get; set; }
        public Tour[] TopBookedTours { get; set; }
    }
}
