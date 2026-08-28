using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RestApi.Validation;

/// <summary>
/// multipart/form-data counterpart of UseBodyValidator. The shipped body validator
/// only resolves parameters bound from [FromBody], so it cannot be used on the
/// endpoints that receive an uploaded file through [FromForm].
/// </summary>
public class UseFormValidator : global::UseValidator.UseValidator
{
    protected override object GetPayload(ActionExecutingContext context)
    {
        var formParameters = context.ActionDescriptor.Parameters.Where(IsFromForm).ToList();

        if (formParameters.Count != 1)
        {
            throw new InvalidOperationException("UseFormValidator requires exactly one parameter decorated with [FromForm]");
        }

        var name = formParameters[0].Name;
        context.ActionArguments.TryGetValue(name, out var payload);

        return payload
               ?? throw new InvalidOperationException($"The ActionArgument '{name}' is null. Ensure that the action is called with the appropriate form data.");

        static bool IsFromForm(ParameterDescriptor parameter)
        {
            return parameter.BindingInfo?.BindingSource?.Id == BindingSource.Form.Id;
        }
    }
}
