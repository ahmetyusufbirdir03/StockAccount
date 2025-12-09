using FluentValidation;

namespace StockAccountContracts.Dtos.StockTrans.Create;

public class CreateStockTransRequestDtoValidator : AbstractValidator<CreateStockTransRequestDto>
{
    public CreateStockTransRequestDtoValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("CompanyId is required");
        RuleFor(x => x.StockId)
            .NotEmpty().WithMessage("StockId is required");
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid transaction type");
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than or equal to zero");
        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("UnitPrice must be greater than or equal to zero");
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
    }
}