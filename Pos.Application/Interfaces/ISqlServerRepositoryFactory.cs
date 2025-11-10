using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Interfaces
{
    public interface ISqlServerRepositoryFactory
    {
        IRepository<T> CreateRepository<T>() where T : class;
    }
}
