using System;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Stylet;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels
{
    public class RootViewModel : Screen
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly DialogManager _dialogManager;
        private readonly SettingsService _settingsService;
        private readonly UpdateService _updateService;

        public SnackbarMessageQueue Notifications { get; } = new(TimeSpan.FromSeconds(5));

        public DashboardViewModel Dashboard { get; }

        public RootViewModel(
            IViewModelFactory viewModelFactory,
            DialogManager dialogManager,
            SettingsService settingsService,
            UpdateService updateService)
        {
            _viewModelFactory = viewModelFactory;
            _dialogManager = dialogManager;
            _settingsService = settingsService;
            _updateService = updateService;

            Dashboard = _viewModelFactory.CreateDashboardViewModel();

            DisplayName = $"{App.Name} v{App.VersionString}";
        }

        private async Task ShowUkraineSupportMessageAsync()
        {
            if (!_settingsService.IsUkraineSupportMessageEnabled)
                return;

            var dialog = _viewModelFactory.CreateMessageBoxViewModel(
                "Bienvenue sur YoutubeDownloader",
                @"Message du créateur Alors que la Russie mène une guerre génocidaire contre mon pays, je suis reconnaissant envers tous ceux qui continuent de soutenir l'Ukraine dans notre lutte pour la liberté.

                Ce logiciel a étais crée par Tyrrrz puis redevelopper par danbenba.",
                "Acceder à la page du créateur",
                "Fermer"
            );

            // Désactiver ce message à l'avenir
            _settingsService.IsUkraineSupportMessageEnabled = false;
            _settingsService.Save();

            if (await _dialogManager.ShowDialogAsync(dialog) == true)
                ProcessEx.StartShellExecute("https://tyrrrz.me/ukraine?source=youtubedownloader");
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                var updateVersion = await _updateService.CheckForUpdatesAsync();
                if (updateVersion is null)
                    return;

                Notifications.Enqueue($"Téléchargement de la mise à jour vers {App.Name} v{updateVersion}...");
                await _updateService.PrepareUpdateAsync(updateVersion);

                Notifications.Enqueue(
                    "La mise à jour a été téléchargée et sera installée lorsque vous quitterez l'application",
                    "INSTALLER MAINTENANT", () =>
                    {
                        _updateService.FinalizeUpdate(true);
                        RequestClose();
                    }
                );
            }
            catch
            {
                // Échec de la mise à jour ne doit pas planter l'application
                Notifications.Enqueue("Échec de la mise à jour de l'application");
            }
        }

        public async void OnViewFullyLoaded()
        {
            await ShowUkraineSupportMessageAsync();
            await CheckForUpdatesAsync();
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();

            _settingsService.Load();

            // Synchroniser le thème avec les paramètres
            if (_settingsService.IsDarkModeEnabled)
            {
                App.SetDarkTheme();
            }
            else
            {
                App.SetLightTheme();
            }

            // L'application vient d'être mise à jour, afficher le journal des modifications
            if (_settingsService.LastAppVersion is not null && _settingsService.LastAppVersion != App.Version)
            {
                Notifications.Enqueue(
                    $"Mise à jour réussie vers {App.Name} v{App.VersionString}",
                    "JOURNAL DES MODIFICATIONS", () => ProcessEx.StartShellExecute(App.ChangelogUrl)
                );

                _settingsService.LastAppVersion = App.Version;
                _settingsService.Save();
            }
        }

        protected override void OnClose()
        {
            base.OnClose();

            Dashboard.CancelAllDownloads();

            _settingsService.Save();
            _updateService.FinalizeUpdate(false);
        }
    }
}
