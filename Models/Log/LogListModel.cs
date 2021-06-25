using Core.Entities;
using Core.Helpers;

namespace Company.Models
{
    public class LogListModel
    {
        public PaginatedList<Log> Logs { get; set; }
        public Log.LogType? Type { get; set; }
        public bool LogSync { get; set; }
    }
}
