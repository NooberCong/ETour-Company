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
    public class UnbookableTripClosingService : BackgroundService
    {
        private ITripRepository _tripRepository;
        private IUnitOfWork _unitOfWork;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UnbookableTripClosingService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    _tripRepository = scope.ServiceProvider.GetRequiredService<ITripRepository>();
                    _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var unbookables = _tripRepository.Queryable
                        .Include(tr => tr.Bookings)
                        .AsEnumerable()
                        .Where(tr => tr.CanBook(DateTime.Now));

                    foreach (var trip in unbookables)
                    {
                        trip.Close();
                        await _tripRepository.UpdateAsync(trip);
                    }
                    await _unitOfWork.CommitAsync();
                }
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
