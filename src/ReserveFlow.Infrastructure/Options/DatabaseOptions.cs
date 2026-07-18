using System.ComponentModel.DataAnnotations;

namespace ReserveFlow.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    [MinLength(1)]
    public string ConnectionString { get; init; } = string.Empty;
}
