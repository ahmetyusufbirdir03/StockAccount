using FluentValidation;

namespace StockAccountContracts.Dtos.Stock.Update;

public class UpdateStockRequestDtoValidator : AbstractValidator<UpdateStockRequestDto>
{
    public UpdateStockRequestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Stok Id boþ olamaz.");

        RuleFor(x => x.Name)
            .MinimumLength(3).WithMessage("Ürün adý en az 3 karakter olmalýdýr.")
            .MaximumLength(100).WithMessage("Ürün adý en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0m).WithMessage("Miktar 0 veya daha büyük olmalýdýr.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0m).WithMessage("Fiyat negatif olamaz.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açýklama en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}