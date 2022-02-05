using HarmonyLib;
using System.Reflection;
using MoreEffectiveTransfer.CustomManager;

namespace MoreEffectiveTransfer.Patch
{
    [HarmonyPatch]
    public class TransferManagerMatchOfferPatch
    {
        
        public static MethodBase TargetMethod()
        {
            return typeof(TransferManager).GetMethod("MatchOffers", BindingFlags.NonPublic | BindingFlags.Instance);
        }

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
        public static void Postfix()
        {
            // end profiling
            MoreEffectiveTransfer.timerMETM.Stop();
            MoreEffectiveTransfer.timerVanilla.Stop();
        }
#endif

    }
}