using Harmony;
using System.Reflection;
using System;
using UnityEngine;
using MoreEffectiveTransfer.CustomAI;
using MoreEffectiveTransfer.CustomManager;

namespace MoreEffectiveTransfer.Util
{
    internal static class HarmonyDetours
    {
        private static void ConditionalPatch(this HarmonyInstance harmony, MethodBase method, HarmonyMethod prefix, HarmonyMethod postfix)
        {
            var fullMethodName = string.Format("{0}.{1}", method.ReflectedType?.Name ?? "(null)", method.Name);
            if (harmony.GetPatchInfo(method)?.Owners?.Contains(harmony.Id) == true)
            {
                DebugLog.LogToFileOnly("Harmony patches already present for {0}" + fullMethodName.ToString());
            }
            else
            {
                DebugLog.LogToFileOnly("Patching {0}..." + fullMethodName.ToString());
                harmony.Patch(method, prefix, postfix);
            }
        }

        private static void ConditionalUnPatch(this HarmonyInstance harmony, MethodBase method, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
        {
            var fullMethodName = string.Format("{0}.{1}", method.ReflectedType?.Name ?? "(null)", method.Name);
            if (prefix != null)
            {
                DebugLog.LogToFileOnly("UnPatching Prefix{0}..." + fullMethodName.ToString());
                harmony.Unpatch(method, HarmonyPatchType.Prefix);
            }
            if (postfix != null)
            {
                DebugLog.LogToFileOnly("UnPatching Postfix{0}..." + fullMethodName.ToString());
                harmony.Unpatch(method, HarmonyPatchType.Postfix);
            }
        }

        public static void Apply()
        {
            var harmony = HarmonyInstance.Create("MoreEffectiveTransfer");
            //1
            var carAIPathfindFailure = typeof(CarAI).GetMethod("PathfindFailure", BindingFlags.NonPublic | BindingFlags.Instance);
            var carAIPathfindFailurePostFix = typeof(CustomCarAI).GetMethod("CarAIPathfindFailurePostFix");
            harmony.ConditionalPatch(carAIPathfindFailure,
                null,
                new HarmonyMethod(carAIPathfindFailurePostFix));
            //2
            var transferManagerAddOutgoingOffer = typeof(TransferManager).GetMethod("AddOutgoingOffer", BindingFlags.Public | BindingFlags.Instance);
            var transferManagerAddOutgoingOfferPrefix = typeof(CustomTransferManager).GetMethod("TransferManagerAddOutgoingOfferPrefix");
            harmony.ConditionalPatch(transferManagerAddOutgoingOffer,
                new HarmonyMethod(transferManagerAddOutgoingOfferPrefix),
                null);
            //3
            var helicopterDepotAISimulationStep = typeof(HelicopterDepotAI).GetMethod("SimulationStep", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Building.Frame).MakeByRefType() }, null);
            var helicopterDepotAISimulationStepPostFix = typeof(CustomHelicopterDepotAI).GetMethod("HelicopterDepotAISimulationStepPostFix");
            harmony.ConditionalPatch(helicopterDepotAISimulationStep,
                null,
                new HarmonyMethod(helicopterDepotAISimulationStepPostFix));
            Loader.HarmonyDetourFailed = false;
            DebugLog.LogToFileOnly("Harmony patches applied");
        }

        public static void DeApply()
        {
            var harmony = HarmonyInstance.Create("MoreEffectiveTransfer");
            //1
            var carAIPathfindFailure = typeof(CarAI).GetMethod("PathfindFailure", BindingFlags.NonPublic | BindingFlags.Instance);
            var carAIPathfindFailurePostFix = typeof(CustomCarAI).GetMethod("CarAIPathfindFailurePostFix");
            harmony.ConditionalUnPatch(carAIPathfindFailure,
                null,
                new HarmonyMethod(carAIPathfindFailurePostFix));
            //2
            var transferManagerAddOutgoingOffer = typeof(TransferManager).GetMethod("AddOutgoingOffer", BindingFlags.Public | BindingFlags.Instance);
            var transferManagerAddOutgoingOfferPrefix = typeof(CustomTransferManager).GetMethod("TransferManagerAddOutgoingOfferPrefix");
            harmony.ConditionalUnPatch(transferManagerAddOutgoingOffer,
                new HarmonyMethod(transferManagerAddOutgoingOfferPrefix),
                null);
            //3
            var helicopterDepotAISimulationStep = typeof(HelicopterDepotAI).GetMethod("SimulationStep", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Building.Frame).MakeByRefType() }, null);
            var helicopterDepotAISimulationStepPostFix = typeof(CustomHelicopterDepotAI).GetMethod("HelicopterDepotAISimulationStepPostFix");
            harmony.ConditionalUnPatch(helicopterDepotAISimulationStep,
                null,
                new HarmonyMethod(helicopterDepotAISimulationStepPostFix));
            DebugLog.LogToFileOnly("Harmony patches DeApplied");
        }
    }
}
