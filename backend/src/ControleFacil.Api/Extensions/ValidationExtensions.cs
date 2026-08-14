using FluentValidation;

namespace ControleFacil.Api.Extensions;

public static class ValidationExtensions
{
    public static async Task<IResult?> ValidateOrProblemAsync<T>(this IValidator<T> validator, T instance)
    {
        var result = await validator.ValidateAsync(instance);
        return result.IsValid ? null : Results.ValidationProblem(result.ToDictionary());
    }
}
