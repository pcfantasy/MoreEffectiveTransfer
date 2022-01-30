/*
 * Inspired by: SmartFireFighters Improved AI
 * by themonthlydaily
 * see: https://github.com/themonthlydaily/Cities-Skylines---Smarter-Firefighters-Improved-AI
 */
using HarmonyLib;
using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

/*
 * STATUS: DISABLED FOR NOW
 * Does not work as intended, garbagetrucks with flag GoingBack ignore new destination, flags WaitingTarget and GoingBack are incompatible and lead to despawning!
 */


namespace MoreEffectiveTransfer.Patch
{

    //// Instruct garbagetrucks to make incoming offers to collect more trash even when returning home
    //[HarmonyPatch(typeof(GarbageTruckAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    //public static class GarbageTruckAISimulationStepPatch
    //{

    //    static AccessTools.FieldRef<GarbageTruckAI, int> m_cargoCapacityRef = AccessTools.FieldRefAccess<GarbageTruckAI, int>("m_cargoCapacity");


    //    public static void Postfix(GarbageTruckAI __instance, ushort vehicleID, ref Vehicle vehicleData)
    //    {
    //        int cargo_cap = (int)(m_cargoCapacityRef(__instance) * 0.7); //70% of total capacity

    //        if (((vehicleData.m_flags & (Vehicle.Flags.GoingBack)) != 0) && (vehicleData.m_transferSize < cargo_cap)) //going back and not full? make incoming offer for more garbage collection!
    //        {
    //            AddOfferIfNotExists(vehicleID, ref vehicleData);
    //        }
    //    }

    //    public static void AddOfferIfNotExists(ushort vehicleID, ref Vehicle vehicleData)
    //    {
    //        // Remove existing offer
    //        TransferManager.TransferOffer transferOffer = new TransferManager.TransferOffer()
    //        {
    //            Priority = 7,
    //            Vehicle = vehicleID,
    //            Position = vehicleData.GetLastFramePosition(),
    //            Amount = 1,
    //            Active = true
    //        };
    //        Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Garbage, transferOffer);

    //        // add offer
    //        Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Garbage, transferOffer);
    //    }

    //} //class GarbageTruckAISimulationStepPatch

}