using Core.Entities;
using System.Collections.Generic;

namespace Company.Models
{
    public class TripDetailModel
    {
        public Trip Trip { get; set; }
        public IEnumerable<Trip> ItineraryImportTrips { get; set; }
    }
}
