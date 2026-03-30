namespace Behavioral.ChainOfResponsibility;

public static class ChainDemo
{
    public static void Run()
    {
        var l1 = new L1SupportHandler();
        var l2 = new L2SupportHandler();
        var l3 = new L3SupportHandler();
        l1.SetNext(l2).SetNext(l3);

        var tickets = new[]
        {
            new SupportTicket("T-001", "Password reset",       SupportLevel.L1),
            new SupportTicket("T-002", "Billing issue",        SupportLevel.L2),
            new SupportTicket("T-003", "Production data loss", SupportLevel.L3),
        };

        foreach (var ticket in tickets)
            l1.Handle(ticket);
    }
}
