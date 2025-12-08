using FluentValidation;
using StockAccountDomain.Enums;

namespace StockAccountContracts.Dtos.Stock.Create;

public class CreateStockRequestDtoValidator : AbstractValidator<CreateStockRequestDto>
{
    public CreateStockRequestDtoValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("CompanyId boş olamaz.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MinimumLength(3).WithMessage("Ürün adı en az 3 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Ürün adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0m).WithMessage("Miktar 0'dan büyük veya eşit olmalıdır.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Geçerli bir birim seçiniz.")
            .Must(u => u != UnitEnum.Unknown).WithMessage("Geçerli bir birim seçiniz.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0m).WithMessage("Fiyat negatif olamaz.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}