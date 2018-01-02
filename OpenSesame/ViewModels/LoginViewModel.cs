using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MyQNET;
using OpenSesame.Store;
using OpenSesame.Utils;
using OpenSesame.Views;
using ReactiveUI;
using Rendr.XFKit.Core.Util;
using Xamarin.Forms;

namespace OpenSesame.ViewModels
{
    public class LoginViewModel : ViewModelBase
	{
		string _username;
        public string Username
        {
            get { return _username; }
            private set { this.RaiseAndSetIfChanged(ref _username, value); }
        }

        string _password;
        public string Password
        {
            get { return _password; }
            set { this.RaiseAndSetIfChanged(ref _password, value); }
        }

        Uri _profilePhotoUrl;
        public Uri ProfilePhotoUrl
        {
            get { return _profilePhotoUrl; }
            private set { this.RaiseAndSetIfChanged(ref _profilePhotoUrl, value); }
        }

        ReactiveCommand<object, Unit> _tryLogin;

        public ReactiveCommand<object, Unit> TryLogin
        {
            get { return _tryLogin; }
            private set { this.RaiseAndSetIfChanged(ref _tryLogin, value); }
        }

        ReactiveCommand<object, Unit> _viewSettings;

        public ReactiveCommand<object, Unit> ViewSettings
        {
            get { return _viewSettings; }
            private set { this.RaiseAndSetIfChanged(ref _viewSettings, value); }
        }

        ObservableAsPropertyHelper<bool> _authenticationIsValid;

        public bool AuthenticationIsValid
        {
            get => _authenticationIsValid?.Value ?? false;
        }

        public INavigation Nav {get;set;}

        IMyQService _svc;

        public LoginViewModel()
		{
            this.Username = ApplicationStore.Username;
            this.Password = ApplicationStore.Password;

			_svc = DependencyService.Get<IMyQService>(DependencyFetchTarget.GlobalInstance);

            this.WhenActivated(disposables => {
                TryLogin = ReactiveCommand.CreateFromTask<object>(
                    //The action to run for the command
                    async x =>
                    {
                        ApplicationStore.Username = Username;
                        ApplicationStore.Password = Password;

                        (var success, var token) = await _svc.Login(Username, Password);

                        if (success)
                        {
                            ApplicationStore.SecurityToken = token;
                            await Nav.PopModalAsync();
                        }
                    },
                    //When can this command run
                    Observable.CombineLatest(
                        this.WhenAnyObservable(x => x.TryLogin.IsExecuting).StartWith(false),
                        this.WhenAnyValue(x => x.AuthenticationIsValid).StartWith(false),
                        (isExecuting, authenticationIsValid) => !isExecuting && authenticationIsValid))
                    .DisposeWith(disposables);


                ViewSettings = ReactiveCommand.CreateFromTask<object>(
                    //The action to run for the command
                    async _ => await Nav.PushModalAsync(new SettingsView()),
                    //When can this command run
                    this.WhenAnyObservable(x => x.ViewSettings.IsExecuting).Select(isExecuting => !isExecuting))
                    .DisposeWith(disposables);


                this.WhenAnyValue(vm => vm.Username)
                    .SubscribeOn(RxApp.TaskpoolScheduler)
                    .Select(username => {
                        if (string.IsNullOrEmpty(username) || !RegexUtil.ValidEmailAddress().Match(username).Success)
                            return default(Uri);
                        
                        return Gravatar.GetImageUrl(username, 88 * 2);
                    })
                    .Catch(Observable.Return(default(Uri)))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => ProfilePhotoUrl = x)
                    .DisposeWith(disposables);

                this.WhenAnyObservable(x => x.Changed)
                    .SubscribeOn(RxApp.TaskpoolScheduler)
                    .Select(_ => Unit.Default)
                    .StartWith(Unit.Default)
                    .Select(x => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                    .ToProperty(this, x => x.AuthenticationIsValid, out _authenticationIsValid, false, scheduler: RxApp.MainThreadScheduler)
                    .DisposeWith(disposables);
            });

		}
	}
}
