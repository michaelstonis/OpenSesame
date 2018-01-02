using System;
using ReactiveUI;

namespace OpenSesame.ViewModels
{
    public class ViewModelBase : ReactiveObject, ISupportsActivation
    {
        protected readonly ViewModelActivator _viewModelActivator = new ViewModelActivator();

        public ViewModelActivator Activator
        {
            get { return _viewModelActivator; }
        }
    }
}
