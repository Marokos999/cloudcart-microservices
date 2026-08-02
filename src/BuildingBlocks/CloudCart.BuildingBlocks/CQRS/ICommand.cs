using MediatR;

namespace CloudCart.BuildingBlocks.CQRS;

public interface ICommand : IRequest<Unit>
{
    
}

public interface ICommand<TResponse> : IRequest<TResponse>
{
    
}