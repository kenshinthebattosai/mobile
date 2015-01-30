using System.Collections.Generic;
using Bugsnag;
using Foundation;
using Toggl.Phoebe;
using Toggl.Phoebe.Analytics;
using Toggl.Phoebe.Data;
using Toggl.Phoebe.Logging;
using Toggl.Phoebe.Net;
using Toggl.Ross.Analytics;
using Toggl.Ross.Data;
using Toggl.Ross.Logging;
using Toggl.Ross.Net;
using Toggl.Ross.ViewControllers;
using Toggl.Ross.Views;
using UIKit;
using XPlatUtils;

namespace Toggl.Ross
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    [Register ("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate, IPlatformInfo
    {
        private TogglWindow window;
        private bool isResuming;

        public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
        {
            RegisterComponents ();

            var signIn = Google.Plus.SignIn.SharedInstance;
            signIn.ClientId = Build.GoogleOAuthClientId;
            signIn.Scopes = new [] {
                "https://www.googleapis.com/auth/userinfo.profile",
                "https://www.googleapis.com/auth/userinfo.email",
            };

            Toggl.Ross.Theme.Style.Initialize ();

            // Start app
            window = new TogglWindow (UIScreen.MainScreen.Bounds);
            window.RootViewController = new MainViewController ();
            window.MakeKeyAndVisible ();

            // Make sure critical services are running are running:
            ServiceContainer.Resolve<UpgradeManger> ().TryUpgrade ();
            ServiceContainer.Resolve<IBugsnagClient> ();
            ServiceContainer.Resolve<BugsnagUserManager> ();

            ServiceContainer.Resolve<ITracker> ();

            return true;
        }

        public override void OnActivated (UIApplication application)
        {
            // Make sure the user data is refreshed when the application becomes active
            ServiceContainer.Resolve<ISyncManager> ().Run ();
            ServiceContainer.Resolve<NetworkIndicatorManager> ();
            ServiceContainer.Resolve<WidgetSyncManager>();

            isResuming = true;
        }

        public override bool OpenUrl (UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            return Google.Plus.UrlHandler.HandleUrl (url, sourceApplication, annotation);
        }

        private void RegisterComponents ()
        {
            Services.Register ();

            // Override default implementation
            ServiceContainer.Register<ITimeProvider> (() => new NSTimeProvider ());

            // Register Ross components:
            ServiceContainer.Register<IPlatformInfo> (this);
            ServiceContainer.Register<ILogger> (() => new Logger ());
            ServiceContainer.Register<SettingsStore> ();
            ServiceContainer.Register<ISettingsStore> (() => ServiceContainer.Resolve<SettingsStore> ());
            ServiceContainer.Register<IWidgetUpdateService> (() => new WidgetUpdateService());
            ServiceContainer.Register<ExperimentManager> (() => new ExperimentManager (
                typeof (Toggl.Phoebe.Analytics.Experiments),
                typeof (Toggl.Ross.Analytics.Experiments)));
            ServiceContainer.Register<IBugsnagClient> (delegate {
                return new BugsnagClient (Build.BugsnagApiKey) {
                    DeviceId = ServiceContainer.Resolve<SettingsStore> ().InstallId,
                    ProjectNamespaces = new List<string> () { "Toggl." },
                    NotifyReleaseStages = new List<string> () { "production" },
                    #if DEBUG
                    ReleaseStage = "development",
                    #else
                    ReleaseStage = "production",
                    #endif
                };
            });
            ServiceContainer.Register<ITracker> (() => new Tracker());
            ServiceContainer.Register<INetworkPresence> (() => new NetworkPresence ());
            ServiceContainer.Register<NetworkIndicatorManager> ();
            ServiceContainer.Register<TagChipCache> ();
        }

        public static TogglWindow TogglWindow
        {
            get {
                return ((AppDelegate)UIApplication.SharedApplication.Delegate).window;
            }
        }

        string IPlatformInfo.AppIdentifier
        {
            get { return Build.AppIdentifier; }
        }

        private string appVersion;

        string IPlatformInfo.AppVersion
        {
            get {
                if (appVersion == null) {
                    appVersion = NSBundle.MainBundle.InfoDictionary.ObjectForKey (
                                     new NSString ("CFBundleShortVersionString")).ToString ();
                }
                return appVersion;
            }
        }
    }
}
