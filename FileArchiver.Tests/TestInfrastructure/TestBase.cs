using Ninject;
using Ninject.MockingKernel.Moq;

namespace FileArchiver.Tests.TestInfrastructure
{
    /// <summary>
    /// Base class for tests providing Ninject dependency injection with mocking support.
    /// Uses MoqMockingKernel to automatically create mocks for dependencies.
    /// </summary>
    public abstract class TestBase : IDisposable
    {
        protected MoqMockingKernel Kernel { get; }

        protected TestBase()
        {
            Kernel = new MoqMockingKernel();
            Kernel.Load<TestKernelModule>();
            ConfigureKernel(Kernel);
        }

        /// <summary>
        /// Override this method to configure additional bindings for specific test classes.
        /// </summary>
        protected virtual void ConfigureKernel(IKernel kernel)
        {
            // Override in derived classes to add specific bindings
        }

        /// <summary>
        /// Gets a mock of the specified type from the kernel.
        /// </summary>
        protected T GetMock<T>() where T : class
        {
            return Kernel.Get<T>();
        }

        /// <summary>
        /// Resolves an instance from the kernel.
        /// </summary>
        protected T Get<T>()
        {
            return Kernel.Get<T>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Kernel?.Dispose();
            }
        }
    }
}