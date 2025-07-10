using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KorRaporOnline.API.Extensions
{
    public static class ValidationExtensions
    {
        public static bool TryValidate<T>(this T model, out ICollection<ValidationResult> results)
        {
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, new ValidationContext(model), results, true);
        }
    }
}