using Harmony;
using System.Reflection;
using MoreEffectiveTransfer.CustomManager;

namespace MoreEffectiveTransfer.Patch
{
    [HarmonyPatch]
    public class TransferManagerMatchOfferPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(TransferManager).GetMethod("MatchOffer", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static bool Prefix(ref TransferManager.TransferReason material, ref TransferManager.TransferOffer offer)
        {
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