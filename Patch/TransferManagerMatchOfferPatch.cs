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
            if (!ModSettings.optionEnableNewTransferManager)
            {
                Profiling.timerCounterVanilla++;
                Profiling.timerVanilla.Start();
                return true;
            }

            Profiling.timerCounterMETM++;
            Profiling.timerMETM.Start();
#endif

            // Dispatch to TransferDispatcher
            CustomTransferDispatcher.Instance.SubmitMatchOfferJob(material);
            return false;
        }



        [HarmonyPostfix]
        public static void Postfix()
        {
            if (ModSettings.optionEnableNewTransferManager)
            {
                Profiling.timerMETM_StartTransfers.Start();
                // Start queued transfers:
                CustomTransferDispatcher.Instance.StartTransfers();
                Profiling.timerMETM_StartTransfers.Stop();
            }
#if (PROFILE)
            Profiling.timerMETM.Stop();
            Profiling.timerVanilla.Stop();
#endif
        }

    } //TransferManagerMatchOfferPatch


#if (DEBUG_VANILLA)
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
    } //TransferManagerStartTransferPatch
#endif

}