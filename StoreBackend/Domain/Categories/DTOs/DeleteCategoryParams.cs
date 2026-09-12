namespace Domain.Categories;

public class DeleteCategoryParams
{
    public required Guid Id { get; set; }
    public required string DeletedById { get; set; }
}
