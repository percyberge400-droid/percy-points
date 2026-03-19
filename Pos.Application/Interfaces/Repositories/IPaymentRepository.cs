using Pos.Domain.Entities;

namespace Pos.Application.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllPayment(string env);
    }
}
