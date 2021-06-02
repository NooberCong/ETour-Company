using Infrastructure.InterfaceImpls;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class BlogFormModel
    {
        public Post Post { get; set; }
        public IFormFileCollection Images { get; set; }
    }
}
