namespace Domain.Data;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }

    DateTime? UpdatedAt { get; set; }

    DateTime? DeletedAt { get; set; }

    string CreatedById { get; set; }

    string? UpdatedById { get; set; }

    string? DeletedById { get; set; }
}
