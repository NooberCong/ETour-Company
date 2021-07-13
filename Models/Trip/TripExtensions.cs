using Core.Entities;
using System;
using System.Collections.Generic;

namespace Company.Models
{
    public static class TripExtensions
    {
        public static IReadOnlyDictionary<string, List<string>> GetValidationErrors(this Trip trip)
        {
            Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

            if (trip.EndTime <= trip.StartTime)
            {
                if (errors.GetValueOrDefault("Trip.EndTime") == null)
                {
                    errors["Trip.EndTime"] = new List<string>();
                }
                errors["Trip.EndTime"].Add("End time cannot be before start time");
            }

            if (trip.StartTime < DateTime.Now.AddDays(31))
            {
                if (errors.GetValueOrDefault("Trip.StartTime") == null)
                {
                    errors["Trip.StartTime"] = new List<string>();
                }
                errors["Trip.StartTime"].Add("Start time must be at least 31 days from now");
            }
            return errors;
        }
    }
}
