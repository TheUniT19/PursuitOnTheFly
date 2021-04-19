using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PursuitOnTheFly
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
    public static class Logic
    {
        //private static readonly List<Ped> chasedPeds = new List<Ped>();
        public static void MainLogic()
        {            
            var entity = GetEntityPlayerIsAimingAt();
            if (entity is null)
            {
                return;
            }
            if (entity is Ped)
            {
                Game.LogTrivial($"Pursuit on the Fly: Entity is Ped.");
                DoPedStuff((Ped)entity);
                Game.LogTrivial("Pursuit on the Fly: TASK FINISHED.");
                return;
            }
            if (entity is Vehicle)
            {
                Game.LogTrivial($"Pursuit on the Fly: Entity is Vehicle.");
                DoVehicleStuff((Vehicle)entity);
                Game.LogTrivial("Pursuit on the Fly: TASK FINISHED.");
                return;
            }
            Game.LogTrivial($"Pursuit on the Fly: Entity is {entity.GetType()}, what the hell are you trying to do??? END");
        }

        private static Entity GetEntityPlayerIsAimingAt()
        {
            try
            {
                Game.LogTrivial("Pursuit on the Fly: Attempting to get entity aimed at.");
                unsafe
                {
                    uint entityHandle;
                    NativeFunction.Natives.x2975C866E6713290(Game.LocalPlayer, new IntPtr(&entityHandle)); // Stores the entity the player is aiming at in the uint provided in the second parameter.
                    var entPlayerAimingAt = World.GetEntityByHandle<Entity>(entityHandle);
                    Game.LogTrivial("Pursuit on the Fly: Succesfully got entity");
                    return entPlayerAimingAt;
                }
            }
            catch
            {
                Game.LogTrivial("Pursuit on the Fly: The player was not aiming at an entity. END");
                return null;
            }
        }

        private static void DoPedStuff(Ped ped)
        {
            Game.LogTrivial("Pursuit on the Fly: Doing ped specific actions.");
            if (IsPedValid(ped))
            {
                if (ped.IsInAnyVehicle(atGetIn: false))
                {
                    Game.LogTrivial("Pursuit on the Fly: Ped is in vehicle, doing vehicle specific actions.");
                    DoVehicleStuff(ped.CurrentVehicle);
                }
                else
                {
                    DoPursuitStuff(ped);
                    Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS OFFICERS_REPORT CRIME_RESIST_ARREST_04 IN_OR_ON_POSITION INTRO_1 UNITS_RESPOND_CODE_03", ped.Position);
                }
            }
        }

        private static void DoVehicleStuff(Vehicle vehicle)
        {
            Game.LogTrivial("Pursuit on the Fly: Doing vehicle specific actions.");
            if (IsVehicleValid(vehicle))
            {
                var validPeds = vehicle.Occupants.Where(occupant => IsPedValid(occupant));
                if (validPeds.Any())
                {
                    Game.LogTrivial("Pursuit on the Fly: Valid peds in vehicle.");
                    Game.LogTrivial("Pursuit on the Fly: Checking if vehicle is pulled over.");
                    var currentPullover = Functions.GetCurrentPullover();
                    if (!(currentPullover is null) && Functions.GetPulloverSuspect(currentPullover).CurrentVehicle == vehicle)
                    {
                        Game.LogTrivial("Pursuit on the Fly: Vehicle is pulled over, forcing to end.");
                        Functions.ForceEndCurrentPullover();
                    }

                    Game.LogTrivial("Pursuit on the Fly: Starting pursuit for valid peds.");
                    foreach (var ped in validPeds)
                    {
                        DoPursuitStuff(ped);
                    }

                    Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS OFFICERS_REPORT CRIME_RESIST_ARREST_04 IN_OR_ON_POSITION INTRO_1 UNITS_RESPOND_CODE_03", vehicle.Position);
                }
            }
        }

        private static void DoPursuitStuff(Ped ped)
        {
            //var isInPursuit = Functions.IsPedInPursuit(ped);
            //Game.LogTrivial($"Pursuit on the Fly: Ped in pursuit - {isInPursuit}");
            //if (!isInPursuit)
            //{
                Game.LogTrivial("Pursuit on the Fly: Getting active pursuit or creating one.");
                var pursuit = Functions.GetActivePursuit() ?? Functions.CreatePursuit();
                Game.LogTrivial("Pursuit on the Fly: Pursuit successful, adding ped to pursuit.");
                Functions.AddPedToPursuit(pursuit, ped);
                Game.LogTrivial("Pursuit on the Fly: Ped successfully added to pursuit, setting active for player.");
                Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                Game.LogTrivial("Pursuit on the Fly: Pursuit is active for player.");            
            //}

            //var previouslyTagged = chasedPeds.Any(p => p.Equals(ped));
            //Game.LogTrivial($"Pursuit on the Fly: Ped previously tagged - {previouslyTagged}");

            //if (!previouslyTagged && !isInPursuit)
            //{
            //    chasedPeds.Add(ped);
            //    Functions.SetPursuitDisableAIForPed(ped, true);                
            //}
            //else if (previouslyTagged)
            //{
            //    Functions.SetPursuitDisableAIForPed(ped, false);
            //}
        }

        private static bool IsPedValid(Ped ped)
        {
            var result =
                ped.IsHuman &&
                ped.IsAlive &&
                !Functions.IsPedInPursuit(ped) &&
                !Functions.IsPedACop(ped) &&
                !Functions.IsPedArrested(ped) &&
                !Functions.IsPedBeingCuffed(ped) &&
                !Functions.IsPedBeingCuffedByPlayer(ped) &&
                !Functions.IsPedBeingGrabbed(ped) &&
                !Functions.IsPedBeingGrabbedByPlayer(ped) &&
                !Functions.IsPedGettingArrested(ped) &&
                !Functions.IsPedInPrison(ped);

            var end = (result) ? "" : " For this Ped: END";
            Game.LogTrivial($"Pursuit on the Fly: Ped is valid -- {result}.{end}");

            return result;
        }

        private static bool IsVehicleValid(Vehicle vehicle)
        {
            //Explicitly allowed:
            //var partialResult =
            //    vehicle.HasOccupants && (
            //    vehicle.Model.IsCar ||
            //    vehicle.Model.IsAmphibiousCar ||
            //    vehicle.Model.IsAmphibiousQuadBike ||
            //    vehicle.Model.IsBicycle ||
            //    vehicle.Model.IsBigVehicle ||
            //    vehicle.Model.IsBike ||
            //    vehicle.Model.IsBus ||
            //    vehicle.Model.IsQuadBike);

            var result =
                vehicle.HasOccupants &&
                // prevent forcing of pursuit if any occupant is already in a pursuit
                // (i.e. in case of a kidnapping callout)
                !vehicle.Occupants.Any(p => Functions.IsPedInPursuit(p)) &&
                !vehicle.IsHelicopter &&
                !vehicle.IsPlane &&
                !vehicle.IsSubmarine &&
                !vehicle.IsTrailer &&
                !vehicle.IsTrain;

            var end = (result) ? "" : " END";
            Game.LogTrivial($"Pursuit on the Fly: Vehicle is valid -- {result}.{end}");

            return result;
        }
    }
}
