using System;

using Foundation;
using LocalAuthentication;
using UIKit;

namespace OpFlow.iOS.Helpers
{
    public static class LocalAuthHelper
    {
        private enum LocalAuthType
        {
            None,
            Passcode,
            TouchId,
            FaceId
        }

        public static bool IsLocalAuthAvailable => GetLocalAuthType() != LocalAuthType.None;

        public static void Authenticate(Action onSuccess, Action onFailure){
            var context = new LAContext();

            if (context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out NSError authError)
               || context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, out authError))
            {
                var replyHandler = new LAContextReplyHandler((success, error) =>
                {
                    if (success)
                        onSuccess?.Invoke();
                    else
                        onFailure?.Invoke();
                });

                context.EvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, "Please authenticate to proceed", replyHandler);
            }
        }

        private static LocalAuthType GetLocalAuthType()
        {
            var localAuthContext = new LAContext();

            if (localAuthContext.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, out NSError authError))
            {
                if (localAuthContext.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out authError))
                {
                    if (GetOsMajorVersion() >= 11 && localAuthContext.BiometryType == LABiometryType.FaceId)
                    {
                        return LocalAuthType.FaceId;
                    }

                    return LocalAuthType.TouchId;
                }

                return LocalAuthType.Passcode;
            }

            return LocalAuthType.None;
        }

        private static int GetOsMajorVersion()
        {
            return int.Parse(UIDevice.CurrentDevice.SystemVersion.Split('.')[0]);
        }
    }
}
