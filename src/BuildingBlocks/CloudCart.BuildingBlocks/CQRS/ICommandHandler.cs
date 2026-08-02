using MediatR;

namespace CloudCart.BuildingBlocks.CQRS;

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Unit> where TCommand : ICommand
{
    
}

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
 where TCommand : ICommand<TResponse>
{
    
}