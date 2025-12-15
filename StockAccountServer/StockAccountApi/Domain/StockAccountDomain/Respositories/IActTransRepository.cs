using StockAccountDomain.Entities;

namespace StockAccountDomain.Respositories;

public interface IActTransRepository
{
    Task<ActTrans> CreateActTransAsync(ActTrans actTrans);
}
