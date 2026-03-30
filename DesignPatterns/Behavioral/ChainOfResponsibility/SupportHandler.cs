namespace Behavioral.ChainOfResponsibility;

/// <summary>
/// Abstract handler — builds the chain via SetNext() and delegates
/// to the next handler if the current one cannot process the request.
/// </summary>
public abstract class SupportHandler
{
    private SupportHandler? _next;

    /// <summary>Returns next so callers can chain: l1.SetNext(l2).SetNext(l3)</summary>
    public SupportHandler SetNext(SupportHandler next)
    {
        _next = next;
        return next;
    }

    public virtual void Handle(SupportTicket ticket)
    {
        _next?.Handle(ticket);
    }
}
