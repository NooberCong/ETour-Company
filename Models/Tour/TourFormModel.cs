using Core.Entities;
using Core.Validation_Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Company.Models
{
    public class TourFormModel
    {
        public Tour Tour { get; set; }
        public IFormFileCollection Images { get; set; }
    }
}
