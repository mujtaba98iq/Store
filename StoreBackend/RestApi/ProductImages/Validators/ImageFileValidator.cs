using FluentValidation;
using RestApi.Extensions;

namespace RestApi.ProductImages;

public class ImageFileValidator : AbstractValidator<IFormFile>
{
    public ImageFileValidator()
    {
        RuleFor(x => x.Length)
            .GreaterThan(0)
            .WithMessage("Image cannot be empty.")
            .LessThanOrEqualTo(FormFileExtensions.MaxImageSizeInBytes)
            .WithMessage($"Image cannot exceed {FormFileExtensions.MaxImageSizeInMegabytes} MB.");

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(image => image.HasAllowedImageContentType())
            .WithMessage($"Image must be one of the following content types: {FormFileExtensions.AllowedImageContentTypesDescription}.")
            .Must(image => image.HasAllowedImageContent())
            .WithMessage("Image content does not match a supported image format.")
            .When(x => x.Length > 0);
    }
}
