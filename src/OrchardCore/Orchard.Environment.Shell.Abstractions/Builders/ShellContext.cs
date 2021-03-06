using System;
using Orchard.Environment.Shell.Builders.Models;
using Orchard.Environment.Shell;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace Orchard.Hosting.ShellBuilders
{
    /// <summary>
    /// The shell context represents the shell's state that is kept alive
    /// for the whole life of the application
    /// </summary>
    public class ShellContext : IDisposable
    {
        private bool _disposed = false;
        private int _refCount = 0;
        private bool _released = false;

        public ShellSettings Settings { get; set; }
        public ShellBlueprint Blueprint { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Whether the shell is activated. 
        /// </summary>
        public bool IsActivated { get; set; }

        /// <summary>
        /// Creates a standalone service scope that can be used to resolve local services.
        /// </summary>
        public IServiceScope CreateServiceScope()
        {
            if (_disposed)
            {
                throw new InvalidOperationException("Can't use CreateServiceScope on a disposed context");
            }

            if (_released)
            {
                throw new InvalidOperationException("Can't use CreateServiceScope on a released context");
            }

            return ServiceProvider.CreateScope();
        }

        /// <summary>
        /// Whether the <see cref="ShellContext"/> instance has been released, for instance when a tenant is changed.
        /// </summary>
        public bool Released => _released;

        /// <summary>
        /// Returns the number of active requests on this tenant.
        /// </summary>
        public int ActiveRequests => _refCount;

        public void RequestStarted()
        {
            Interlocked.Increment(ref _refCount);
        }

        public void RequestEnded()
        {
            var refCount = Interlocked.Decrement(ref _refCount);

            if (_released && refCount == 0)
            {
                Dispose();
            }
        }

        /// <summary>
        /// Mark the <see cref="ShellContext"/> has a candidate to be released.
        /// </summary>
        public void Release()
        {
            // When a tenant is changed and should be restarted, its shell context is replaced with a new one, 
            // so that new request can't use it anymore. However some existing request might still be running and try to 
            // resolve or use its services. We then call this method to count the remaining references and dispose it 
            // when the number reached zero.

            _released = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                // Disposes all the services registered for this shell
                if (ServiceProvider != null)
                {
                    (ServiceProvider as IDisposable)?.Dispose();
                    ServiceProvider = null;
                }

                IsActivated = false;

                Settings = null;
                Blueprint = null;

                _disposed = true;
            }
        }
        
        ~ShellContext()
        {
            Dispose(false);
        }
    }
}