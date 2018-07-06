using System;
using Foundation;

namespace OpFlow.iOS
{
    public abstract class Settings
    {
        const string Environment = "PRODUCTION_ENVIRONMENT";

        private static bool? _productionEnvironment;
        public Settings()
        {
        }

        public static bool ProductionEnvironment
        {
            get 
            {
                if (_productionEnvironment == null){
                    LoadDefaultValues();
                }

                return _productionEnvironment ?? true;
            }
        }

        private static void LoadDefaultValues()
        {
            var productionEnvironment = NSUserDefaults.StandardUserDefaults.BoolForKey(Environment);

            _productionEnvironment = productionEnvironment;
        }
    }
}
