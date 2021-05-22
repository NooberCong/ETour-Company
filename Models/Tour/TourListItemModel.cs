using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class TourListItemModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Start { get; set; }
        public string Destination { get; set; }
        public string Type { get; set; }

        public static TourListItemModel FromTourEntity(Tour tour)
        {
            return new TourListItemModel
            {
                ID = tour.ID,
                Title = tour.Title,
                Start = tour.StartPlace,
                Destination = tour.Destination,
                Type = tour.Type.ToString()
            };
        }
    }
}
