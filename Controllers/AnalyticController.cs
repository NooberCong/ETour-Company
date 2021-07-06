using ChartJSCore.Helpers;
using ChartJSCore.Models;
using Company.Models;
using Core.Entities;
using Core.Interfaces;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Company.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AnalyticController : Controller
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly AnalyticsService _analyticsService;

        public AnalyticController(IBookingRepository bookingRepository, AnalyticsService analyticsService)
        {
            _bookingRepository = bookingRepository;
            _analyticsService = analyticsService;
        }

        public IActionResult Index()
        {
            Chart revenueChart = CreateRevenueChart();
            Chart ticketSalesChart = CreateTicketSalesChart();
            Chart bookingStatusSegmentChart = CreateBookingStatusSegmentationChart();
            Tour[] tours = _analyticsService.TopBookedTour(5, _bookingRepository.Queryable.Include(bk => bk.Trip).ThenInclude(tr => tr.Tour), DateTime.Now.AddDays(-30), DateTime.Now);

            return View(new AnalyticModel
            {
                RevenueChart = revenueChart,
                TicketSalesChart = ticketSalesChart,
                TopBookedTours = tours,
                BookingStatusSegmentChart = bookingStatusSegmentChart
            });
        }

        private Chart CreateBookingStatusSegmentationChart()
        {
            var seqments = _analyticsService.BookingStatusSegmentation(_bookingRepository.Queryable);
            var chart = new Chart();
            chart.Type = Enums.ChartType.Pie;
            chart.Options = new Options
            {
                Title = new Title { Text = "Booking status breakdown" },
            };

            Data data = new();
            data.Labels = Enum.GetNames(typeof(Booking.BookingStatus)).ToList();

            PieDataset pieDataset = new PieDataset
            {
                Data = seqments.Segments.Select(seg => (double?)seg).ToList(),
                BackgroundColor = Enum.GetValues<Booking.BookingStatus>().Select(status => BookingStatusToChartColor(status)).ToList()
            };

            data.Datasets = new List<Dataset>();
            data.Datasets.Add(pieDataset);
            chart.Data = data;

            return chart;
        }

        private ChartColor BookingStatusToChartColor(Booking.BookingStatus status)
        {
            return status switch
            {
                Booking.BookingStatus.Awaiting_Deposit => ChartColor.FromRgba(178, 134, 0, .75),
                Booking.BookingStatus.Processing => ChartColor.FromRgba(0, 45, 156, .5),
                Booking.BookingStatus.Awaiting_Payment => ChartColor.FromRgba(159, 24, 83, .75),
                Booking.BookingStatus.Completed => ChartColor.FromRgba(25, 128, 56, .75),
                Booking.BookingStatus.Canceled => ChartColor.FromRgba(87, 4, 8, .75),
                _ => throw new NotImplementedException(),
            };
        }

        private Chart CreateTicketSalesChart()
        {
            var crossYearTicketSales = _analyticsService.CrossYearMonthlyTicketSales(_bookingRepository.Queryable, DateTime.Now);

            Chart chart = new Chart();

            chart.Type = Enums.ChartType.Line;
            chart.Options = new Options
            {
                Title = new Title { Text = "Monthly Ticket Sales" },
            };

            Data data = new();

            data.Labels = crossYearTicketSales.Months;

            LineDataset lastYearDataset = new LineDataset()
            {
                Label = "Last year",
                Data = crossYearTicketSales.LastYearTicketSales.Select(rev => (double?)rev).ToList(),
                BackgroundColor = ChartColor.FromRgba(75, 192, 192, 0.4),
                BorderColor = ChartColor.FromRgb(75, 192, 192),
                Fill = "true",
                LineTension = 0.1,
                BorderCapStyle = "butt",
                BorderDash = new List<int> { },
                BorderDashOffset = 0.0,
                BorderJoinStyle = "miter",
                PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
                PointBorderWidth = new List<int> { 1 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
                PointHoverBorderWidth = new List<int> { 2 },
                PointRadius = new List<int> { 1 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };

            LineDataset thisYearDataSet = new LineDataset()
            {
                Label = "This year",
                Data = crossYearTicketSales.ThisYearTicketSales.Select(rev => (double?)rev).ToList(),
                BackgroundColor = ChartColor.FromRgba(197, 17, 78, 0.4),
                BorderColor = ChartColor.FromRgb(197, 17, 78),
                Fill = "true",
                LineTension = 0.1,
                BorderCapStyle = "butt",
                BorderDash = new List<int> { },
                BorderDashOffset = 0.0,
                BorderJoinStyle = "miter",
                PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(197, 17, 78) },
                PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
                PointBorderWidth = new List<int> { 1 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(197, 17, 78) },
                PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
                PointHoverBorderWidth = new List<int> { 2 },
                PointRadius = new List<int> { 1 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };

            data.Datasets = new List<Dataset>();
            data.Datasets.Add(lastYearDataset);
            data.Datasets.Add(thisYearDataSet);
            chart.Data = data;
            return chart;
        }

        private Chart CreateRevenueChart()
        {
            var crossYearRevenues = _analyticsService.CrossYearMonthlyRevenue(_bookingRepository.Queryable, DateTime.Now);

            Chart chart = new Chart();

            chart.Type = Enums.ChartType.Bar;
            chart.Options = new Options
            {
                Title = new Title { Text = "Monthly Revenues" },
            };

            Data data = new();

            data.Labels = crossYearRevenues.Months;

            BarDataset lastYearDataset = new BarDataset()
            {
                Label = "Last year",
                Data = crossYearRevenues.LastYearRevenues.Select(rev => (double?)rev).ToList(),
                BackgroundColor = new List<ChartColor> { ChartColor.FromRgba(75, 192, 192, 0.4) },
                BorderColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
            };

            BarDataset thisYearDataSet = new BarDataset()
            {
                Label = "This year",
                Data = crossYearRevenues.ThisYearRevenues.Select(rev => (double?)rev).ToList(),
                BackgroundColor = new List<ChartColor> { ChartColor.FromRgba(197, 17, 78, 0.4) },
                BorderColor = new List<ChartColor> { ChartColor.FromRgb(197, 17, 78) },
            };

            data.Datasets = new List<Dataset>();
            data.Datasets.Add(lastYearDataset);
            data.Datasets.Add(thisYearDataSet);
            chart.Data = data;
            return chart;
        }
    }
}
