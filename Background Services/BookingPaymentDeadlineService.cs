using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Company.Background_Services
{
    public class BookingPaymentDeadlineService : BackgroundService
    {
        private IBookingRepository _bookingRepository;
        private ICustomerRepository _customerRepository;
        private IUnitOfWork _unitOfWork;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public BookingPaymentDeadlineService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    _bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                    _customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
                    _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var bookingsAwaitingPayment = _bookingRepository.Queryable.Include(bk => bk.Trip).Where(bk => bk.Status == Booking.BookingStatus.Awaiting_Deposit || bk.Status == Booking.BookingStatus.Awaiting_Payment);

                    if (bookingsAwaitingPayment.Any())
                    {

                        foreach (var booking in bookingsAwaitingPayment)
                        {
                            if (booking.PaymentDeadline <= DateTime.Now)
                            {
                                booking.Cancel(DateTime.Now);
                                var customer = await _customerRepository.FindAsync(booking.OwnerID);
                                booking.RefundPoints(customer);

                                foreach (var pointLog in booking.PointLogs)
                                {
                                    _customerRepository.AddPointLog(pointLog);
                                }

                                await _bookingRepository.UpdateAsync(booking);

                                Console.WriteLine($"Canceled booking No.{booking.ID} due to expired payment deadline");
                            }
                        }

                        await _unitOfWork.CommitAsync();
                    }
                }

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
