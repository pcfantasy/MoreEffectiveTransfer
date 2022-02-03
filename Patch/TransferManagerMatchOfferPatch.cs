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
            #if (DEBUG || PROFILE)
            // disabled in settings? ->use stock transfer manager
            if (!MoreEffectiveTransfer.optionEnableNewTransferManager)
                return true;
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
    }
}