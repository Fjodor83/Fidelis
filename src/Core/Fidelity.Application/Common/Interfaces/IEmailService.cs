namespace Fidelity.Application.Common.Interfaces;

public interface IEmailService
{
    Task<(bool Success, string ErrorMessage)> InviaEmailNuovoCouponAsync(
        string email, 
        string nome, 
        string titoloCoupon, 
        string codiceCoupon, 
        DateTime dataScadenza);
}
