using System.Text.RegularExpressions;
using Fidelity.Domain.Common;
using Fidelity.Domain.Exceptions;

namespace Fidelity.Domain.ValueObjects;

/// <summary>
/// Email value object with validation - ISO 25000 Functional Suitability
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value.ToLowerInvariant().Trim();
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("L'email non può essere vuota");

        email = email.Trim();

        if (email.Length > 255)
            throw new ValidationException("L'email non può superare i 255 caratteri");

        if (!EmailRegex.IsMatch(email))
            throw new ValidationException("Formato email non valido");

        return new Email(email);
    }

    public static Email? CreateOrNull(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return Create(email);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
