using Domain.Reviews;
using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Reviews;

public class CreateReviewRequestValidator : BaseValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required.");

        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required.");

        // Only the scale is checked here. Whether the caller bought the product on that
        // order is a question about the order, so the service answers it.
        RuleFor(x => x.Rating)
            .InclusiveBetween(Review.MinRating, Review.MaxRating)
            .WithMessage($"Rating must be between {Review.MinRating} and {Review.MaxRating}.");

        RuleFor(x => x.Comment)
            .MaximumLength(ReviewValidation.CommentMaxLength)
            .WithMessage($"Comment cannot exceed {ReviewValidation.CommentMaxLength} characters.")
            .When(x => x.Comment != null);
    }
}
