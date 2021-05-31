using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Company.Models
{
    public static class ModelStateExtensions
    {
        public static void AddModelErrors(this ModelStateDictionary modelState, IReadOnlyDictionary<string, List<string>> errors)
        {
            foreach (var item in errors)
            {
                foreach (var error in item.Value)
                {
                    modelState.AddModelError(item.Key, error);
                }
            }
        }
    }
}
