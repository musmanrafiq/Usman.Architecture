// ADR-0030 -- Write handler: DB writes + outbox logging (NO HTTP)
using Microsoft.EntityFrameworkCore;
using SagaHandlerBoundaries.Entities;
using SagaHandlerBoundaries.Outbox;
using System.Threading;
using System.Threading.Tasks;

namespace SagaHandlerBoundaries.Handlers;

