using FluentValidation;

namespace StockAccountContracts.Dtos.Auth.Login;

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Email)
               .NotEmpty().WithMessage("Email adresi boş olamaz")
               .EmailAddress().WithMessage("Geçerli bir email adresi giriniz");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır")
            .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir")
            .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir")
            .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir");
    }
}
