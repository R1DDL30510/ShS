using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace SecureHomeSystem.Configuration;

/// <summary>
/// Validates <see cref="DockerOptions" /> beyond what data annotations cover. The validator
/// ensures the Compose files are referenced and that label selectors contain usable values.
/// </summary>
public sealed class DockerOptionsValidator : IValidateOptions<DockerOptions>
{
    public ValidateOptionsResult Validate(string? name, DockerOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ComposeFilePath))
        {
            failures.Add("Docker:ComposeFilePath must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(options.EnvironmentFile))
        {
            failures.Add("Docker:EnvironmentFile must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(options.ProjectName))
        {
            failures.Add("Docker:ProjectName must not be empty.");
        }

        if (options.AutoStartProfiles is { Length: > 0 })
        {
            for (var i = 0; i < options.AutoStartProfiles.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(options.AutoStartProfiles[i]))
                {
                    failures.Add($"Docker:AutoStartProfiles[{i}] must not be empty.");
                    break;
                }
            }
        }

        if (options.Detection is null)
        {
            failures.Add("Docker:Detection must be configured.");
        }
        else if (options.Detection.LabelSelector is null || options.Detection.LabelSelector.Count == 0)
        {
            failures.Add("Docker:Detection:LabelSelector must contain at least one label mapping.");
        }
        else
        {
            foreach (var (key, value) in options.Detection.LabelSelector)
            {
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                {
                    failures.Add("Docker:Detection:LabelSelector entries must include non-empty keys and values.");
                    break;
                }
            }

            AppendFailures(options.Detection, "Docker:Detection", failures);
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
