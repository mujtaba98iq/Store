namespace RestApi.Reviews;

/// <summary>
/// Limits shared by the create and update validators, so the two cannot drift apart. The
/// star scale is not here: it is a domain rule and lives on the entity.
/// </summary>
public static class ReviewValidation
{
    public const int CommentMaxLength = 2000;
}
