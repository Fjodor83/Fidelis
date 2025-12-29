using System.Security.Cryptography;
using Fidelity.Domain.Common;
using Fidelity.Domain.Exceptions;

namespace Fidelity.Domain.ValueObjects;

/// <summary>
/// Codice Fidelity value object - ISO 25000 Security (cryptographic generation)
/// </summary>
public sealed class CodiceFidelity : ValueObject
{
    private const string Prefix = "SUN";
    private const int CodeLength = 12; // SUN + 9 digits

    public string Value { get; }

    private CodiceFidelity(string value)
    {
        Value = value.ToUpperInvariant();
    }

    /// <summary>
    /// Creates a new unique fidelity code using cryptographically secure random
    /// </summary>
    public static CodiceFidelity Generate()
    {
        // Use cryptographically secure random number generator
        var bytes = new byte[4];
        RandomNumberGenerator.Fill(bytes);
        var number = Math.Abs(BitConverter.ToInt32(bytes, 0) % 900000000) + 100000000;

        return new CodiceFidelity($"{Prefix}{number}");
    }

    /// <summary>
    /// Creates from existing code (validation)
    /// </summary>
    public static CodiceFidelity Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ValidationException("Il codice fidelity non può essere vuoto");

        code = code.ToUpperInvariant().Trim();

        if (code.Length != CodeLength)
            throw new ValidationException($"Il codice fidelity deve essere di {CodeLength} caratteri");

        if (!code.StartsWith(Prefix))
            throw new ValidationException($"Il codice fidelity deve iniziare con {Prefix}");

        var numericPart = code.Substring(Prefix.Length);
        if (!numericPart.All(char.IsDigit))
            throw new ValidationException("Il codice fidelity deve contenere solo numeri dopo il prefisso");

        return new CodiceFidelity(code);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(CodiceFidelity code) => code.Value;
}
