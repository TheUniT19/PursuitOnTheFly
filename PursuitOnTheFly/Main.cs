namespace PursuitOnTheFly
{
    using LSPD_First_Response.Mod.API;
    using Rage;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
    public class Main : Plugin
    {
        public override void Finally()
        {
            Game.LogTrivial("Pursuit on the Fly: Unloaded");
        }
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += Functions_OnDutyStateChanged;
        }
        private static void Functions_OnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                PursuitOnTheFly.Initialize.MainMethod();
                Game.LogTrivial("Pursuit on the Fly: Loaded");
            }
        }
    }
}
