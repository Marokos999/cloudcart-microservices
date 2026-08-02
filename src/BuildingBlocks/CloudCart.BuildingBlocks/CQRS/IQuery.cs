using MediatR;

namespace CloudCart.BuildingBlocks.CQRS;

public interface IQuery<TResponse> : IRequest<TResponse>
{
    
}