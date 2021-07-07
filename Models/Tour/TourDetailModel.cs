using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class TourDetailModel
    {
        public Tour Tour { get; set; }
        public IEnumerable<TourReview> Reviews { get; set; }
    }
}
