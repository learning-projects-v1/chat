using ChatApp.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Services;

public class BaseRepository<T>: IRepository<T> where T : class
{
    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(T entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
