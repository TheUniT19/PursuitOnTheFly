using Rage;

namespace PursuitOnTheFly.Misc
{
    internal static class Extensions
    {
        internal static void LogTrivial(string message) => Game.LogTrivial($"Pursuit on the Fly: {message}");
    }
}
