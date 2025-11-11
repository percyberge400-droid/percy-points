using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Interfaces
{
    public interface ISqliteRepositoryFactory
    {
        IRepository<T> CreateRepository<T>() where T : class;
    }
}
