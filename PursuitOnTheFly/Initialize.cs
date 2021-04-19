using Rage;
using System.Windows.Forms;

namespace PursuitOnTheFly
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
    internal static class Initialize
    {
        private static InitializationFile Initialization { get; set; }
        //In this method, we load up the .ini file so other methods can use it.
        private static void InitialiseFile()
        {
            Game.LogTrivial("Pursuit on the Fly: Attempting to load PursuitOnTheFly.ini");
            Initialization = new InitializationFile("Plugins/LSPDFR/PursuitOnTheFly.ini");
            Initialization.Create();
            Game.LogTrivial("Pursuit on the Fly: ini loaded successfully");
        }
        //In this method, we load up the ini file with the method above and we read one of the values: in this case, a keybinding.

        internal static unsafe void MainMethod()
        {
            //Load the ini first.
            InitialiseFile();
            //A keysconverter is used to convert a string to a key.
            KeysConverter kc = new KeysConverter();

            Keys keyForcePursuit;
            Keys modForcePursuit;

            try
            {
                Game.LogTrivial("Pursuit on the Fly: Attempting to read ForcePursuit keybinding");
                keyForcePursuit = (Keys)kc.ConvertFromString(Initialization.ReadString("Keybindings", "ForcePursuit", "T"));
                Game.LogTrivial($"Pursuit on the Fly: Successfully read ForcePursuit key ({keyForcePursuit})");
            }
            catch
            {
                keyForcePursuit = Keys.T;
                var log = $"Pursuit on the Fly: There was an error reading ForcePursuit keybinding from PursuitsOnTheFly.ini, make sure your preferred key is valid. Applying default key ({keyForcePursuit}).";
                Game.DisplayNotification(log);
                Game.LogTrivial(log);
            }

            try
            {
                modForcePursuit = (Keys)kc.ConvertFromString(Initialization.ReadString("Keybindings", "ForcePursuitModifier", "None"));
            }
            catch
            {
                modForcePursuit = Keys.None;
                var log = "Pursuit on the Fly: There was an error reading ForcePursuitModifier keybinding from PursuitOnTheFly.ini, make sure your preferred modifier key is valid. Applying no modifier key.";
                Game.DisplayNotification(log);
                Game.LogTrivial(log);
            }

            if (modForcePursuit != Keys.None)
            {
                Game.LogTrivial("Pursuit on the Fly: Modifier key exists.");
                GameFiber.StartNew(delegate
                {
                    MainLoop.WithModifier(keyForcePursuit, modForcePursuit);
                });
            }
            else
            {
                Game.LogTrivial("Pursuit on the Fly: No modifier key exists.");
                GameFiber.StartNew(delegate
                {
                    MainLoop.NoModifier(keyForcePursuit);
                });
            }
        }
    }
}