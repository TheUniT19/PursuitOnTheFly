using LSPD_First_Response.Mod.API;
using PursuitOnTheFly.Misc;
using Rage;
using Rage.Native;
using System;
using System.Linq;

namespace PursuitOnTheFly.PotfLogic
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
    internal static class PursuitOnTheFly
    {
        private static ConfigurationContainer Configuration;
        internal static void TryForce(ConfigurationContainer configuration)
        {
            Configuration = configuration;
            var entity = GetEntityPlayerIsAimingAt();
            if (entity is null)
            {
                return;
            }
            if (entity is Ped)
            {
                Extensions.LogTrivial($"Entity is Ped.");
                ForceOnPed((Ped)entity);
                Extensions.LogTrivial($"TASK FINISHED.");
                return;
            }
            if (entity is Vehicle)
            {
                Extensions.LogTrivial($"Entity is Vehicle.");
                ForceOnVehicle((Vehicle)entity);
                Extensions.LogTrivial($"TASK FINISHED.");
                return;
            }
            Extensions.LogTrivial($"Entity is {entity.GetType()}, what exactly are you trying to do? END");
        }

        private static Entity GetEntityPlayerIsAimingAt()
        {
            try
            {
                Extensions.LogTrivial($"Attempting to get entity aimed at.");
                unsafe
                {
                    uint entityHandle;
                    NativeFunction.Natives.x2975C866E6713290(Game.LocalPlayer, new IntPtr(&entityHandle)); // Stores the entity the player is aiming at in the uint provided in the second parameter.
                    var entPlayerAimingAt = World.GetEntityByHandle<Entity>(entityHandle);
                    Extensions.LogTrivial($"Succesfully got entity");
                    return entPlayerAimingAt;
                }
            }
            catch
            {
                Extensions.LogTrivial($"The player was not aiming at an entity. END");
                return null;
            }
        }

        private static void ForceOnPed(Ped ped)
        {
            Extensions.LogTrivial($"Trying to force pursuit on ped.");
            if (IsPedValid(ped))
            {
                if (ped.IsInAnyVehicle(atGetIn: false))
                {
                    Extensions.LogTrivial($"Ped is in vehicle, doing vehicle specific actions.");
                    ForceOnVehicle(ped.CurrentVehicle);
                }
                else
                {
                    DoForcePursuit(ped);
                    Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS OFFICERS_REPORT CRIME_RESIST_ARREST_04 IN_OR_ON_POSITION INTRO_1 UNITS_RESPOND_CODE_03", ped.Position);
                }
            }
        }

        private static void ForceOnVehicle(Vehicle vehicle)
        {
            Extensions.LogTrivial($"Trying to force pursuit on vehicle.");
            if (IsVehicleValid(vehicle))
            {
                var validPeds = vehicle.Occupants.Where(occupant => IsPedValid(occupant));
                if (validPeds.Any())
                {
                    Extensions.LogTrivial($"Valid peds in vehicle.");
                    Extensions.LogTrivial($"Checking if vehicle is pulled over.");
                    var currentPullover = Functions.GetCurrentPullover();
                    if (!(currentPullover is null) && Functions.GetPulloverSuspect(currentPullover).CurrentVehicle == vehicle)
                    {
                        Extensions.LogTrivial($"Vehicle is pulled over, forcing to end.");
                        Functions.ForceEndCurrentPullover();
                    }
                    Extensions.LogTrivial($"Starting pursuit for valid peds.");
                    foreach (var ped in validPeds)
                    {
                        DoForcePursuit(ped);
                    }

                    Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS OFFICERS_REPORT CRIME_RESIST_ARREST_04 IN_OR_ON_POSITION INTRO_1 UNITS_RESPOND_CODE_03", vehicle.Position);
                }
            }
        }

        private static void DoForcePursuit(Ped ped)
        {
            Extensions.LogTrivial($"Getting active pursuit or creating one.");

            var pursuit = Functions.GetActivePursuit();

            if (pursuit is null)
            {
                pursuit = Functions.CreatePursuit();

                if (Configuration.AllowInvestigation)
                    Functions.SetPursuitInvestigativeMode(pursuit, true);
            }

            Extensions.LogTrivial($"Pursuit successful, adding ped to pursuit.");
            Functions.AddPedToPursuit(pursuit, ped);
            Extensions.LogTrivial($"Ped successfully added to pursuit, setting active for player.");
            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
            Extensions.LogTrivial($"Pursuit is active for player.");
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

            var end = result ? "" : " For this Ped: END";
            Extensions.LogTrivial($"Ped is valid -- {result}.{end}");

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

            var end = result ? "" : " END";
            Extensions.LogTrivial($"Vehicle is valid -- {result}.{end}");

            return result;
        }
    }
}
