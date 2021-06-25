using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Views.Shared
{
    public static class ManageNavPages
    {
        public static string Analytic => "Analytic";

        public static string Log => "Log";

        public static string Account => "Account";

        public static string Tour => "Tour";

        public static string Trip => "Trip";

        public static string Discount => "Discount";

        public static string Booking => "Booking";
        public static string Invoice => "Invoice";

        public static string Blog => "Blog";

        public static string Question => "Question";

        public static string AnalyticNavClass(ViewContext viewContext) => PageNavClass(viewContext, Analytic);

        public static string LogNavClass(ViewContext viewContext) => PageNavClass(viewContext, Log);

        public static string AccountNavClass(ViewContext viewContext) => PageNavClass(viewContext, Account);

        public static string TourNavClass(ViewContext viewContext) => PageNavClass(viewContext, Tour);

        public static string TripNavClass(ViewContext viewContext) => PageNavClass(viewContext, Trip);

        public static string DiscountNavClass(ViewContext viewContext) => PageNavClass(viewContext, Discount);

        public static string BookingNavClass(ViewContext viewContext) => PageNavClass(viewContext, Booking);
        public static string InvoiceNavClass(ViewContext viewContext) => PageNavClass(viewContext, Invoice);

        public static string BlogNavClass(ViewContext viewContext) => PageNavClass(viewContext, Blog);

        public static string QuestionNavClass(ViewContext viewContext) => PageNavClass(viewContext, Question);

        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.RouteData.Values["controller"]?.ToString();
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
