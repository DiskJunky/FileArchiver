using Ninject.Modules;

namespace FileArchiver.Tests.TestInfrastructure
{
    /// <summary>
    /// Ninject module for configuring test dependencies.
    /// </summary>
    public class TestKernelModule : NinjectModule
    {
        public override void Load()
        {
            // Register test-specific bindings here
            // Services will be mocked in individual tests using MockingKernel
        }
    }
}