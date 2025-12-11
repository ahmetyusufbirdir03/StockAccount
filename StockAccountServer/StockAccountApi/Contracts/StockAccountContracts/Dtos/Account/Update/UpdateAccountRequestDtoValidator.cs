using FluentValidation;

namespace StockAccountContracts.Dtos.Account.Update;

public class UpdateAccountRequestDtoValidator : AbstractValidator<UpdateAccountRequestDto>
{
    public UpdateAccountRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi boş olamaz")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarası boş olamaz")
            .Matches(@"^0\d{10}$")
            .WithMessage("Geçerli bir telefon numarası giriniz (örn: 05551112233)");

        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("İsim boş olamaz")
            .MinimumLength(5).WithMessage("İsim adı en az 5 karakter olmalı");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(200).WithMessage("Address cannot exceed 200 characters.");
    }
}