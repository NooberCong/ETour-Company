using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class ItineraryFormModel
    {
        public Trip Trip { get; set; }
        public Itinerary Itinerary { get; set; }
    }
}
