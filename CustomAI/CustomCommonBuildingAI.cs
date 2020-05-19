using MoreEffectiveTransfer.Util;

namespace MoreEffectiveTransfer.CustomAI
{
    public class CustomCommonBuildingAI
    {
        public delegate void CommonBuildingAICalculateOwnVehicles(CommonBuildingAI CommonBuildingAI, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);
        public static CommonBuildingAICalculateOwnVehicles CalculateOwnVehicles;

        public static void InitDelegate()
        {
            if (CalculateOwnVehicles != null)
                return;
            CalculateOwnVehicles = FastDelegateFactory.Create<CommonBuildingAICalculateOwnVehicles>(typeof(CommonBuildingAI), "CalculateOwnVehicles", instanceMethod: true);
        }
    }
}
