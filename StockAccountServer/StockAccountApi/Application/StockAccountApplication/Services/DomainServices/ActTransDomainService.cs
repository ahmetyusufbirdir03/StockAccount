using StockAccountDomain.Entities;
using StockAccountDomain.Models;
using StockAccountDomain.Respositories;
using StockAccountDomain.Services;

namespace StockAccountApplication.Services.DomainServices;

public class ActTransDomainService : IActTransDomainService
{
    private readonly IActTransRepository _actTransRepository;
    public ActTransDomainService(IActTransRepository actTransRepository)
    {
        _actTransRepository = actTransRepository;
    }
    public async Task<ActTrans> CreateActTransAsync(ActTransModel actTransModel)
    {
        var actTrans = new ActTrans(
           actTransModel.CompanyId, actTransModel.AccountId, actTransModel.ReceiptId,
           actTransModel.Amount, actTransModel.Description);

        actTrans.CreatedAt = DateTime.UtcNow;

        var responseData = await _actTransRepository.CreateActTransAsync(actTrans);

        return responseData;
    }
}
