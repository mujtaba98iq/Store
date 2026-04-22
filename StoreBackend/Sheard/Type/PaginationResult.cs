namespace Sheard.Type;

public class PaginationResult<T>
{
    public required int TotalCount { get; init; }
    public required List<T> Data { get; init; }
}
