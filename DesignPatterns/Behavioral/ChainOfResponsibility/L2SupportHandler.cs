namespace Behavioral.ChainOfResponsibility;

public class L2SupportHandler : SupportHandler
{
    public override void Handle(SupportTicket ticket)
    {
        if (ticket.Level == SupportLevel.L2)
            Console.WriteLine($"[L2 Support] Resolved ticket {ticket.Id}: {ticket.Description}");
        else
        {
            Console.WriteLine($"[L2 Support] Escalating ticket {ticket.Id} to L3...");
            base.Handle(ticket);
        }
    }
}
