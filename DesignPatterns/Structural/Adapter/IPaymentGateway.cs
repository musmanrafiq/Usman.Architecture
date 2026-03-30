namespace Structural.Adapter;

public interface IPaymentGateway
{
    bool Charge(string customerId, decimal amount, string currency);
}
