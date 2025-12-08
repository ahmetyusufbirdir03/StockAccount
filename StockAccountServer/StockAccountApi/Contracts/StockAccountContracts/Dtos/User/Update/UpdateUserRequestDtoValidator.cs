using FluentValidation;

namespace StockAccountContracts.Dtos.User.Update;

public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserRequestDtoValidator()
    {
        RuleFor(x => x.Email)
               .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
               .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^0\d{10}$")
            .WithMessage("Geçerli bir telefon numarası giriniz (örn: 05551112233)")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Name)
            .MinimumLength(3).WithMessage("İsim en az 3 karakter olmalı")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));
        RuleFor(x => x.Surname)
            .MinimumLength(3).WithMessage("İsim en az 3 karakter olmalı")
            .When(x => !string.IsNullOrWhiteSpace(x.Surname));


    }
}