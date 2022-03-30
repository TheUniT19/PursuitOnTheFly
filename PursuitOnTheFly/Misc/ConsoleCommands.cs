using PursuitOnTheFly.PotfLogic;

namespace PursuitOnTheFly.Misc
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
    public static class ConsoleCommands
    {
        [Rage.Attributes.ConsoleCommand(Description = "Pursuit on the Fly: Reload configuration", Name = "ReloadPotfConfig")]
        public static void ReloadPotfConfig()
        {
            Extensions.LogTrivial($"Config reload requested.");
            InputManager.Stop();
            var config = ConfigurationManager.GetConfiguration();
            InputManager.Start(config);
            Extensions.LogTrivial($"Config reloaded.");
        }
    }
}
