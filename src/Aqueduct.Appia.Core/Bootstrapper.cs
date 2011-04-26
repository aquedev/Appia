﻿namespace Aqueduct.Appia.Core
{
    using Nancy.Session;
    using Nancy;
    using Nancy.ViewEngines;
    using System;
    using System.Collections.Generic;
    using System.Linq;

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

        protected override void RegisterViewEngines(TinyIoC.TinyIoCContainer container, IEnumerable<Type> viewEngineTypes)
        {
            this.container.RegisterMultiple<IViewEngine>(viewEngineTypes.Where(eng => eng != typeof(SuperSimpleViewEngine))).AsSingleton();
        }

        protected override Type DefaultViewFactory
        {
            get { return typeof(AppiaViewFactory); }
        }
    }
}