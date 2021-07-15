using Company.Models;
using Core.Entities;
using Core.Helpers;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Company.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LogController : Controller
    {
        private static readonly int _pageSize = 20;
        private readonly ILogRepository _logRepository;

        public LogController(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public IActionResult Index(Log.LogType? type, int pageNumber = 1, bool logSync = true)
        {
            var logs = _logRepository.Queryable.Where(log => type == null || log.Type == type).OrderByDescending(log => log.LastUpdated);

            return View(new LogListModel
            {
                Logs = PaginatedList<Log>.Create(logs, pageNumber, _pageSize),
                Type = type,
                LogSync = logSync,
            });
        }
    }
}
