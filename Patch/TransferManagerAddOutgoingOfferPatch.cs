using HarmonyLib;
using System.Reflection;
using MoreEffectiveTransfer.CustomManager;

namespace MoreEffectiveTransfer.Patch
{
    [HarmonyPatch]
    public class TransferManagerAddOutgoingOfferPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(TransferManager).GetMethod("AddOutgoingOffer", BindingFlags.Public | BindingFlags.Instance);
        }

        public static void Prefix(ref TransferManager.TransferReason material, ref TransferManager.TransferOffer offer)
        {
            if (!CustomTransferManager._init)
            {
                CustomTransferManager.Init();
                CustomTransferManager._init = true;
            }

            //If no HelicopterDepot, change offer type.
            if (!HelicopterDepotAISimulationStepPatch.haveFireHelicopterDepotFinal)
            {
                if (material == TransferManager.TransferReason.Fire2)
                {
                    material = TransferManager.TransferReason.Fire;
                }
            }
        }
    }
}