namespace Fidelity.Application.Common.Interfaces;

/// <summary>
/// Email service interface - ISO 25000: Maintainability (decoupling)
/// </summary>
public interface IEmailService
{
    Task<(bool Success, string? Error)> InviaEmailVerificaAsync(
        string email,
        string nome,
        string token,
        string linkRegistrazione,
        string puntoVenditaNome);

    Task<bool> InviaEmailBenvenutoAsync(
        string email,
        string nome,
        string codiceFidelity,
        byte[]? cardDigitale = null);

    Task<bool> InviaEmailNuovoCouponAsync(
        string email,
        string nome,
        string titoloCoupon,
        string codiceCoupon,
        DateTime dataScadenza);

    Task<bool> InviaEmailPuntiAssegnatiAsync(
        string email,
        string nome,
        int puntiAssegnati,
        int puntiTotali,
        decimal importoSpesa);

    Task<bool> InviaEmailLivelloRaggiuntoAsync(
        string email,
        string nome,
        string nuovoLivello);

    Task<bool> InviaEmailResetPasswordAsync(
        string email,
        string nome,
        string resetToken,
        string resetLink);
}
