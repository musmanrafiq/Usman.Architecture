// ADR-0030 -- Read handler: NO DB writes, NO HTTP calls
using Microsoft.EntityFrameworkCore;
using SagaHandlerBoundaries.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SagaHandlerBoundaries.Handlers;

public class ListenForOrderPlacedHandler : IEventHandler<OrderPlacedEvent>
{
    private readonly AppDbContext _db;
    private readonly ILogger<ListenForOrderPlacedHandler> _logger;

    public ListenForOrderPlacedHandler(AppDbContext db, ILogger<ListenForOrderPlacedHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Handle(OrderPlacedEvent evt, CancellationToken ct)
    {
        // READ: fetch product price (read-only query)
        var product = await _db.Products.FindAsync(evt.ProductId);

        // No write operations here
        // No HTTP calls here
