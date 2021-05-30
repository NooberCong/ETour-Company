using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class TourListModel
    {
        public IEnumerable<Tour> Tours { get; set; }
        public bool ShowClosed { get; set; }
    }
}
