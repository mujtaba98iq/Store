using Domain.Reviews;
using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Reviews;

public class UpdateReviewRequestValidator : BaseValidator<UpdateReviewRequest>
{
    public UpdateReviewRequestValidator()
    {
        // Checked only when given: leaving a field out keeps what is already there, so an
        // absent rating is a customer editing their words and nothing else.
        RuleFor(x => x.Rating)
            .InclusiveBetween(Review.MinRating, Review.MaxRating)
            .WithMessage($"Rating must be between {Review.MinRating} and {Review.MaxRating}.")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.Comment)
            .MaximumLength(ReviewValidation.CommentMaxLength)
            .WithMessage($"Comment cannot exceed {ReviewValidation.CommentMaxLength} characters.")
            .When(x => x.Comment != null);
    }
}
