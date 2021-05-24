using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class TourListModel
    {
        public IEnumerable<TourListItem> Tours { get; set; }
        public bool ShowClosed { get; set; }
    }
}
