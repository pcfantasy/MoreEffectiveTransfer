using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MoreEffectiveTransfer
{
    public class MoreEffectiveTransferThreading : ThreadingExtensionBase
    {
        public static bool isFirstTime = true;
        //Store Failed to connected building
        public static uint[,] canNotConnectedBuildingID = new uint[49152,8];
        public static byte[] refreshCanNotConnectedBuildingIDCount = new byte[49152];
        public override void OnBeforeSimulationFrame()
        {
            base.OnBeforeSimulationFrame();
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex + 1u;
                int num7 = (int)(currentFrameIndex & 15u);
                int num8 = num7 * 1024;
                int num9 = (num7 + 1) * 1024 - 1;
                VehicleManager instance1 = Singleton<VehicleManager>.instance;
                if (MoreEffectiveTransfer.IsEnabled)
                {
                    for (int i = num8; i <= num9; i = i + 1)
                    {
                        VehicleStatus(i, currentFrameIndex, ref instance1.m_vehicles.m_buffer[i]);
                    }
                }
            }
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

        public void VehicleStatus(int i, uint currentFrameIndex, ref Vehicle vehicle)
        {
            int num4 = (int)(currentFrameIndex & 255u);

            if (MoreEffectiveTransfer.fixUnRouteTransfer)
            {
                if (NeedCheckPathFind((TransferManager.TransferReason)vehicle.m_transferType))
                {
                    if (vehicle.m_targetBuilding != 0)
                    {
                        if (vehicle.m_sourceBuilding != 0)
                        {
                            if (vehicle.m_flags.IsFlagSet(Vehicle.Flags.Created) && !vehicle.m_flags.IsFlagSet(Vehicle.Flags.Deleted))
                            {
                                if (vehicle.m_flags.IsFlagSet(Vehicle.Flags.WaitingPath))
                                {
                                    PathManager instance1 = Singleton<PathManager>.instance;
                                    byte pathFindFlags = instance1.m_pathUnits.m_buffer[(int)((UIntPtr)vehicle.m_path)].m_pathFindFlags;
                                    if ((pathFindFlags & 8) != 0)
                                    {
                                        bool alreadyHaveFailedBuilding = false;
                                        bool reachMaxFailedBuilding = true;
                                        for (int j = 0; j < 8; j++)
                                        {
                                            if (canNotConnectedBuildingID[vehicle.m_targetBuilding, j] == vehicle.m_sourceBuilding)
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
                                                if (canNotConnectedBuildingID[vehicle.m_targetBuilding, j] == 0)
                                                {
                                                    canNotConnectedBuildingID[vehicle.m_targetBuilding, j] = vehicle.m_sourceBuilding;
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
                                    else if ((pathFindFlags & 4) != 0)
                                    {
                                        //After several times, we can ignore those (can not connected) buildings
                                        refreshCanNotConnectedBuildingIDCount[vehicle.m_targetBuilding]++;
                                        if (refreshCanNotConnectedBuildingIDCount[vehicle.m_targetBuilding] > 32)
                                        {
                                            for (int j = 0; j < 8; j++)
                                            {
                                                refreshCanNotConnectedBuildingIDCount[vehicle.m_targetBuilding] = 0;
                                                if (canNotConnectedBuildingID[vehicle.m_targetBuilding, j] != 0)
                                                {
                                                    canNotConnectedBuildingID[vehicle.m_targetBuilding, j] = 0;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
