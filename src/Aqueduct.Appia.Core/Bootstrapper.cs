namespace Aqueduct.Appia.Core
{
    using Nancy.Session;
    using Nancy;
    using Nancy.ViewEngines;
    using System;
    using System.Collections.Generic;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        public override void ConfigureRequestContainer(TinyIoC.TinyIoCContainer existingContainer)
        {
            base.ConfigureRequestContainer(existingContainer);
        }

        protected override void InitialiseInternal(TinyIoC.TinyIoCContainer container)
        {
            base.InitialiseInternal(container);

            CookieBasedSessions.Enable(this, "MyPassPhrase", "MySaltIsReallyGood", "MyHmacPassphrase");
        }

        protected override void RegisterViewSourceProviders(TinyIoC.TinyIoCContainer container, System.Collections.Generic.IEnumerable<System.Type> viewSourceProviderTypes)
        {
            container.RegisterMultiple<IViewSourceProvider>(
                new List<Type> { typeof(FileSystemViewSourceProvider) })
                .AsSingleton();
        }
    }
}