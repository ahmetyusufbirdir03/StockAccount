using FluentValidation;

namespace StockAccountContracts.Dtos.Stock.QuantityUpdate;

public class QuantityRequestDtoValidator : AbstractValidator<QuantityRequestDto>
{
    public QuantityRequestDtoValidator()
    {
        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Amount can not be empty")
            .GreaterThan(0).WithMessage("Amount must be greater than zero");
    }
}
