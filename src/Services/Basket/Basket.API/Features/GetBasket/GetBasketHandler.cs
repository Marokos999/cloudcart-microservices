using Basket.API.Data;
using Basket.API.Models;
using CloudCart.BuildingBlocks.CQRS;

namespace Basket.API.Features.GetBasket;

public record GetBasketQuery(string UserName) : IQuery<GetBasketResult>;
public record GetBasketResult(ShoppingCart Cart);

public class GetBasketHandler(IBasketRepository repository) : IQueryHandler<GetBasketQuery, GetBasketResult>
{
    public async Task<GetBasketResult> Handle(GetBasketQuery query, CancellationToken cancellationToken)
    {
        var cart = await repository.GetBasket(query.UserName, cancellationToken);
        return  new GetBasketResult(cart);
    }
}