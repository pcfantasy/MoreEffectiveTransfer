using HarmonyLib;
using System.Reflection;
using MoreEffectiveTransfer.CustomManager;
using MoreEffectiveTransfer.Util;

namespace MoreEffectiveTransfer.Patch
{
    [HarmonyPatch(typeof(TransferManager), "MatchOffers")]
    public class TransferManagerMatchOfferPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TransferManager.TransferReason material)
        {
#if (PROFILE)
            // disabled in settings? ->use stock transfer manager
            if (!MoreEffectiveTransfer.optionEnableNewTransferManager)
            {
                // begin Profiling
                MoreEffectiveTransfer.timerCounterVanilla++;
                MoreEffectiveTransfer.timerVanilla.Start();
                return true;
            }

            MoreEffectiveTransfer.timerCounterMETM++;
            MoreEffectiveTransfer.timerMETM.Start();
#endif

            if (CustomTransferManager.CanUseNewMatchOffers(material))
            {
                CustomTransferManager.MatchOffers(material);
                return false;
            }
            else
            {
                return true;
            }
        }


#if (PROFILE)
        [HarmonyPostfix]
        public static void Postfix()
        {
            // end profiling
            MoreEffectiveTransfer.timerMETM.Stop();
            MoreEffectiveTransfer.timerVanilla.Stop();
        }
#endif
    }


#if (DEBUG)
    [HarmonyPatch(typeof(TransferManager), "StartTransfer")]
    public class TransferManagerStartTransferPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TransferManager __instance, TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            if (material == TransferManager.TransferReason.Dead)
                DebugLog.DebugMsg($"[VANILLA StartTransfer]: {material}, out: {offerOut.Priority},{offerOut.Active},{offerOut.Building}/{offerOut.Vehicle} -- in: {offerIn.Priority},{offerIn.Active},{offerIn.Building}/{offerIn.Vehicle} -- amt: {delta}");

            return true;
        }
    }
#endif

}