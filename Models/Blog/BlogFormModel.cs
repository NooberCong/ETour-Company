using Core.Validation_Attributes;
using Infrastructure.InterfaceImpls;
using Microsoft.AspNetCore.Http;

namespace Company.Models
{
    public class BlogFormModel
    {
        public Post Post { get; set; }
        public IFormFile CoverImg { get; set; }
        public string CommaSeparatedTags { get; set; }
    }
}
