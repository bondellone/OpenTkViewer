using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using OpenTkViewer.Controls;
using OpenTkViewer.ViewModels;

namespace OpenTkViewer
{
    public class OpenTkViewerBootstrapper : BootstrapperBase
    {
        private SimpleContainer container;

        public OpenTkViewerBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            container = new SimpleContainer();
            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<Camera>();
            container.Singleton<OpenTkControl>();
            container.Singleton<ViewerScene>();
            container.Singleton<SceneViewModel>();
            container.PerRequest<ShellViewModel>();
            base.Configure();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            return container.GetInstance(serviceType, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return container.GetAllInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }
    }
}