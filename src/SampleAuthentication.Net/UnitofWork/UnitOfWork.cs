using SampleAuthentication.Net.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleAuthentication.Net.UnitofWork
{
    public interface IUnitOfWork : IDisposable
    {
        ApplicationDBContext Context { get; }
        void SaveChanges();
    }
    public class UnitOfWork : IUnitOfWork
    {
        public ApplicationDBContext Context { get; }
        private bool _isDisposed;
        public UnitOfWork(ApplicationDBContext context)
        {
            Context = context;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    Context.Dispose();
                }
            }
            _isDisposed = true;
        }
        public void SaveChanges()
        {
            Context.SaveChanges();
        }
    }
}
