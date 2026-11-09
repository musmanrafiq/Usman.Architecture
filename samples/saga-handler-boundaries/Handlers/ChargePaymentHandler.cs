// ADR-0030 -- HTTP handler: NO DB writes, ONLY external call
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SagaHandlerBoundaries.Entities;
using SagaHandlerBoundaries.Outbox;

namespace SagaHandlerBoundaries.Handlers;

public class ChargePaymentHandler : IEventHandler<OrderPlacedEvent>
{
    private readonly IHttpClientFactory _httpFactory;
