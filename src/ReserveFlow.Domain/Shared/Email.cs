using System.Text.RegularExpressions;
using ReserveFlow.Domain.Abstractions;

namespace ReserveFlow.Domain.Shared;

public sealed partial class Email : ValueObject
{
    private static readonly Regex EmailRegex = MyRegex();

    private Email(string value)
    {
        Value = value;
    }

    // EF Core materialization
    private Email()
    {
        Value = null!;
    }

    public string Value { get; private set; }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email is required.", nameof(value));
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 256 || !EmailRegex.IsMatch(normalized))
        {
            throw new ArgumentException("Email format is invalid.", nameof(value));
        }

        return new Email(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
