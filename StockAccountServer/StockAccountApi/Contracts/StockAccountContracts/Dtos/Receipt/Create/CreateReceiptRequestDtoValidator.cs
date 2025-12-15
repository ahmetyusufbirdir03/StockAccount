using FluentValidation;
using StockAccountContracts.Dtos.Receipt.Create;

public class CreateReceiptRequestDtoValidator : AbstractValidator<CreateReceiptRequestDto>
{
    public CreateReceiptRequestDtoValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("CompanyId boş olamaz.");

        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("AccountId boş olamaz.");

        RuleFor(x => x.StockId)
            .NotEmpty().WithMessage("StockId boş olamaz.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity 0'dan büyük olmalıdır.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Geçerli bir satış türü giriniz.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description 500 karakterden uzun olamaz.");
    }
}
