namespace Domain.Categories;

public class CreateCategoryParams
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string CreatedById { get; set; }
}
