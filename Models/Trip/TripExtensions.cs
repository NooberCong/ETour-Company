using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public static class TripExtensions
    {
        public static IReadOnlyDictionary<string, List<string>> GetValidationErrors(this Trip trip)
        {
            Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

            if (trip.StartTime < DateTime.Now || trip.StartTime >= trip.EndTime)
            {
                if (errors.GetValueOrDefault("") == null)
                {
                    errors[""] = new List<string>();
                }
                errors[""].Add("Invalid time frame, Start time must be a day in the future and is before end time");
            }
            
            return errors;
        }
    }
}
