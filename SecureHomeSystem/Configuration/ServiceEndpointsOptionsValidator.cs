using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace SecureHomeSystem.Configuration;

/// <summary>
/// Enforces data annotation validation for the <see cref="ServiceEndpointsOptions" /> object graph.
/// </summary>
public sealed class ServiceEndpointsOptionsValidator : IValidateOptions<ServiceEndpointsOptions>
{
    public ValidateOptionsResult Validate(string? name, ServiceEndpointsOptions options)
    {
        var failures = new List<string>();

        AppendFailures(options, "Services", failures);

        if (options.Ollama is not null)
        {
            AppendFailures(options.Ollama, "Services:Ollama", failures);
        }

        if (options.OpenWebUi is not null)
        {
            AppendFailures(options.OpenWebUi, "Services:OpenWebUi", failures);
        }

        if (options.StableDiffusion is not null)
        {
            AppendFailures(options.StableDiffusion, "Services:StableDiffusion", failures);
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private static void AppendFailures(object instance, string prefix, ICollection<string> failures)
    {
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true))
        {
            foreach (var result in results)
            {
                var message = string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? "Validation failed."
                    : result.ErrorMessage;
                failures.Add($"{prefix}: {message}");
            }
        }
    }
}
