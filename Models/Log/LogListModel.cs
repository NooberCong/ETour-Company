using Core.Entities;
using System.Collections.Generic;

namespace Company.Models
{
    public class LogListModel
    {
        public IEnumerable<Log> Logs { get; set; }
        public Log.LogType? Type { get; set; }
        public int PageNumber { get; set; }
        public int PageCount { get; set; }
        public bool LogSync { get; set; }
    }
}
