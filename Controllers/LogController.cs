using Company.Models;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Company.Controllers
{
    public class LogController : Controller
    {
        private static int _pageSize = 20;
        private readonly ILogRepository _logRepository;

        public LogController(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public IActionResult Index(Log.LogType? type, int pageNumber = 1, bool logSync = true)
        {
            var logs = _logRepository.QueryFilteredPaged(log => type == null || log.Type == type, pageNumber, _pageSize);
            return View(new LogListModel
            {
                Logs = logs,
                Type = type,
                PageNumber = pageNumber,
                PageCount = _logRepository.PageCount(log => type == null || log.Type == type, _pageSize),
                LogSync = logSync,
            });
        }
    }
}
