using Core.Entities;
using System;
using System.Collections.Generic;

namespace Client.Models
{
    public static class DiscountExtensions
    {
        public static IReadOnlyDictionary<string, List<string>> GetValidationErrors(this Discount discount)
        {
            Dictionary<string, List<string>> errors = new();

            if (discount.Type == Discount.DiscountType.Percentage && (discount.Value < 0 || discount.Value > 100))
            {
                if (errors.GetValueOrDefault(nameof(discount.Value)) == null)
                {
                    errors[nameof(discount.Value)] = new List<string>();
                }
                errors[nameof(discount.Value)].Add("Invalid percentage value, must be from 0 to 100");
            }

            if (discount.ValidFrom >= discount.ValidUntil)
            {
                if (errors.GetValueOrDefault(nameof(discount.ValidFrom)) == null)
                {
                    errors[nameof(discount.ValidFrom)] = new List<string>();
                }
                errors[nameof(discount.ValidFrom)].Add("Valid From cannot be after Valid Until");
            }

            if (discount.ValidUntil <= DateTime.Now)
            {
                if (errors.GetValueOrDefault(nameof(discount.ValidUntil)) == null)
                {
                    errors[nameof(discount.ValidUntil)] = new List<string>();
                }
                errors[nameof(discount.ValidUntil)].Add("Valid until cannot be a time in the past");
            }
            return errors;
        }
    }
}
