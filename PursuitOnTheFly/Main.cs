namespace PursuitOnTheFly
{
    using LSPD_First_Response.Mod.API;
    using PursuitOnTheFly.Misc;
    using PursuitOnTheFly.PotfLogic;
    using Rage;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
    public class Main : Plugin
    {
        public override void Finally()
        {
            Extensions.LogTrivial($"Unloaded");
        }
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += Functions_OnDutyStateChanged;
        }
        private static void Functions_OnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                var config = ConfigurationManager.GetConfiguration();
                InputManager.Start(config);
                Extensions.LogTrivial($"Loaded");
            }
            else
            {
                InputManager.Stop();
                Extensions.LogTrivial($"Ended");
            }
        }
    }
}
