using MediatR;

namespace CloudCart.BuildingBlocks.CQRS;

public interface IQueryHandler<TQuery, TResponse>: IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    
}