namespace Domain.Categories;

public class UpdateCategoryParams
{
    public required Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public required string UpdatedById { get; set; }
}
