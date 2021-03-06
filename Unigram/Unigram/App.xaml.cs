﻿using Microsoft.HockeyApp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Template10.Common;
using Template10.Services.NavigationService;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Services;
using Unigram.Services.Settings;
using Unigram.ViewModels;
using Unigram.ViewModels.BasicGroups;
using Unigram.ViewModels.Channels;
using Unigram.ViewModels.Chats;
using Unigram.ViewModels.Delegates;
using Unigram.ViewModels.Dialogs;
using Unigram.ViewModels.Payments;
using Unigram.ViewModels.SecretChats;
using Unigram.ViewModels.Settings;
using Unigram.ViewModels.Settings.Privacy;
using Unigram.ViewModels.SignIn;
using Unigram.ViewModels.Supergroups;
using Unigram.ViewModels.Users;
using Unigram.Views;
using Unigram.Views.BasicGroups;
using Unigram.Views.Channels;
using Unigram.Views.Chats;
using Unigram.Views.Dialogs;
using Unigram.Views.Host;
using Unigram.Views.Payments;
using Unigram.Views.SecretChats;
using Unigram.Views.Settings;
using Unigram.Views.Settings.Privacy;
using Unigram.Views.SignIn;
using Unigram.Views.Supergroups;
using Unigram.Views.Users;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Media.SpeechRecognition;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Resources;

namespace Unigram
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : BootStrapper
    {
        public static ShareOperation ShareOperation { get; set; }
        public static Window ShareWindow { get; set; }

        public static ConcurrentDictionary<long, DataPackageView> DataPackages { get; } = new ConcurrentDictionary<long, DataPackageView>();

        public static AppServiceConnection Connection { get; private set; }

        private readonly UISettings _uiSettings;

        public UISettings UISettings => _uiSettings;

        //public ViewModelLocator Locator
        //{
        //    get
        //    {
        //        return Resources["Locator"] as ViewModelLocator;
        //    }
        //}

        public ViewModelLocator Locator { get; } = new ViewModelLocator();

        public static MediaPlayer Playback { get; } = new MediaPlayer();

        private BackgroundTaskDeferral appServiceDeferral = null;

        private MediaExtensionManager m_mediaExtensionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
#if DEBUG
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en";
#endif

            Locator.Configure(/*session*/);

            if (!SettingsService.Current.Appearance.RequestedTheme.HasFlag(TelegramTheme.Default))
            {
                RequestedTheme = SettingsService.Current.Appearance.GetCalculatedApplicationTheme();
            }

            InitializeComponent();

            _uiSettings = new UISettings();

            m_mediaExtensionManager = new MediaExtensionManager();
            m_mediaExtensionManager.RegisterByteStreamHandler("Unigram.Native.OpusByteStreamHandler", ".ogg", "audio/ogg");

            InactivityHelper.Detected += Inactivity_Detected;

            UnhandledException += async (s, args) =>
            {
                args.Handled = true;

                try
                {
                    await new TLMessageDialog(args.Exception?.ToString() ?? string.Empty, "Unhandled exception").ShowQueuedAsync();
                }
                catch { }
            };

#if !DEBUG

            HockeyClient.Current.Configure(Constants.HockeyAppId,
                new TelemetryConfiguration()
                {
                    EnableDiagnostics = true,
                    Collectors = WindowsCollectors.Metadata |
                                 WindowsCollectors.Session |
                                 WindowsCollectors.UnhandledException
                });

#endif
        }

        public override INavigable ResolveForPage(Page page, INavigationService navigationService)
        {
            var id = navigationService is TLNavigationService ex ? ex.SessionId : 0;
            var container = TLContainer.Current;

            switch (page)
            {
                case BasicGroupEditPage basicGroupEdit:
                    return container.Resolve<BasicGroupEditViewModel, IBasicGroupDelegate>(basicGroupEdit, id);
                case ChannelCreateStep1Page channelCreateStep1:
                    return container.Resolve<ChannelCreateStep1ViewModel>(id);
                case ChannelCreateStep2Page channelCreateStep2:
                    return container.Resolve<ChannelCreateStep2ViewModel>(id);
                case ChannelCreateStep3Page channelCreateStep3:
                    return container.Resolve<ChannelCreateStep3ViewModel>(id);
                case BasicGroupCreateStep1Page chatCreateStep1:
                    return container.Resolve<BasicGroupCreateStep1ViewModel>(id);
                case BasicGroupCreateStep2Page chatCreateStep2:
                    return container.Resolve<BasicGroupCreateStep2ViewModel>(id);
                case ChatInviteLinkPage chatInviteLink:
                    return container.Resolve<ChatInviteLinkViewModel>(id);
                case ChatInvitePage chatInvite:
                    return container.Resolve<ChatInviteViewModel>(id);
                case ChatSharedMediaPage dialogSharedMedia:
                    return container.Resolve<ChatSharedMediaViewModel, IFileDelegate>(dialogSharedMedia, id);
                case DialogShareLocationPage dialogShareLocation:
                    return container.Resolve<DialogShareLocationViewModel>(id);
                case InstantPage instant:
                    return container.Resolve<InstantViewModel>(id);
                case MainPage main:
                    return container.Resolve<MainViewModel>(id);
                case PaymentFormStep1Page paymentFormStep1:
                    return container.Resolve<PaymentFormStep1ViewModel>(id);
                case PaymentFormStep2Page paymentFormStep2:
                    return container.Resolve<PaymentFormStep2ViewModel>(id);
                case PaymentFormStep3Page paymentFormStep3:
                    return container.Resolve<PaymentFormStep3ViewModel>(id);
                case PaymentFormStep4Page paymentFormStep4:
                    return container.Resolve<PaymentFormStep4ViewModel>(id);
                case PaymentFormStep5Page paymentFormStep5:
                    return container.Resolve<PaymentFormStep5ViewModel>(id);
                case PaymentReceiptPage paymentReceipt:
                    return container.Resolve<PaymentReceiptViewModel>(id);
                case ProfilePage profile:
                    return container.Resolve<ProfileViewModel, IProfileDelegate>(profile, id);
                case SecretChatCreatePage secretChatCreate:
                    return container.Resolve<SecretChatCreateViewModel>(id);
                case SettingsPhoneIntroPage settingsPhoneIntro:
                    return container.Resolve<SettingsPhoneIntroViewModel>(id);
                case SettingsPhonePage settingsPhone:
                    return container.Resolve<SettingsPhoneViewModel>(id);
                case SettingsPhoneSentCodePage settingsPhoneSentCode:
                    return container.Resolve<SettingsPhoneSentCodeViewModel>(id);
                case SettingsPrivacyAllowCallsPage settingsPrivacyAllowCalls:
                    return container.Resolve<SettingsPrivacyAllowCallsViewModel>(id);
                case SettingsPrivacyAllowChatInvitesPage settingsPrivacyAllowChatInvites:
                    return container.Resolve<SettingsPrivacyAllowChatInvitesViewModel>(id);
                case SettingsPrivacyAlwaysAllowCallsPage settingsPrivacyAlwaysAllowCalls:
                    return container.Resolve<SettingsPrivacyAlwaysAllowCallsViewModel>(id);
                case SettingsPrivacyAlwaysAllowChatInvitesPage settingsPrivacyAlwaysAllowChatInvites:
                    return container.Resolve<SettingsPrivacyAlwaysAllowChatInvitesViewModel>(id);
                case SettingsPrivacyAlwaysShowStatusPage settingsPrivacyAlwaysShowStatus:
                    return container.Resolve<SettingsPrivacyAlwaysShowStatusViewModel>(id);
                case SettingsPrivacyNeverAllowCallsPage settingsPrivacyNeverAllowCalls:
                    return container.Resolve<SettingsPrivacyNeverAllowCallsViewModel>(id);
                case SettingsPrivacyNeverAllowChatInvitesPage settingsPrivacyNeverAllowChatInvites:
                    return container.Resolve<SettingsPrivacyNeverAllowChatInvitesViewModel>(id);
                case SettingsPrivacyNeverShowStatusPage settingsPrivacyNeverShowStatus:
                    return container.Resolve<SettingsPrivacyNeverShowStatusViewModel>(id);
                case SettingsPrivacyShowStatusPage settingsPrivacyShowStatus:
                    return container.Resolve<SettingsPrivacyShowStatusViewModel>(id);
                case SettingsAppearancePage settingsAppearance:
                    return container.Resolve<SettingsAppearanceViewModel>(id);
                case SettingsBlockedUsersPage settingsBlockedUsers:
                    return container.Resolve<SettingsBlockedUsersViewModel, IFileDelegate>(settingsBlockedUsers, id);
                case SettingsBlockUserPage settingsBlockUser:
                    return container.Resolve<SettingsBlockUserViewModel>(id);
                case SettingsDataAndStoragePage settingsDataAndStorage:
                    return container.Resolve<SettingsDataAndStorageViewModel>(id);
                case SettingsDataAutoPage settingsDataAuto:
                    return container.Resolve<SettingsDataAutoViewModel>(id);
                case SettingsAdvancedPage settingsGeneral:
                    return container.Resolve<SettingsAdvancedViewModel>(id);
                case SettingsLanguagePage settingsLanguage:
                    return container.Resolve<SettingsLanguageViewModel>(id);
                case SettingsMasksArchivedPage settingsMasksArchived:
                    return container.Resolve<SettingsMasksArchivedViewModel>(id);
                case SettingsMasksPage settingsMasks:
                    return container.Resolve<SettingsMasksViewModel>(id);
                case SettingsNotificationsPage settingsNotifications:
                    return container.Resolve<SettingsNotificationsViewModel>(id);
                case SettingsPrivacyAndSecurityPage settingsPrivacyAndSecurity:
                    return container.Resolve<SettingsPrivacyAndSecurityViewModel>(id);
                case SettingsPasswordPage settingsSecurityEnterPassword:
                    return container.Resolve<SettingsPasswordViewModel>(id);
                case SettingsPasscodePage settingsSecurityPasscode:
                    return container.Resolve<SettingsPasscodeViewModel>(id);
                case SettingsSessionsPage settingsSessions:
                    return container.Resolve<SettingsSessionsViewModel>(id);
                case SettingsNetworkPage settingsNetwork:
                    return container.Resolve<SettingsNetworkViewModel>(id);
                case SettingsStickersArchivedPage settingsStickersArchived:
                    return container.Resolve<SettingsStickersArchivedViewModel>(id);
                case SettingsStickersFeaturedPage settingsStickersTrending:
                    return container.Resolve<SettingsStickersTrendingViewModel>(id);
                case SettingsStickersPage settingsStickers:
                    return container.Resolve<SettingsStickersViewModel>(id);
                case SettingsStoragePage settingsStorage:
                    return container.Resolve<SettingsStorageViewModel>(id);
                case SettingsUsernamePage settingsUsername:
                    return container.Resolve<SettingsUsernameViewModel>(id);
                case SettingsWallpaperPage settingsWallPaper:
                    return container.Resolve<SettingsWallpaperViewModel>(id);
                case SettingsWebSessionsPage settingsWebSessions:
                    return container.Resolve<SettingsWebSessionsViewModel>(id);
                case SettingsPage settings:
                    return container.Resolve<SettingsViewModel, ISettingsDelegate>(settings, id);
                case SignInPage signIn:
                    return container.Resolve<SignInViewModel>(id);
                case SignInPasswordPage signInPassword:
                    return container.Resolve<SignInPasswordViewModel>(id);
                case SignInSentCodePage signInSentCode:
                    return container.Resolve<SignInSentCodeViewModel>(id);
                case SignUpPage signUp:
                    return container.Resolve<SignUpViewModel>(id);
                case SupergroupAddAdministratorPage supergroupAddAdministrator:
                    return container.Resolve<SupergroupAddAdministratorViewModel>(id);
                case SupergroupAddRestrictedPage supergroupAddRestricted:
                    return container.Resolve<SupergroupAddRestrictedViewModel>(id);
                case SupergroupAdministratorsPage supergroupAdministrators:
                    return container.Resolve<SupergroupAdministratorsViewModel, ISupergroupDelegate>(supergroupAdministrators, id);
                case SupergroupBannedPage supergroupBanned:
                    return container.Resolve<SupergroupBannedViewModel, ISupergroupDelegate>(supergroupBanned, id);
                case SupergroupEditAdministratorPage supergroupEditAdministrator:
                    return container.Resolve<SupergroupEditAdministratorViewModel, IMemberDelegate>(supergroupEditAdministrator, id);
                case SupergroupEditPage supergroupEdit:
                    return container.Resolve<SupergroupEditViewModel, ISupergroupDelegate>(supergroupEdit, id);
                case SupergroupEditRestrictedPage supergroupEditRestricted:
                    return container.Resolve<SupergroupEditRestrictedViewModel, IMemberDelegate>(supergroupEditRestricted, id);
                case SupergroupEditStickerSetPage supergroupEditStickerSet:
                    return container.Resolve<SupergroupEditStickerSetViewModel>(id);
                case SupergroupEventLogPage supergroupEventLog:
                    return container.Resolve<SupergroupEventLogViewModel>(id);
                case SupergroupMembersPage supergroupMembers:
                    return container.Resolve<SupergroupMembersViewModel, ISupergroupDelegate>(supergroupMembers, id);
                case SupergroupRestrictedPage supergroupRestricted:
                    return container.Resolve<SupergroupRestrictedViewModel, ISupergroupDelegate>(supergroupRestricted, id);
                case UserCommonChatsPage userCommonChats:
                    return container.Resolve<UserCommonChatsViewModel>(id);
                case UserCreatePage userCreate:
                    return container.Resolve<UserCreateViewModel>(id);
            }

            return base.ResolveForPage(page, navigationService);
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            //var flowDirectionSetting = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues["LayoutDirection"];

            //args.Window.CoreWindow.FlowDirection = flowDirectionSetting == "RTL" ? CoreWindowFlowDirection.RightToLeft : CoreWindowFlowDirection.LeftToRight;

            CustomXamlResourceLoader.Current = new XamlResourceLoader();
            base.OnWindowCreated(args);
        }

        protected override WindowContext CreateWindowWrapper(Window window)
        {
            return new TLWindowContext(window, 0);
        }

        private void Inactivity_Detected(object sender, EventArgs e)
        {
            WindowContext.Default().Dispatcher.Dispatch(() =>
            {
                var passcode = TLContainer.Current.Resolve<IPasscodeService>();
                if (passcode != null && UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse)
                {
                    passcode.Lock();
                    ShowPasscode();
                }
            });
        }

        private static volatile bool _passcodeShown;
        public static async void ShowPasscode()
        {
            if (_passcodeShown)
            {
                return;
            }

            _passcodeShown = true;

            var dialog = new PasscodePage();
            var result = await dialog.ShowQueuedAsync();

            _passcodeShown = false;
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs e)
        {
            HandleActivated(e.WindowActivationState != CoreWindowActivationState.Deactivated);
            SettingsService.Current.Appearance.UpdateTimer();
        }

        private void Window_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            HandleActivated(e.Visible);

            if (e.Visible && TLContainer.Current.Passcode.IsLockscreenRequired)
            {
                ShowPasscode();
            }
            else
            {
                if (UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Touch)
                {
                    TLContainer.Current.Passcode.CloseTime = DateTime.Now;
                }
                else
                {
                    TLContainer.Current.Passcode.CloseTime = DateTime.MaxValue;
                }
            }

#if !DEBUG && !PREVIEW
            if (e.Visible)
            {
                var dispatcher = Window.Current.Dispatcher;
                Execute.BeginOnThreadPool(async () =>
                {
                    await new HockeyAppUpdateService().CheckForUpdatesAsync(Constants.HockeyAppId, dispatcher);
                });
            }
#endif
        }

        private void HandleActivated(bool active)
        {
            var aggregator = TLContainer.Current.Resolve<IEventAggregator>();
            if (aggregator != null)
            {
                aggregator.Publish(active ? "Window_Activated" : "Window_Deactivated");
            }

            var cacheService = TLContainer.Current.Resolve<ICacheService>();
            if (cacheService != null)
            {
                cacheService.Options.Online = active;
            }
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            if (args.TaskInstance.TriggerDetails is ToastNotificationActionTriggerDetail triggerDetail)
            {
                var data = Toast.GetData(triggerDetail);
                if (data == null)
                {
                    return;
                }

                var session = TLContainer.Current.Lifetime.ActiveItem.Id;
                if (data.TryGetValue("session", out string value) && int.TryParse(value, out int result))
                {
                    session = result;
                }

                var deferral = args.TaskInstance.GetDeferral();
                await TLContainer.Current.Resolve<INotificationsService>(session).ProcessAsync(data);
                deferral.Complete();
            }
        }

        public override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            //Locator.Configure();
            //UnigramContainer.Current.ResolveType<IGenerationService>();

            if (TLContainer.Current.Passcode.IsEnabled)
            {
                TLContainer.Current.Passcode.Lock();
                InactivityHelper.Initialize(TLContainer.Current.Passcode.AutolockTimeout);
            }

            if (Window.Current != null)
            {
                Window.Current.Activated -= Window_Activated;
                Window.Current.Activated += Window_Activated;
                Window.Current.VisibilityChanged -= Window_VisibilityChanged;
                Window.Current.VisibilityChanged += Window_VisibilityChanged;

                TLWindowContext.GetForCurrentView().UpdateTitleBar();

                Theme.Current.Update();
                //NotifyThemeChanged();
            }

            return base.OnInitializeAsync(args);
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            if (startKind == StartKind.Activate)
            {
                var lifetime = TLContainer.Current.Lifetime;
                var sessionId = lifetime.ActiveItem.Id;

                var id = Toast.GetSession(args);
                if (id != null)
                {
                    lifetime.ActiveItem = lifetime.Items.FirstOrDefault(x => x.Id == id.Value) ?? lifetime.ActiveItem;
                }

                if (sessionId != TLContainer.Current.Lifetime.ActiveItem.Id)
                {
                    var root = Window.Current.Content as RootPage;
                    root.Switch(lifetime.ActiveItem);
                }
            }

            var navService = WindowContext.GetForCurrentView().NavigationServices.GetByFrameId($"{TLContainer.Current.Lifetime.ActiveItem.Id}");
            var service = TLContainer.Current.Resolve<IProtoService>();
            if (service == null)
            {
                return;
            }

            var state = service.GetAuthorizationState();
            if (state == null)
            {
                return;
            }

            TLWindowContext.GetForCurrentView().SetActivatedArgs(args, navService);
            TLWindowContext.GetForCurrentView().UpdateTitleBar();

            Window.Current.Activated -= Window_Activated;
            Window.Current.Activated += Window_Activated;
            Window.Current.VisibilityChanged -= Window_VisibilityChanged;
            Window.Current.VisibilityChanged += Window_VisibilityChanged;

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 500));
            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            //Theme.Current.Update();
            //NotifyThemeChanged();

            var dispatcher = Window.Current.Dispatcher;
            Task.Run(() => OnStartSync(dispatcher));
            //return Task.CompletedTask;
        }

        public override UIElement CreateRootElement(IActivatedEventArgs e)
        {
            var id = Toast.GetSession(e);
            if (id != null)
            {
                TLContainer.Current.Lifetime.ActiveItem = TLContainer.Current.Lifetime.Items.FirstOrDefault(x => x.Id == id.Value) ?? TLContainer.Current.Lifetime.ActiveItem;
            }

            var sessionId = TLContainer.Current.Lifetime.ActiveItem.Id;

            if (e is ContactPanelActivatedEventArgs /*|| (e is ProtocolActivatedEventArgs protocol && protocol.Uri.PathAndQuery.Contains("domain=telegrampassport", StringComparison.OrdinalIgnoreCase))*/)
            {
                var navigationFrame = new Frame { FlowDirection = ApiInfo.FlowDirection };
                var navigationService = NavigationServiceFactory(BackButton.Ignore, ExistingContent.Include, navigationFrame, sessionId, $"Main{sessionId}", false) as NavigationService;
                navigationService.SerializationService = TLSerializationService.Current;

                return navigationFrame;
            }
            else
            {
                var navigationFrame = new Frame();
                var navigationService = NavigationServiceFactory(BackButton.Ignore, ExistingContent.Include, navigationFrame, sessionId, $"{sessionId}", true) as NavigationService;
                navigationService.SerializationService = TLSerializationService.Current;

                return new RootPage(navigationService) { FlowDirection = ApiInfo.FlowDirection };
            }
        }

        public override UIElement CreateRootElement(Frame frame)
        {
            return new StandalonePage(frame) { FlowDirection = ApiInfo.FlowDirection };
        }

        protected override INavigationService CreateNavigationService(Frame frame, int session, string id, bool root)
        {
            if (root)
            {
                return new TLRootNavigationService(TLContainer.Current.Resolve<ISessionService>(session), frame, session, id);
            }

            return new TLNavigationService(TLContainer.Current.Resolve<IProtoService>(session), frame, session, id);
        }

        private async void OnStartSync(CoreDispatcher dispatcher)
        {
            //#if DEBUG
            //await VoIPConnection.Current.ConnectAsync();
            //#endif

            await Toast.RegisterBackgroundTasks();

            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication("App").Clear();
            }
            catch { }

            try
            {
                ToastNotificationManager.History.Clear("App");
            }
            catch { }

#if !DEBUG && !PREVIEW
            Execute.BeginOnThreadPool(async () =>
            {
                await new HockeyAppUpdateService().CheckForUpdatesAsync(Constants.HockeyAppId, dispatcher);
            });
#endif

            //if (ApiInformation.IsTypePresent("Windows.ApplicationModel.FullTrustProcessLauncher"))
            //{
            //    await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            //}

            try
            {
                // Prepare stuff for Cortana
                var localVoiceCommands = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceCommands/VoiceCommands.xml"));
                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(localVoiceCommands);
            }
            catch { }
        }

        public override async void OnResuming(object s, object e, AppExecutionState previousExecutionState)
        {
            Logs.Log.Write("OnResuming");

            //#if DEBUG
            //await VoIPConnection.Current.ConnectAsync();
            //#endif

            base.OnResuming(s, e, previousExecutionState);
        }

        public override Task OnSuspendingAsync(object s, SuspendingEventArgs e, bool prelaunchActivated)
        {
            Logs.Log.Write("OnSuspendingAsync");

            return base.OnSuspendingAsync(s, e, prelaunchActivated);
        }


        //private void Window_Activated(object sender, WindowActivatedEventArgs e)
        //{
        //    ((SolidColorBrush)Resources["TelegramTitleBarBackgroundBrush"]).Color = e.WindowActivationState != CoreWindowActivationState.Deactivated ? ((SolidColorBrush)Resources["TelegramTitleBarBackgroundBrushBase"]).Color : ((SolidColorBrush)Resources["TelegramTitleBarBackgroundBrushDeactivated"]).Color;
        //}

        //public static void NotifyThemeChanged()
        //{
        //    var frame = Window.Current.Content as Frame;
        //    if (frame == null)
        //    {
        //        return;
        //    }

        //    var current = App.Current as App;
        //    var theme = current.UISettings.GetColorValue(UIColorType.Background);

        //    frame.RequestedTheme = SettingsService.Current.Appearance.CurrentTheme.HasFlag(TelegramTheme.Dark) || (SettingsService.Current.Appearance.CurrentTheme.HasFlag(TelegramTheme.Default) && theme.R == 0 && theme.G == 0 && theme.B == 0) ? ElementTheme.Light : ElementTheme.Dark;
        //    //frame.RequestedTheme = ApplicationSettings.Current.CurrentTheme;

        //    //var dark = (bool)App.Current.Resources["IsDarkTheme"];

        //    //frame.RequestedTheme = dark ? ElementTheme.Light : ElementTheme.Dark;
        //    //frame.RequestedTheme = ElementTheme.Default;
        //}
    }
}
