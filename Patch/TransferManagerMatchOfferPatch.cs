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
            // Profiling
            MoreEffectiveTransfer.timer.Start();

            // disabled in settings? ->use stock transfer manager
            if (!MoreEffectiveTransfer.optionEnableNewTransferManager)
            {
                MoreEffectiveTransfer.timerCounterVanilla++;
                return true;
            }
            MoreEffectiveTransfer.timerCounterMETM++;
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
        public static void Postfix(TransferManager.TransferReason material)
        {
            // end profiling
            MoreEffectiveTransfer.timer.Stop();
            if (!MoreEffectiveTransfer.optionEnableNewTransferManager)
                MoreEffectiveTransfer.timerMillisecondsVanilla += MoreEffectiveTransfer.timer.ElapsedMilliseconds;
            else
                MoreEffectiveTransfer.timerMillisecondsMETM += MoreEffectiveTransfer.timer.ElapsedMilliseconds;
            
            MoreEffectiveTransfer.timer.Reset();
        }
#endif

    }
}