using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEffectiveTransfer
{
    public class CustomCarAI
    {
        //Store Failed to connected building
        public static uint[,] canNotConnectedBuildingID = new uint[49152, 8];
        public static byte[] refreshCanNotConnectedBuildingIDCount = new byte[49152];

        protected void PathfindFailure(ushort vehicleID, ref Vehicle data)
        {
            RecordFailedBuilding(vehicleID, ref data);
            data.Unspawn(vehicleID);
        }

        private bool NeedCheckPathFind(TransferManager.TransferReason material)
        {
            switch (material)
            {
                case TransferManager.TransferReason.Oil:
                case TransferManager.TransferReason.Ore:
                case TransferManager.TransferReason.Coal:
                case TransferManager.TransferReason.Petrol:
                case TransferManager.TransferReason.Food:
                case TransferManager.TransferReason.Grain:
                case TransferManager.TransferReason.Lumber:
                case TransferManager.TransferReason.Logs:
                case TransferManager.TransferReason.Goods:
                case TransferManager.TransferReason.LuxuryProducts:
                case TransferManager.TransferReason.AnimalProducts:
                case TransferManager.TransferReason.Flours:
                case TransferManager.TransferReason.Petroleum:
                case TransferManager.TransferReason.Plastics:
                case TransferManager.TransferReason.Metals:
                case TransferManager.TransferReason.Glass:
                case TransferManager.TransferReason.PlanedTimber:
                case TransferManager.TransferReason.Paper:
                case TransferManager.TransferReason.Fire:
                case TransferManager.TransferReason.Garbage:
                case TransferManager.TransferReason.GarbageMove:
                case TransferManager.TransferReason.Crime:
                case TransferManager.TransferReason.CriminalMove:
                case TransferManager.TransferReason.Dead:
                case TransferManager.TransferReason.DeadMove:
                case TransferManager.TransferReason.Snow:
                case TransferManager.TransferReason.SnowMove:
                case TransferManager.TransferReason.RoadMaintenance:
                case TransferManager.TransferReason.ParkMaintenance:
                case TransferManager.TransferReason.Taxi:
                    return true;
                default: return false;
            }
        }

        protected void RecordFailedBuilding(ushort vehicleID, ref Vehicle data)
        {
            if (MoreEffectiveTransfer.fixUnRouteTransfer)
            {
                if (NeedCheckPathFind((TransferManager.TransferReason)data.m_transferType))
                {
                    if (data.m_targetBuilding != 0)
                    {
                        if (data.m_sourceBuilding != 0)
                        {
                            bool alreadyHaveFailedBuilding = false;
                            bool reachMaxFailedBuilding = true;
                            for (int j = 0; j < 8; j++)
                            {
                                if (canNotConnectedBuildingID[data.m_targetBuilding, j] == data.m_sourceBuilding)
                                {
                                    alreadyHaveFailedBuilding = true;
                                    reachMaxFailedBuilding = false;
                                    break;
                                }
                            }

                            if (!alreadyHaveFailedBuilding)
                            {
                                for (int j = 0; j < 8; j++)
                                {
                                    if (canNotConnectedBuildingID[data.m_targetBuilding, j] == 0)
                                    {
                                        canNotConnectedBuildingID[data.m_targetBuilding, j] = data.m_sourceBuilding;
                                        reachMaxFailedBuilding = false;
                                        break;
                                    }
                                }
                            }

                            if (reachMaxFailedBuilding)
                            {
                                DebugLog.LogToFileOnly("Error: reachMaxFailedBuilding, please check your roadnetwork");
                            }
                        }
                    }
                }
            }
        }

        protected void ForgetFailedBuilding(ushort vehicleID, ref Vehicle data)
        {
            if (MoreEffectiveTransfer.fixUnRouteTransfer)
            {
                if (refreshCanNotConnectedBuildingIDCount[data.m_targetBuilding] > 32)
                {
                    if (data.m_targetBuilding != 0)
                    {
                        if (data.m_sourceBuilding != 0)
                        {
                            //After several times, we can ignore those (can not connected) buildings
                            refreshCanNotConnectedBuildingIDCount[data.m_targetBuilding]++;
                            for (int j = 0; j < 8; j++)
                            {
                                refreshCanNotConnectedBuildingIDCount[data.m_targetBuilding] = 0;
                                if (canNotConnectedBuildingID[data.m_targetBuilding, j] != 0)
                                {
                                    canNotConnectedBuildingID[data.m_targetBuilding, j] = 0;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void PathfindSuccess(ushort vehicleID, ref Vehicle data)
        {
            ForgetFailedBuilding(vehicleID, ref data);
        }
    }
}
