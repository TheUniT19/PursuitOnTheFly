using PursuitOnTheFly.Misc;
using Rage;
using System.Windows.Forms;

namespace PursuitOnTheFly.PotfLogic
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
    internal static class InputManager
    {
        private static bool CancellationRequested;
        internal static void Start(ConfigurationContainer configuration)
        {
            CancellationRequested = false;
            if (configuration is null)
            {
                Extensions.LogTrivial($"Configuration was NULL, aborting");
                return;
            }
            if (configuration.ForcePursuitModifierKey != Keys.None)
            {
                Extensions.LogTrivial($"Modifier key exists.");
                GameFiber.StartNew(delegate
                {
                    WithModifier(configuration);
                });
            }
            else
            {
                Extensions.LogTrivial($"No modifier key exists.");
                GameFiber.StartNew(delegate
                {
                    NoModifier(configuration);
                });
            }
        }

        internal static void Stop() => CancellationRequested = true;

        private static void NoModifier(ConfigurationContainer configuration)
        {
            while (!CancellationRequested)
            {
                if (Game.IsKeyDown(configuration.ForcePursuitKey))
                {
                    Extensions.LogTrivial($"Key {configuration.ForcePursuitKey} pressed");
                    PursuitOnTheFly.TryForce(configuration);
                }
                GameFiber.Yield();
            }
            Extensions.LogTrivial($"Cancellation requested.");
        }
        private static void WithModifier(ConfigurationContainer configuration)
        {
            while (!CancellationRequested)
            {
                if (Game.IsKeyDownRightNow(configuration.ForcePursuitModifierKey))
                {
                    if (Game.IsKeyDown(configuration.ForcePursuitKey))
                    {
                        Extensions.LogTrivial($"Modifier {configuration.ForcePursuitModifierKey} and key {configuration.ForcePursuitKey} pressed");
                        PursuitOnTheFly.TryForce(configuration);
                    }
                }
                GameFiber.Yield();
            }
            Extensions.LogTrivial($"Cancellation requested.");
        }
    }
}