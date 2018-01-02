using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using OpenSesame.ViewModels;
using ReactiveUI;
using ReactiveUI.XamForms;
using Xamarin.Forms;

namespace OpenSesame.Views
{
    public partial class LoginView : ReactiveContentPage<LoginViewModel>
	{
		public LoginView()
		{
			InitializeComponent();

			this.BindingContext = new LoginViewModel(){
				Nav = this.Navigation
			};

            this.WhenActivated(disposables => {
                this.Bind(ViewModel, vm => vm.Username, ui => ui.username.Text)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, vm => vm.Password, ui => ui.password.Text)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.ProfilePhotoUrl)
                    .Select(uri => new UriImageSource{ Uri = uri })
                    .BindTo(this, x => x.profilePhoto.Source)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel, vm => vm.TryLogin, ui => ui.login)
                    .DisposeWith(disposables);
                
                this.BindCommand(ViewModel, vm => vm.ViewSettings, ui => ui.settings)
                    .DisposeWith(disposables);
            });

		}
	}
}
