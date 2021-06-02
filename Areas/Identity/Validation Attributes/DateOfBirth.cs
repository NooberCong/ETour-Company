using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Areas.Identity.Validation_Attributes
{
    public class DateOfBirth : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime dateInput = (DateTime)value;

            return DateTime.Now.AddYears(-100) < dateInput && dateInput < DateTime.Now.AddYears(-18);
        }
    }
}
