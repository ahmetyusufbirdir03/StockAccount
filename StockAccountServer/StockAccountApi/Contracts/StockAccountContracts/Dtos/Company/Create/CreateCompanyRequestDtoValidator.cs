using FluentValidation;

namespace StockAccountContracts.Dtos.Company.Create;

public class CreateCompanyRequestDtoValidator : AbstractValidator<CreateCompanyRequestDto>
{
    public CreateCompanyRequestDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("CompanyName is required.")
            .MaximumLength(100).WithMessage("CompanyName cannot exceed 100 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarası boş olamaz")
            .Matches(@"^0\d{10}$")
            .WithMessage("Geçerli bir telefon numarası giriniz (örn: 05551112233)");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(200).WithMessage("Address cannot exceed 200 characters.");
    }
}