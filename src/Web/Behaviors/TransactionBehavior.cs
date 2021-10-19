using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces.Application.Responses;
using Infrastructure.Persistence;
using MediatR.Pipeline;

namespace Web.Behaviors
{
    public class BeginTransactionBehavior<TRequest> : IRequestPreProcessor<TRequest>
    {
        private readonly ApplicationContext _context;

        public BeginTransactionBehavior(ApplicationContext context)
        {
            _context = context;
        }

        public async Task Process(TRequest request, CancellationToken cancellationToken)
        {
            await _context.BeginTransactionAsync();
        }
    }
    
    public class CommitTransactionBehavior<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
    {
        private readonly ApplicationContext _context;

        public CommitTransactionBehavior(ApplicationContext context)
        {
            _context = context;
        }
        
        public async Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
        {
            if (response is IRollbackTransactionResponse errorResponse)
                _context.RollbackTransaction();
            else
                await _context.CommitTransactionAsync();
        }
    }
    
    public class RollbackTransactionBehavior<TRequest, TResponse, TException> : IRequestExceptionHandler<TRequest, TResponse, TException> where TException : Exception
    {
        private readonly ApplicationContext _context;

        public RollbackTransactionBehavior(ApplicationContext context)
        {
            _context = context;
        }
        
        public Task Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state, CancellationToken ct)
        {
            _context.RollbackTransaction();

            return Task.CompletedTask;
        }
    }
}