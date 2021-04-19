using Rage;
using System.Windows.Forms;

namespace PursuitOnTheFly
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
    public static class MainLoop
    {
        public static void NoModifier(Keys keyForcePursuit)
        {
            //This loop runs until it's broken
            while (true)
            {
                //If our key has been pressed
                if (Game.IsKeyDown(keyForcePursuit))
                {
                    Game.LogTrivial($"Pursuit on the Fly: Key {keyForcePursuit} pressed");
                    Logic.MainLogic();
                }
                //Let other GameFibers do their job by sleeping this one for a bit.
                GameFiber.Yield();
            }
        }
        public static void WithModifier(Keys keyForcePursuit, Keys modForcePursuit)
        {
            //This loop runs until it's broken
            while (true)
            {
                //If our key has been pressed

                if (Game.IsKeyDownRightNow(modForcePursuit))
                {
                    if (Game.IsKeyDown(keyForcePursuit))
                    {
                        Game.LogTrivial($"Pursuit on the Fly: Modifier {modForcePursuit} and key {keyForcePursuit} pressed");
                        Logic.MainLogic();
                    }
                }
                //Let other GameFibers do their job by sleeping this one for a bit.
                GameFiber.Yield();
            }
        }
    }
}