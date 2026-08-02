namespace CloudCart.BuildingBlocks.Pagination;

public class PaginatedResult<TEntity>
{
    public IEnumerable<TEntity> Data { get;} = [];
    public long Total { get; }
    public int Page { get; }
    public int PageSize { get; }

    public PaginatedResult(IEnumerable<TEntity> data, long total, int page, int pageSize )
    {
        Data = data;
        Total = total;
        Page = page;
        PageSize = pageSize;
    }
}