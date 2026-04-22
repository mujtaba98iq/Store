
namespace Sheard.Type;

public class ListingOptions
{
    public int Page { get; init; }
    public int PageSize { get; init; } = 10;
    public OrderDirection? OrderByDirection { get; init; } = OrderDirection.Desc;
}
