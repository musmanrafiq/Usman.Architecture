namespace Behavioral.ChainOfResponsibility;

public class L1SupportHandler : SupportHandler
{
    public override void Handle(SupportTicket ticket)
    {
        if (ticket.Level == SupportLevel.L1)
            Console.WriteLine($"[L1 Support] Resolved ticket {ticket.Id}: {ticket.Description}");
        else
        {
            Console.WriteLine($"[L1 Support] Escalating ticket {ticket.Id} to L2...");
            base.Handle(ticket);
        }
    }
}
