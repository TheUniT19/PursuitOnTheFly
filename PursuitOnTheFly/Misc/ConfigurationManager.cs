using PursuitOnTheFly.Misc;
using Rage;
using System.Windows.Forms;

namespace PursuitOnTheFly
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
    internal static class ConfigurationManager
    {
        internal static ConfigurationContainer GetConfiguration()
        {
            Extensions.LogTrivial($"Attempting to load PursuitOnTheFly.ini");
            var ini = GetInitializationFile();
            var config = ReadFromIni(ini);
            Extensions.LogTrivial($"Finished loading configuration");

            return config;
        }

        private static InitializationFile GetInitializationFile()
        {
            var ini = new InitializationFile("Plugins/LSPDFR/PursuitOnTheFly.ini");
            ini.Create();
            return ini;
        }
        private static unsafe ConfigurationContainer ReadFromIni(InitializationFile initializationFile)
        {
            KeysConverter kc = new KeysConverter();

            Keys forcePursuitKey;
            Keys forcePursuitModifier;
            bool allowInvestigation;
            
            try
            {
                Extensions.LogTrivial($"Attempting to read ForcePursuit keybinding");
                forcePursuitKey = (Keys)kc.ConvertFromString(initializationFile.ReadString("Keybindings", "ForcePursuit", "T"));
                Extensions.LogTrivial($"Successfully read ForcePursuit key ({forcePursuitKey})");
            }
            catch
            {
                forcePursuitKey = Keys.T;
                var log = $"Pursuit on the Fly: There was an error reading ForcePursuit keybinding from PursuitsOnTheFly.ini, make sure your preferred key is valid. Applying default key ({forcePursuitKey}).";
                Game.DisplayNotification(log);
                Game.LogTrivial(log);
            }

            try
            {
                forcePursuitModifier = (Keys)kc.ConvertFromString(initializationFile.ReadString("Keybindings", "ForcePursuitModifier", "None"));
            }
            catch
            {
                forcePursuitModifier = Keys.None;
                var log = "Pursuit on the Fly: There was an error reading ForcePursuitModifier keybinding from PursuitOnTheFly.ini, make sure your preferred modifier key is valid. Applying no modifier key.";
                Game.DisplayNotification(log);
                Game.LogTrivial(log);
            }
            
            try
            {
                allowInvestigation = initializationFile.ReadBoolean("General", "AllowInvestigativeMode", true);
            }
            catch
            {
                allowInvestigation = false;
                var log = "Pursuit on the Fly: There was an error reading the AllowInvestigativeMode setting from PursuitOnTheFly.ini, make sure the value is valid.";
                Game.DisplayNotification(log);
                Game.LogTrivial(log);
            }

            return new ConfigurationContainer(allowInvestigation, forcePursuitKey, forcePursuitModifier);
        }
    }
    internal class ConfigurationContainer
    {
        internal ConfigurationContainer(bool allowInvestigation, Keys forcePursuitKey, Keys forcePursuitModifierKey)
        {
            AllowInvestigation = allowInvestigation;
            ForcePursuitKey = forcePursuitKey;
            ForcePursuitModifierKey = forcePursuitModifierKey;
        }

        internal bool AllowInvestigation { get; }
        internal Keys ForcePursuitKey { get; }
        internal Keys ForcePursuitModifierKey { get; }
    }
}
