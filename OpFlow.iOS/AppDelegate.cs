using System;
using System.Timers;
using Foundation;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        private DateTime _lockoutTime;
        private readonly TimeSpan _inactivitySpan = new TimeSpan(0, 0, 1, 0);

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method

			if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0)) {
				var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
					UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
					new NSSet());

				UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
				UIApplication.SharedApplication.RegisterForRemoteNotifications();
			} else {
				UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
				UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
			}

            return true;
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			// Get current device token
			var token = deviceToken.Description;
			if (!string.IsNullOrWhiteSpace(token)) {
				token = token.Trim('<').Trim('>');
			}

			// Get prvious token
			var oldToken = NSUserDefaults.StandardUserDefaults.StringForKey("PushDeviceToken");

            // Has the token changed?
            if (string.IsNullOrEmpty(oldToken) || !oldToken.Equals(token))
			{
				// TODO: Put logic here to notice server that token has changed / been created
			}

			// Save new device token
			NSUserDefaults.StandardUserDefaults.SetString(token, "PushDeviceToken");
		}

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{
			new UIAlertView("Error registering push notifications", error.LocalizedDescription, null, "OK", null).Show();
		}

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.

            // If application is in background for longer than allowable time, sign out user
            _lockoutTime = DateTime.Now.Add(_inactivitySpan);
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.

            if (_lockoutTime < DateTime.Now)
                AppSettings.SignOutUser();

            _lockoutTime = DateTime.MaxValue;
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }
}