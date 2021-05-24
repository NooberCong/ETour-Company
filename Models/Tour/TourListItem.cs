using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class TourListItem
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Start { get; set; }
        public string Destination { get; set; }
        public string Type { get; set; }
        public bool IsOpen { get; set; }

        public static TourListItem FromTourEntity(Tour tour)
        {
            return new TourListItem
            {
                ID = tour.ID,
                Title = tour.Title,
                Start = tour.StartPlace,
                Destination = tour.Destination,
                Type = tour.Type.ToString(),
                IsOpen = tour.IsOpen
            };
        }
    }
}
