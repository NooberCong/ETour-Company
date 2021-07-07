using Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class TripListModel
    {
        public IEnumerable<Trip> Trips { get; set; }
        public IEnumerable<Tour> Tours { get; set; }
        public bool ShowClosed { get; set; }
        public bool ShowOwned { get; set; }
    }
}
