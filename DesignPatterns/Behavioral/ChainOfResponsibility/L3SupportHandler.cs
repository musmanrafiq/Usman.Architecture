namespace Behavioral.ChainOfResponsibility;

public class L3SupportHandler : SupportHandler
{
    public override void Handle(SupportTicket ticket)
        => Console.WriteLine($"[L3 Engineering] Final resolution for ticket {ticket.Id}: {ticket.Description}");
}
