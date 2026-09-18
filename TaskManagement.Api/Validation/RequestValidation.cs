using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.Validation;

/// <summary>
/// Validates a request DTO using the DataAnnotations already declared on it
/// (for example [Required] on RegisterRequest).
///
/// This is generic on purpose: the rule and its message live on the DTO, so
/// adding a new required field needs no new helper method — just an attribute
/// on the DTO.
/// </summary>
public static class RequestValidation
{
    public static void EnsureValid(object request)
    {
        var results = new List<ValidationResult>();

        // Handles attributes placed on properties and IValidatableObject.
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);

        // Positional records put attributes such as [Required] on the constructor
        // parameters, not on the generated properties, so validate those as well.
        ValidateConstructorParameters(request, results);

        if (results.Count > 0)
        {
            throw new ArgumentException(results[0].ErrorMessage);
        }
    }

    private static void ValidateConstructorParameters(object request, List<ValidationResult> results)
    {
        var constructor = request.GetType().GetConstructors()
            .OrderByDescending(candidate => candidate.GetParameters().Length)
            .FirstOrDefault();

        if (constructor is null)
        {
            return;
        }

        foreach (var parameter in constructor.GetParameters())
        {
            var value = parameter.Name is null
                ? null
                : request.GetType().GetProperty(parameter.Name)?.GetValue(request);

            foreach (var attribute in parameter.GetCustomAttributes(true).OfType<ValidationAttribute>())
            {
                var context = new ValidationContext(request) { MemberName = parameter.Name };
                var result = attribute.GetValidationResult(value, context);

                if (result is not null && result != ValidationResult.Success)
                {
                    results.Add(result);
                }
            }
        }
    }
}
