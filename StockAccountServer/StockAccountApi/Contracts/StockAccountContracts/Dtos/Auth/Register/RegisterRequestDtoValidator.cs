using FluentValidation;

namespace StockAccountContracts.Dtos.Auth.Register;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi boş olamaz")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz")
            .MinimumLength(5).WithMessage("Şifre en az 5 karakter olmalıdır")
            .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir")
            .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir")
            .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarası boş olamaz")
            .Matches(@"^0\d{10}$")
            .WithMessage("Geçerli bir telefon numarası giriniz (örn: 05551112233)");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("İsim boş olamaz")
            .MinimumLength(5).WithMessage("İsim adı en az 5 karakter olmalı");

        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage("Soyisim boş olamaz")
            .MinimumLength(5).WithMessage("SoyİSİM adı en az 5 karakter olmalı");
    }
}
