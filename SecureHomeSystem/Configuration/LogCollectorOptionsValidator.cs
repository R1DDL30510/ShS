using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace SecureHomeSystem.Configuration;

/// <summary>
/// Applies recursive data-annotation validation to <see cref="LogCollectorOptions" />.
/// </summary>
public sealed class LogCollectorOptionsValidator : IValidateOptions<LogCollectorOptions>
{
    public ValidateOptionsResult Validate(string? name, LogCollectorOptions options)
    {
        var failures = new List<string>();

        AppendFailures(options, "LogCollector", failures);

        if (options.Rotation is not null)
        {
            AppendFailures(options.Rotation, "LogCollector:Rotation", failures);
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
