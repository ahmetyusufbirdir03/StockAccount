using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Auth.Register;

namespace StockAccountContracts;

public static class Registration
{
    public static void AddContracts(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .SelectMany(e => e.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                var response = ResponseDto<object>.Fail(errors, StatusCodes.Status400BadRequest);

                return new BadRequestObjectResult(response);
            };
        });

    }
}
