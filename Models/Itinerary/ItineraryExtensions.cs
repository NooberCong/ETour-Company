using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public static class ItineraryExtensions
    {
        public static IReadOnlyDictionary<string, List<string>> GetValidationErrors(this Itinerary itinerary, Trip trip)
        {
            Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

            if (itinerary.StartTime < trip.StartTime || itinerary.StartTime > trip.EndTime)
            {
                if (errors.GetValueOrDefault($"{nameof(Itinerary)}.{nameof(itinerary.StartTime)}") == null)
                {
                    errors[$"{nameof(Itinerary)}.{nameof(itinerary.StartTime)}"] = new List<string>();
                }
                errors[$"{nameof(Itinerary)}.{nameof(itinerary.StartTime)}"].Add("Invalid Start time, must be within the start time and end time of the owning trip");
            }
            return errors;
        }
    }
}
