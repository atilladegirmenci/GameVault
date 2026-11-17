using FluentAssertions;
using GameDBWebApi.DTOs.Review;
using GameDBWebApi.DTOs.Validators;

namespace GameDBWebApi.Tests.Unit.Validators;

public class CreateReviewRequestDtoValidatorTests
{
    private readonly CreateReviewRequestDtoValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Validate_RatingWithinOneAndTen_IsValid(short rating)
    {
        var result = _validator.Validate(new CreateReviewRequestDto
        {
            Rating = rating,
            Comment = "A review"
        });

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Validate_RatingOutsideOneAndTen_IsInvalid(short rating)
    {
        var result = _validator.Validate(new CreateReviewRequestDto
        {
            Rating = rating,
            Comment = "A review"
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateReviewRequestDto.Rating));
    }
}
