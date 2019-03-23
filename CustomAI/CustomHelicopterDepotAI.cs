using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoreEffectiveTransfer.CustomAI
{
    public class CustomHelicopterDepotAI
    {
        public static bool haveFireHelicopterDepot = false;
        public static bool haveFireHelicopterDepotFinal = false;
        public static bool haveSickHelicopterDepot = false;
        public static bool haveSickHelicopterDepotFinal = false;
        public static void HelicopterDepotAISimulationStepPostFix(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            if (buildingData.Info.m_class.m_service == ItemClass.Service.FireDepartment)
            {
                haveFireHelicopterDepot = true;
            }
            else if (buildingData.Info.m_class.m_service == ItemClass.Service.HealthCare)
            {
                haveSickHelicopterDepot = true;
            }
        }
    }
}
