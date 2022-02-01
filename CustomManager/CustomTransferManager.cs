﻿using ColossalFramework;
using ColossalFramework.Plugins;
using MoreEffectiveTransfer.CustomAI;
using MoreEffectiveTransfer.Util;
using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace MoreEffectiveTransfer.CustomManager
{
    public class CustomTransferManager : TransferManager
    {
        public static bool _init = false;

        // Matching logic
        private enum OFFER_MATCHMODE : int { INCOMING_FIRST = 1, OUTGOING_FIRST = 2, BALANCED = 3 };


        // References to game functionalities:
        private static TransferManager _TransferManager = null;
        private static BuildingManager _BuildingManager = null;
        private static VehicleManager _VehicleManager = null;
        private static InstanceManager _InstanceManager = null;
        private static DistrictManager _DistrictManager = null;
        private static CitizenManager _CitizenManager = null;

        // TransferManager internal fields and arrays
        public static TransferManager.TransferOffer[] m_outgoingOffers;
        public static TransferManager.TransferOffer[] m_incomingOffers;
        public static ushort[] m_outgoingCount;
        public static ushort[] m_incomingCount;
        public static int[] m_outgoingAmount;
        public static int[] m_incomingAmount;


        #region DELEGATES
        public static void InitDelegate()
        {
            TransferManagerStartTransferDG = FastDelegateFactory.Create<TransferManagerStartTransfer>(typeof(TransferManager), "StartTransfer", instanceMethod: true);
            TransferManagerGetDistanceMultiplierDG = FastDelegateFactory.Create<TransferManagerGetDistanceMultiplier>(typeof(TransferManager), "GetDistanceMultiplier", instanceMethod: false);
        }

        public delegate void TransferManagerStartTransfer(TransferManager TransferManager, TransferReason material, TransferOffer offerOut, TransferOffer offerIn, int delta);
        public static TransferManagerStartTransfer TransferManagerStartTransferDG;

        public delegate float TransferManagerGetDistanceMultiplier(TransferManager.TransferReason material);
        public static TransferManagerGetDistanceMultiplier TransferManagerGetDistanceMultiplierDG;
        #endregion


        public static void Init()
        {
            _TransferManager = Singleton<TransferManager>.instance;
            if (_TransferManager == null)
            {
                CODebugBase<LogChannel>.Error(LogChannel.Core, "No instance of TransferManager found!");
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, "No instance of TransferManager found!");
                return;
            }

            var incomingCount = typeof(TransferManager).GetField("m_incomingCount", BindingFlags.NonPublic | BindingFlags.Instance);
            var incomingOffers = typeof(TransferManager).GetField("m_incomingOffers", BindingFlags.NonPublic | BindingFlags.Instance);
            var incomingAmount = typeof(TransferManager).GetField("m_incomingAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            var outgoingCount = typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance);
            var outgoingOffers = typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.NonPublic | BindingFlags.Instance);
            var outgoingAmount = typeof(TransferManager).GetField("m_outgoingAmount", BindingFlags.NonPublic | BindingFlags.Instance);

            m_incomingCount = incomingCount.GetValue(_TransferManager) as ushort[];
            m_incomingOffers = incomingOffers.GetValue(_TransferManager) as TransferManager.TransferOffer[];
            m_incomingAmount = incomingAmount.GetValue(_TransferManager) as int[];
            m_outgoingCount = outgoingCount.GetValue(_TransferManager) as ushort[];
            m_outgoingOffers = outgoingOffers.GetValue(_TransferManager) as TransferManager.TransferOffer[];
            m_outgoingAmount = outgoingAmount.GetValue(_TransferManager) as int[];

            InitDelegate();

            // get references to other managers:
            CustomTransferManager._BuildingManager = Singleton<BuildingManager>.instance;
            CustomTransferManager._InstanceManager = Singleton<InstanceManager>.instance;
            CustomTransferManager._VehicleManager  = Singleton<VehicleManager>.instance;
            CustomTransferManager._DistrictManager = Singleton<DistrictManager>.instance;
            CustomTransferManager._CitizenManager  = Singleton<CitizenManager>.instance;

            _init = true;
        }


        public static bool CanUseNewMatchOffers(TransferReason material)
        {
            switch (material)
            {
                // Goods & Service for new transfer manager:
                /*
              case TransferReason.Garbage:
              case TransferReason.GarbageMove:
              case TransferReason.GarbageTransfer:
              case TransferReason.Crime:
              case TransferReason.CriminalMove:
              case TransferReason.Fire:
              case TransferReason.Fire2:
              case TransferReason.ForestFire:
                */
              case TransferReason.Sick:
              case TransferReason.Sick2:
              case TransferReason.SickMove:
              case TransferReason.Dead:
              case TransferReason.DeadMove:

            //case TransferReason.Collapsed:
            //case TransferReason.Collapsed2:

            //case TransferReason.Snow:
            //case TransferReason.SnowMove:
            //case TransferReason.RoadMaintenance:            
            //case TransferReason.ParkMaintenance:

            //case TransferReason.Mail:
            //case TransferReason.UnsortedMail:
            //case TransferReason.SortedMail:
            //case TransferReason.OutgoingMail:
            //case TransferReason.IncomingMail:

            //case TransferReason.Oil:
            //case TransferReason.Ore:
            //case TransferReason.Logs:
            //case TransferReason.Grain:
            //case TransferReason.Goods:
            //case TransferReason.Coal:
            //case TransferReason.Petrol:
            //case TransferReason.Food:
            //case TransferReason.Lumber:
            
            //case TransferReason.Taxi:

            //case TransferReason.AnimalProducts:
            //case TransferReason.Flours:
            //case TransferReason.Paper:
            //case TransferReason.PlanedTimber:
            //case TransferReason.Petroleum:
            //case TransferReason.Plastics:
            //case TransferReason.Glass:
            //case TransferReason.Metals:
            //case TransferReason.LuxuryProducts:
            //case TransferReason.Fish:
                
                    return true;
                
                // Default: use vanilla transfermanager (esp. citizens)
                default: 
                    return false;
            }
        }

        private static OFFER_MATCHMODE GetMatchOffersMode(TransferReason material)
        {
            //incoming first: pick highest priority outgoing offers by distance
            //outgoing first: try to fulfill all outgoing offers by descending priority. incoming offer mapped by distance only (priority not relevant).
            switch (material)
            {
                case TransferReason.Oil:
                case TransferReason.Ore:
                case TransferReason.Coal:
                case TransferReason.Petrol:
                case TransferReason.Food:
                case TransferReason.Grain:
                case TransferReason.Lumber:
                case TransferReason.Logs:
                case TransferReason.Goods:
                case TransferReason.LuxuryProducts:
                case TransferReason.AnimalProducts:
                case TransferReason.Flours:
                case TransferReason.Petroleum:
                case TransferReason.Plastics:
                case TransferReason.Metals:
                case TransferReason.Glass:
                case TransferReason.PlanedTimber:
                case TransferReason.Paper:
                case TransferReason.Snow:
                case TransferReason.RoadMaintenance:
                case TransferReason.ParkMaintenance:
                case TransferReason.Fish:
                    return OFFER_MATCHMODE.BALANCED;

                case TransferReason.Garbage:            //Garbage: outgoing offer (passive) from buldings with garbage to be collected, incoming (active) from landfills
                case TransferReason.GarbageTransfer:    //GarbageTransfer: outgoing (passive) from landfills/wtf, incoming (active) from wasteprocessingcomplex
                case TransferReason.Crime:              //Crime: outgoing offer (passive) 
                case TransferReason.ForestFire:         //like Fire2
                case TransferReason.Fire2:              //Fire2: helicopter
                case TransferReason.Fire:               //Fire: outgoing offer (passive) - always prio7
                case TransferReason.Dead:               //Dead: outgoing offer (passive) 
                case TransferReason.Sick:               //Sick: outgoing offer (passive) [special case: citizen with outgoing and active]
                case TransferReason.Sick2:              //Sick2: helicopter
                case TransferReason.Taxi:
                    return OFFER_MATCHMODE.OUTGOING_FIRST;

                case TransferReason.GarbageMove:        //GarbageMove: outgoing (active) from emptying landfills, incoming (passive) from receiving landfills/wastetransferfacilities/wasteprocessingcomplex
                case TransferReason.CriminalMove:
                case TransferReason.SickMove:
                case TransferReason.DeadMove:           //outgoing (active) from emptying, incoming (passive) from receiving
                case TransferReason.SnowMove:
                    return OFFER_MATCHMODE.INCOMING_FIRST;

                default: 
                    return OFFER_MATCHMODE.BALANCED;
            }
        }

        public static bool RejectLowPriority(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.GarbageMove:
                case TransferReason.CriminalMove:
                case TransferReason.DeadMove:
                case TransferReason.SnowMove:
                    return true;
                default:
                    return false;
            }
        }

        public static bool CanWareHouseTransfer(TransferOffer offerIn, TransferOffer offerOut, TransferReason material)
        {
            BuildingManager bM = Singleton<BuildingManager>.instance;
            if (offerIn.Building == 0 || offerIn.Building > Singleton<BuildingManager>.instance.m_buildings.m_size)
            {
                return true;
            }

            if (offerOut.Building == 0 || offerOut.Building > Singleton<BuildingManager>.instance.m_buildings.m_size)
            {
                return true;
            }

            if (bM.m_buildings.m_buffer[offerOut.Building].Info.m_buildingAI is WarehouseAI)
            {
                if (bM.m_buildings.m_buffer[offerIn.Building].Info.m_buildingAI is OutsideConnectionAI)
                {
                    if (MoreEffectiveTransfer.optionWarehouseReserveTrucks)
                    {
                        var AI = bM.m_buildings.m_buffer[offerOut.Building].Info.m_buildingAI as WarehouseAI;
                        TransferManager.TransferReason actualTransferReason = AI.GetActualTransferReason(offerOut.Building, ref bM.m_buildings.m_buffer[offerOut.Building]);
                        if (actualTransferReason != TransferManager.TransferReason.None)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(AI.m_info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            int num = (productionRate * AI.m_truckCount + 99) / 100;
                            int num2 = 0;
                            int num3 = 0;
                            int num4 = 0;
                            int num5 = 0;
                            CustomCommonBuildingAI.InitDelegate();
                            CustomCommonBuildingAI.CalculateOwnVehicles(AI, offerOut.Building, ref bM.m_buildings.m_buffer[offerOut.Building], actualTransferReason, ref num2, ref num3, ref num4, ref num5);
                            if (num5 * 1.25f > (num - 1))
                                return false;
                            else
                                return true;
                        }
                    }
                    else
                        return true;
                }
            }

            return true;
        }

        public static float WareHouseFirst(TransferOffer offerIn, TransferOffer offerOut, TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Oil:
                case TransferReason.Ore:
                case TransferReason.Coal:
                case TransferReason.Petrol:
                case TransferReason.Food:
                case TransferReason.Grain:
                case TransferReason.Lumber:
                case TransferReason.Logs:
                case TransferReason.Goods:
                case TransferReason.LuxuryProducts:
                case TransferReason.AnimalProducts:
                case TransferReason.Flours:
                case TransferReason.Petroleum:
                case TransferReason.Plastics:
                case TransferReason.Metals:
                case TransferReason.Glass:
                case TransferReason.PlanedTimber:
                case TransferReason.Paper:
                    break;
                default:
                    return 1f;
            }


            BuildingManager bM = Singleton<BuildingManager>.instance;
            if (bM.m_buildings.m_buffer[offerIn.Building].Info.m_buildingAI is WarehouseAI)
            {
                if (bM.m_buildings.m_buffer[offerIn.Building].m_flags.IsFlagSet(Building.Flags.Downgrading) || bM.m_buildings.m_buffer[offerIn.Building].m_flags.IsFlagSet(Building.Flags.Filling))
                {
                    if (MoreEffectiveTransfer.optionWarehouseFirst)
                        return 100f;
                }
                else
                {
                    if (bM.m_buildings.m_buffer[offerOut.Building].Info.m_buildingAI is WarehouseAI)
                    {
                        if (bM.m_buildings.m_buffer[offerOut.Building].m_flags.IsFlagSet(Building.Flags.Downgrading) || bM.m_buildings.m_buffer[offerOut.Building].m_flags.IsFlagSet(Building.Flags.Filling))
                        {
                            if (MoreEffectiveTransfer.optionWarehouseFirst)
                                return 100f;
                        }
                        else
                        {
                            return 0.01f;
                        }
                    }
                    else
                    {
                        if (MoreEffectiveTransfer.optionWarehouseFirst)
                            return 100f;
                    }
                }
            }
            else if (bM.m_buildings.m_buffer[offerOut.Building].Info.m_buildingAI is WarehouseAI)
            {
                if (bM.m_buildings.m_buffer[offerOut.Building].m_flags.IsFlagSet(Building.Flags.Downgrading) || bM.m_buildings.m_buffer[offerOut.Building].m_flags.IsFlagSet(Building.Flags.Filling))
                {
                    if (MoreEffectiveTransfer.optionWarehouseFirst)
                        return 100f;
                }
                else
                {
                    if (bM.m_buildings.m_buffer[offerIn.Building].Info.m_buildingAI is WarehouseAI)
                    {
                        if (bM.m_buildings.m_buffer[offerIn.Building].m_flags.IsFlagSet(Building.Flags.Downgrading) || bM.m_buildings.m_buffer[offerIn.Building].m_flags.IsFlagSet(Building.Flags.Filling))
                        {
                            if (MoreEffectiveTransfer.optionWarehouseFirst)
                                return 100f;
                        }
                        else
                        {
                            return 0.01f;
                        }
                    }
                    else
                    {
                        if (MoreEffectiveTransfer.optionWarehouseFirst)
                            return 100f;
                    }
                }
            }

            return 1f;
        }

        public static float ApplyPriority(TransferOffer offerIn, TransferOffer offerOut, TransferReason material, float preDistance)
        {
            if (!MoreEffectiveTransfer.optionPreferExportShipPlaneTrain)
            {
                return preDistance;
            }

            switch (material)
            {
                case TransferReason.Oil:
                case TransferReason.Ore:
                case TransferReason.Coal:
                case TransferReason.Petrol:
                case TransferReason.Food:
                case TransferReason.Grain:
                case TransferReason.Lumber:
                case TransferReason.Logs:
                case TransferReason.Goods:
                case TransferReason.LuxuryProducts:
                case TransferReason.AnimalProducts:
                case TransferReason.Flours:
                case TransferReason.Petroleum:
                case TransferReason.Plastics:
                case TransferReason.Metals:
                case TransferReason.Glass:
                case TransferReason.PlanedTimber:
                case TransferReason.Paper:
                    break;
                default:
                    return preDistance;
            }


            BuildingManager bM = Singleton<BuildingManager>.instance;
            bool offerInOutside = false;
            bool offerInOutsidePlane = false;
            bool offerInOutsideShip = false;
            bool offerInOutsideTrain = false;
            bool offerOutOutside = false;
            bool offerOutOutsidePlane = false;
            bool offerOutOutsideShip = false;
            bool offerOutOutsideTrain = false;
            if (bM.m_buildings.m_buffer[offerIn.Building].m_flags.IsFlagSet(Building.Flags.Untouchable))
            {
                switch (bM.m_buildings.m_buffer[offerIn.Building].Info.m_class.m_subService)
                {
                    case ItemClass.SubService.PublicTransportPlane:
                        offerInOutside = true;
                        offerInOutsidePlane = true;
                        break;
                    case ItemClass.SubService.PublicTransportShip:
                        offerInOutside = true;
                        offerInOutsideShip = true;
                        break;
                    case ItemClass.SubService.PublicTransportTrain:
                        offerInOutside = true;
                        offerInOutsideTrain = true;
                        break;
                    default: break;
                }
            }

            if (bM.m_buildings.m_buffer[offerOut.Building].m_flags.IsFlagSet(Building.Flags.Untouchable))
            {
                switch (bM.m_buildings.m_buffer[offerOut.Building].Info.m_class.m_subService)
                {
                    case ItemClass.SubService.PublicTransportPlane:
                        offerOutOutside = true;
                        offerOutOutsidePlane = true;
                        break;
                    case ItemClass.SubService.PublicTransportShip:
                        offerOutOutside = true;
                        offerOutOutsideShip = true;
                        break;
                    case ItemClass.SubService.PublicTransportTrain:
                        offerOutOutside = true;
                        offerOutOutsideTrain = true;
                        break;
                    default: break;
                }
            }

            if (offerInOutside && offerOutOutside)
            {
                DebugLog.LogToFileOnly("Error: offerInOutside && offerOutOutside, no such case");
            }
            else if (offerOutOutside)
            {
                if (offerOutOutsidePlane)
                {
                    return preDistance * MoreEffectiveTransfer.planeStationDistanceRandom;
                } 
                else if (offerOutOutsideShip)
                {
                    return preDistance * MoreEffectiveTransfer.shipStationDistanceRandom;
                }
                else if (offerOutOutsideTrain)
                {
                    return preDistance * MoreEffectiveTransfer.trainStationDistanceRandom;
                }
            } 
            else if (offerInOutside)
            {
                if (offerInOutsidePlane)
                {
                    return preDistance * MoreEffectiveTransfer.planeStationDistanceRandom;
                }
                else if (offerInOutsideShip)
                {
                    return preDistance * MoreEffectiveTransfer.shipStationDistanceRandom;
                }
                else if (offerInOutsideTrain)
                {
                    return preDistance * MoreEffectiveTransfer.trainStationDistanceRandom;
                }
            }
            
            return preDistance;
        }

        public static void ForgetFailedBuilding(ushort targetBuilding)
        {
            if (MoreEffectiveTransfer.optionFixUnRouteTransfer)
            {
                if (targetBuilding != 0)
                {
                    if (MainDataStore.canNotConnectedBuildingIDCount[targetBuilding] != 0)
                    {
                        int maxForgetCount = (MainDataStore.canNotConnectedBuildingIDCount[targetBuilding] << 1) + 8;
                        if (maxForgetCount > 600)
                            maxForgetCount = 600;

                        if (MainDataStore.refreshCanNotConnectedBuildingIDCount[targetBuilding] > maxForgetCount)
                        {
                            //After several times we can refresh fail building list.
                            if (MoreEffectiveTransfer.debugMode)
                            {
                                DebugLog.LogToFileOnly("ForgetFailedBuilding begin, count = " + MainDataStore.canNotConnectedBuildingIDCount[targetBuilding].ToString());
                                DebugLog.LogToFileOnly("DebugInfo: m_targetBuilding id is " + targetBuilding.ToString());
                                DebugLog.LogToFileOnly("ForgetFailedBuilding end");
                            }

                            for (int i = 0; i < MainDataStore.canNotConnectedBuildingIDCount[targetBuilding]; i++)
                            {
                                MainDataStore.canNotConnectedBuildingID[targetBuilding, i] = 0;
                            }
                            MainDataStore.canNotConnectedBuildingIDCount[targetBuilding] = 0;
                            MainDataStore.refreshCanNotConnectedBuildingIDCount[targetBuilding] = 0;
                        }
                        else
                        {
                            MainDataStore.refreshCanNotConnectedBuildingIDCount[targetBuilding]++;
                        }
                    }
                }
            }
        }

        public static bool IsUnRoutedMatch(TransferOffer offerIn, TransferOffer offerOut)
        {
            if (!MoreEffectiveTransfer.optionFixUnRouteTransfer)
            {
                return false;
            }

            bool active = offerIn.Active;
            bool active2 = offerOut.Active;
            VehicleManager instance1 = Singleton<VehicleManager>.instance;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if (active && offerIn.Vehicle != 0)
            {
                ushort targetBuilding = 0;
                ushort sourceBuilding = instance1.m_vehicles.m_buffer[offerIn.Vehicle].m_sourceBuilding;
                targetBuilding = offerOut.Building;

                if ((targetBuilding!=0) && (sourceBuilding!=0))
                {
                    for (int j = 0; j < MainDataStore.canNotConnectedBuildingIDCount[targetBuilding]; j++)
                    {
                        if (MainDataStore.canNotConnectedBuildingID[targetBuilding, j] == sourceBuilding)
                        {
                            ForgetFailedBuilding(targetBuilding);
                            return true;
                        }
                    }
                }
                return false;
                //info.m_vehicleAI.StartTransfer(vehicle, ref vehicles.m_buffer[(int)vehicle], material, offerOut);
            }
            else if (active2 && offerOut.Vehicle != 0)
            {
                ushort targetBuilding = 0;
                ushort sourceBuilding = instance1.m_vehicles.m_buffer[offerOut.Vehicle].m_sourceBuilding;
                targetBuilding = offerIn.Building;

                if ((targetBuilding != 0) && (sourceBuilding != 0))
                {
                    for (int j = 0; j < MainDataStore.canNotConnectedBuildingIDCount[targetBuilding]; j++)
                    {
                        if (MainDataStore.canNotConnectedBuildingID[targetBuilding, j] == sourceBuilding)
                        {
                            ForgetFailedBuilding(targetBuilding);
                            return true;
                        }
                    }
                }
                return false;
                //info2.m_vehicleAI.StartTransfer(vehicle2, ref vehicles2.m_buffer[(int)vehicle2], material, offerIn);
            }
            else if (active && offerIn.Citizen != 0u)
            {
                DebugLog.LogToFileOnly("Error: No such case active && offerIn.Citizen != 0u");
                return false;
            }
            else if (active2 && offerOut.Citizen != 0u)
            {
                DebugLog.LogToFileOnly("Error: No such case active && offerOut.Citizen != 0u");
                return false;
            }
            else if (active2 && offerOut.Building != 0)
            {
                ushort targetBuilding = 0;
                ushort sourceBuilding = offerOut.Building;
                targetBuilding = offerIn.Building;

                if ((targetBuilding != 0) && (sourceBuilding != 0))
                {
                    for (int j = 0; j < MainDataStore.canNotConnectedBuildingIDCount[targetBuilding]; j++)
                    {
                        if (MainDataStore.canNotConnectedBuildingID[targetBuilding, j] == sourceBuilding)
                        {
                            ForgetFailedBuilding(targetBuilding);
                            return true;
                        }
                    }
                }
                return false;
                //info3.m_buildingAI.StartTransfer(building, ref buildings.m_buffer[(int)building], material, offerIn);
            }
            else if (active && offerIn.Building != 0)
            {
                ushort targetBuilding = 0;
                ushort sourceBuilding = offerIn.Building;
                targetBuilding = offerOut.Building;

                if ((targetBuilding != 0) && (sourceBuilding != 0))
                {
                    for (int j = 0; j < MainDataStore.canNotConnectedBuildingIDCount[targetBuilding]; j++)
                    {
                        if (MainDataStore.canNotConnectedBuildingID[targetBuilding, j] == sourceBuilding)
                        {
                            ForgetFailedBuilding(targetBuilding);
                            return true;
                        }
                    }
                }
                return false;
                //info4.m_buildingAI.StartTransfer(building2, ref buildings2.m_buffer[(int)building2], material, offerOut);
            }
            return false;
        }

        public static bool IsMaxUnRoutedCountReached(TransferOffer offerIn, TransferOffer offerOut, bool canUseNewMatchOffers)
        {
            if (!canUseNewMatchOffers)
            {
                return true;
            }

            if (!MoreEffectiveTransfer.optionFixUnRouteTransfer)
            {
                return false;
            }

            bool active = offerIn.Active;
            bool active2 = offerOut.Active;
            VehicleManager instance1 = Singleton<VehicleManager>.instance;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if (active && offerIn.Vehicle != 0)
            {
                ushort targetBuilding = 0;
                ushort sourceBuilding = instance1.m_vehicles.m_buffer[offerIn.Vehicle].m_sourceBuilding;
                targetBuilding = offerOut.Building;

                if ((targetBuilding != 0) && (sourceBuilding != 0))
                {
                    if (MainDataStore.canNotConnectedBuildingIDCount[targetBuilding] == 255)
                    {
                        return true;
                    }
                }
                return false;
                //info.m_vehicleAI.StartTransfer(vehicle, ref vehicles.m_buffer[(int)vehicle], material, offerOut);
            }
            else if (active2 && offerOut.Vehicle != 0)
            {
                ushort targetBuilding = 0;
                ushort sourceBuilding = instance1.m_vehicles.m_buffer[offerOut.Vehicle].m_sourceBuilding;
                targetBuilding = offerIn.Building;

                if ((targetBuilding != 0) && (sourceBuilding != 0))
                {
                    if (MainDataStore.canNotConnectedBuildingIDCount[targetBuilding] == 255)
                    {
                        return true;
                    }
                }
                return false;
                //info2.m_vehicleAI.StartTransfer(vehicle2, ref vehicles2.m_buffer[(int)vehicle2], material, offerIn);
            }
            else if (active && offerIn.Citizen != 0u)
            {
                DebugLog.LogToFileOnly("Error: No such case active && offerIn.Citizen != 0u");
                return false;
            }
            else if (active2 && offerOut.Citizen != 0u)
            {
                DebugLog.LogToFileOnly("Error: No such case active && offerOut.Citizen != 0u");
                return false;
            }
            else if (active2 && offerOut.Building != 0)
            {
                ushort targetBuilding = 0;
                ushort sourceBuilding = offerOut.Building;
                targetBuilding = offerIn.Building;

                if ((targetBuilding != 0) && (sourceBuilding != 0))
                {
                    if (MainDataStore.canNotConnectedBuildingIDCount[targetBuilding] == 255)
                    {
                        return true;
                    }
                }
                return false;
                //info3.m_buildingAI.StartTransfer(building, ref buildings.m_buffer[(int)building], material, offerIn);
            }
            else if (active && offerIn.Building != 0)
            {
                ushort targetBuilding = 0;
                ushort sourceBuilding = offerIn.Building;
                targetBuilding = offerOut.Building;

                if ((targetBuilding != 0) && (sourceBuilding != 0))
                {
                    if (MainDataStore.canNotConnectedBuildingIDCount[targetBuilding] == 255)
                    {
                        return true;
                    }
                }
                return false;
                //info4.m_buildingAI.StartTransfer(building2, ref buildings2.m_buffer[(int)building2], material, offerOut);
            }
            return false;
        }

        public static bool IsLocalUse(ref TransferOffer offerIn, ref TransferOffer offerOut, TransferReason material, int priority)
        {
            const int PRIORITY_THRESHOLD_LOCAL = 4; //upper prios 4..7 also get non-local fulfillment
            bool isMoveTransfer = false;

            // guard: current option setting?
            if (!MoreEffectiveTransfer.optionPreferLocalService)
                return true;

            // guard: priority above threshold -> any service is OK!
            if (priority >= PRIORITY_THRESHOLD_LOCAL)
                return true;

            switch (material)
            {
                // Services subject to prefer local services:
                case TransferReason.Garbage:
                case TransferReason.Crime:
                case TransferReason.Fire:
                case TransferReason.Fire2:
                case TransferReason.ForestFire:
                case TransferReason.Dead:
                case TransferReason.Sick:
                case TransferReason.Sick2:

                // Material Transfers for services subject to policy:
                case TransferReason.GarbageMove:        
                case TransferReason.GarbageTransfer:    
                case TransferReason.CriminalMove:
                case TransferReason.DeadMove:
                case TransferReason.SnowMove:
                    isMoveTransfer = true;      //Move Transfers: incoming offer is passive, allow move/emptying to global district buildings
                    break;

                default:
                    return true;
            }

            // determine buildings or vehicle parent buildings
            ushort buildingIncoming = 0, buildingOutgoing = 0;
            
            if (offerIn.Building != 0) buildingIncoming = offerIn.Building;
            else if (offerIn.Vehicle != 0) buildingIncoming = _VehicleManager.m_vehicles.m_buffer[offerIn.Vehicle].m_sourceBuilding;
            else if (offerIn.Citizen != 0) buildingIncoming = _CitizenManager.m_citizens.m_buffer[offerIn.Citizen].m_homeBuilding;
            
            if (offerOut.Building != 0) buildingOutgoing = offerOut.Building;
            else if (offerOut.Vehicle != 0) buildingOutgoing = _VehicleManager.m_vehicles.m_buffer[offerOut.Vehicle].m_sourceBuilding;
            else if (offerOut.Citizen != 0) buildingOutgoing = _CitizenManager.m_citizens.m_buffer[offerIn.Citizen].m_homeBuilding;

            // get respective districts
            byte districtIncoming = _DistrictManager.GetDistrict(_BuildingManager.m_buildings.m_buffer[buildingIncoming].m_position);
            byte districtOutgoing = _DistrictManager.GetDistrict(_BuildingManager.m_buildings.m_buffer[buildingOutgoing].m_position);

            // return true if: both within same district, or active offer is outside district ("in global area")
            if (  (districtIncoming == districtOutgoing) 
                  || (offerIn.Active && districtIncoming == 0)
                  || (offerOut.Active && districtOutgoing == 0) 
                  || (isMoveTransfer && districtIncoming == 0)
               )
               return true;

            return false;
        }


        public static String DebugInspectOffer(ref TransferOffer offer)
        {
            var instB = default(InstanceID);
            instB.Building = offer.Building;
            return (offer.Building > 0 && offer.Building < _BuildingManager.m_buildings.m_size) ? _BuildingManager.m_buildings.m_buffer[offer.Building].Info?.name + "(" + _InstanceManager.GetName(instB) + ")"
                    : (offer.Vehicle > 0 && offer.Vehicle < _VehicleManager.m_vehicles.m_size)  ? _VehicleManager.m_vehicles.m_buffer[offer.Vehicle].Info?.name : "";
        }


        [Conditional("DEBUG")]
        public static void DebugPrintAllOffers(TransferReason material, int offset, int offerCountIncoming, int offerCountOutgoing)
        {
            for (int i=0; i< offerCountIncoming; i++)
            {
                TransferOffer incomingOffer = m_incomingOffers[offset * 256 + i];
                String bname = DebugInspectOffer(ref incomingOffer);
                DebugLog.DebugMsg($"   in #{i}: act {incomingOffer.Active}, excl {incomingOffer.Exclude}, amt {incomingOffer.Amount}, bvc {incomingOffer.Building}/{incomingOffer.Vehicle}/{incomingOffer.Citizen} name={bname}");
            }

            for (int i = 0; i < offerCountOutgoing; i++)
            {
                TransferOffer outgoingOffer = m_outgoingOffers[offset * 256 + i];
                String bname = DebugInspectOffer(ref outgoingOffer);
                DebugLog.DebugMsg($"   out #{i}: act {outgoingOffer.Active}, excl {outgoingOffer.Exclude}, amt {outgoingOffer.Amount}, bvc {outgoingOffer.Building}/{outgoingOffer.Vehicle}/{outgoingOffer.Citizen} name={bname}");
            }
        }


        public static void MatchOffers(TransferReason material)
        {
            //init on first call
            if (!_init)
            {
                Init();
            }

            // guard: ignore transferreason.none
            if (material == TransferReason.None)
                return;

            // function scope variables
            int offer_offset;
            int offerCountIncoming, offerCountIncomingTotal = 0;
            int offerCountOutgoing, offerCountOutgoingTotal = 0;
            int offerAmountIncoming = 0;
            int offerAmountOutgoing = 0;

            // DEBUG LOGGING
            DebugLog.DebugMsg($"-- TRANSFER REASON: {material.ToString()}");
            for (int priority = 7; priority >= 0; priority--)
            {
                offer_offset = (int)material * 8 + priority;
                offerCountIncoming = m_incomingCount[offer_offset];
                offerCountOutgoing = m_outgoingCount[offer_offset];
                offerCountIncomingTotal += offerCountIncoming;
                offerCountOutgoingTotal += offerCountOutgoing;
                offerAmountIncoming = m_incomingAmount[(int)material];
                offerAmountOutgoing = m_outgoingAmount[(int)material];

                DebugLog.DebugMsg($"   #Offers@priority {priority}: {offerCountIncoming} in (amt {offerAmountIncoming}), {offerCountOutgoing} out (amt {offerAmountOutgoing})");
                DebugPrintAllOffers(material, offer_offset, offerCountIncoming, offerCountOutgoing);
            }

            // guard: nothing to match?
            if (offerCountIncomingTotal == 0 || offerCountOutgoingTotal == 0)
                goto END_OFFERMATCHING;


            // Select offer matching algorithm
            OFFER_MATCHMODE match_mode = GetMatchOffersMode(material);


            // OUTGOING FIRST mode - try to fulfill all outgoing requests by finding incomings by distance
            // -------------------------------------------------------------------------------------------
            if (match_mode == OFFER_MATCHMODE.OUTGOING_FIRST)
            {
                DebugLog.DebugMsg($"   ###MatchMode OUTGOING FIRST###");

                // loop outgoing offers by descending priority
                for (int priority = 7; priority >= 0; priority--)
                {
                    offer_offset = (int)material * 8 + priority;
                    offerCountIncoming = m_incomingCount[offer_offset];
                    offerCountOutgoing = m_outgoingCount[offer_offset];

                    // loop all offers within this priority
                    for (int offerIndex = 0; offerIndex < offerCountOutgoing; offerIndex++)
                    {
                        ref TransferOffer outgoingOffer = ref m_outgoingOffers[offer_offset * 256 + offerIndex];
                        if (outgoingOffer.Exclude || outgoingOffer.Amount==0) continue;
                        DebugLog.DebugMsg($"   ###Matching OUTGOING offer: {DebugInspectOffer(ref outgoingOffer)}");

                        int bestmatch_position = -1;
                        float bestmatch_distance = float.MaxValue;

                        // loop through all matching counterpart offers and find closest one
                        for (int counterpart_prio = 7; counterpart_prio >= 0; counterpart_prio--)
                        {
                            int counterpart_offset = (int)material * 8 + counterpart_prio;
                            int counterpart_offercount = m_incomingCount[counterpart_offset];
                            for (int counterpart_index = 0; counterpart_index < counterpart_offercount; counterpart_index++)
                            {
                                ref TransferOffer incomingOffer = ref m_incomingOffers[counterpart_offset * 256 + counterpart_index];

                                // guards: out=in same? exclude offer (already used?)
                                if ((incomingOffer.Exclude || incomingOffer.Amount==0) || (outgoingOffer.m_object == incomingOffer.m_object)) continue;

                                // CHECK OPTION: preferlocalservice
                                bool isLocalAllowed = IsLocalUse(ref incomingOffer, ref outgoingOffer, material, priority);
                                
                                //TODO: CHECK OPTION: MoreEffectiveTransfer.optionWarehouseFirst

                                float distance = Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position);
                                if ((isLocalAllowed) && (distance < bestmatch_distance))
                                {
                                    bestmatch_position = counterpart_offset * 256 + counterpart_index;
                                    bestmatch_distance = distance;
                                }

                                DebugLog.DebugMsg($"       -> Matching incoming offer: {DebugInspectOffer(ref incomingOffer)}, amt {incomingOffer.Amount}, local: {isLocalAllowed}, distance: {distance}, bestmatch: {bestmatch_distance}");                                
                            }
                        }

                        // Select bestmatch
                        if (bestmatch_position != -1)
                        {
                            DebugLog.DebugMsg($"       -> Selecting bestmatch: {DebugInspectOffer(ref m_incomingOffers[bestmatch_position])}");
                            // ATTENTION: last incomingOffer is NOT necessarily bvestmatch!

                            // Start the transfer
                            int deltaamount = Math.Min(outgoingOffer.Amount, m_incomingOffers[bestmatch_position].Amount);
                            TransferManagerStartTransferDG(Singleton<TransferManager>.instance, material, outgoingOffer, m_incomingOffers[bestmatch_position], deltaamount);

                            // mark offer pair as to be excluded for further matches
                            outgoingOffer.Amount -= deltaamount;
                            m_incomingOffers[bestmatch_position].Amount -= deltaamount;
                            

                        }

                    } //end loop priority

                    // guard: no more incoming offers left? ->break processing
                    if (offerCountIncomingTotal == 0 || offerCountOutgoingTotal == 0) break;

                } //end loop outgoing offer

            } //end OFFER_MATCHMODE.OUTGOING_FIRST


            // INCOMING FIRST mode - try to fulfill all incoming offers by finding outgoings by distance
            // -------------------------------------------------------------------------------------------
            if (match_mode == OFFER_MATCHMODE.INCOMING_FIRST)
            {
                DebugLog.DebugMsg($"   ###MatchMode INCOMING FIRST###");

                // loop incoming offers by descending priority
                for (int priority = 7; priority >= 0; priority--)
                {
                    offer_offset = (int)material * 8 + priority;
                    offerCountIncoming = m_incomingCount[offer_offset];
                    offerCountOutgoing = m_outgoingCount[offer_offset];

                    // loop all offers within this priority
                    for (int offerIndex = 0; offerIndex < offerCountIncoming; offerIndex++)
                    {
                        ref TransferOffer incomingOffer = ref m_incomingOffers[offer_offset * 256 + offerIndex];
                        if (incomingOffer.Exclude || incomingOffer.Amount==0) continue;
                        DebugLog.DebugMsg($"   ###Matching INCOMING offer: {DebugInspectOffer(ref incomingOffer)}");

                        int bestmatch_position = -1;
                        float bestmatch_distance = float.MaxValue;

                        // loop through all matching counterpart offers and find closest one
                        for (int counterpart_prio = 7; counterpart_prio >= 0; counterpart_prio--)
                        {
                            int counterpart_offset = (int)material * 8 + counterpart_prio;
                            int counterpart_offercount = m_outgoingCount[counterpart_offset];
                            for (int counterpart_index = 0; counterpart_index < counterpart_offercount; counterpart_index++)
                            {
                                ref TransferOffer outgoingOffer = ref m_outgoingOffers[counterpart_offset * 256 + counterpart_index];

                                // guards: out=in same? exclude offer (already used?)
                                if ((outgoingOffer.Exclude || outgoingOffer.Amount==0) || (outgoingOffer.m_object == incomingOffer.m_object)) continue;

                                // CHECK OPTION: preferlocalservice
                                bool isLocalAllowed = IsLocalUse(ref incomingOffer, ref outgoingOffer, material, priority);

                                //TODO: CHECK OPTION: MoreEffectiveTransfer.optionWarehouseFirst

                                float distance = Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position);
                                if ((isLocalAllowed) && (distance < bestmatch_distance))
                                {
                                    bestmatch_position = counterpart_offset * 256 + counterpart_index;
                                    bestmatch_distance = distance;
                                }

                                DebugLog.DebugMsg($"       -> Matching outgoing offer: {DebugInspectOffer(ref outgoingOffer)}, amt {outgoingOffer.Amount}, local: {isLocalAllowed}, distance: {distance}, bestmatch: {bestmatch_distance}");
                            }
                        }

                        // Select bestmatch
                        if (bestmatch_position != -1)
                        {
                            DebugLog.DebugMsg($"       -> Selecting bestmatch: {DebugInspectOffer(ref m_outgoingOffers[bestmatch_position])}");
                            // ATTENTION: last outgoingOffer is NOT necessarily the bestmatch!

                            // Start the transfer
                            int deltaamount = Math.Min(incomingOffer.Amount, m_outgoingOffers[bestmatch_position].Amount);
                            TransferManagerStartTransferDG(Singleton<TransferManager>.instance, material, m_outgoingOffers[bestmatch_position], incomingOffer, deltaamount);

                            // mark offer pair as to be excluded for further matches
                            incomingOffer.Amount -= deltaamount;
                            m_outgoingOffers[bestmatch_position].Amount -= deltaamount;
                            
                        }

                    } //end loop priority

                    // guard: no more incoming offers left? ->break processing
                    if (offerCountIncomingTotal == 0 || offerCountOutgoingTotal == 0) break;

                } //end loop outgoing offer

            } //end OFFER_MATCHMODE.INCOMING_FIRST



        END_OFFERMATCHING:
            // finally: clear everything, including unmatched offers!
            ClearAllTransferOffers(material);


            /*
            if (material != TransferReason.None)
            {
                float distanceMultiplier = TransferManagerGetDistanceMultiplierDG(material);
                float maxDistance = (distanceMultiplier == 0f) ? 0f : (0.01f / distanceMultiplier);
                for (int priority = 7; priority >= 0; priority--)
                {
                    int offerIdex = (int)material * 8 + priority;
                    int incomingCount = m_incomingCount[offerIdex];
                    int outgoingCount = m_outgoingCount[offerIdex];
                    int incomingIdex = 0;
                    int outgoingIdex = 0;
                    int oldPriority = priority;
                    // NON-STOCK CODE START
                    byte matchOffersMode = MatchOffersMode(material);
                    bool isLoopValid = false;
                    if ((matchOffersMode == 2) || (matchOffersMode == 3))
                    {
                        isLoopValid = (incomingIdex < incomingCount || outgoingIdex < outgoingCount);
                    }
                    else if (matchOffersMode == 1)
                    {
                        isLoopValid = (outgoingIdex < outgoingCount);
                    }
                    else if (matchOffersMode == 0)
                    {
                        isLoopValid = (incomingIdex < incomingCount);
                    }

                    // NON-STOCK CODE END
                    while (isLoopValid)
                    {
                        //use incomingOffer to match outgoingOffer
                        if ((incomingIdex < incomingCount) && (matchOffersMode != 1))
                        {
                            TransferOffer incomingOffer = m_incomingOffers[offerIdex * 256 + incomingIdex];
                            // NON-STOCK CODE START
                            bool canUseNewMatchOffers = CanUseNewMatchOffers(material);
                            // NON-STOCK CODE END
                            Vector3 incomingPosition = incomingOffer.Position;
                            int incomingOfferAmount = incomingOffer.Amount;
                            do
                            {
                                int incomingPriority = Mathf.Max(0, 2 - priority);
                                int incomingPriorityExclude = (!incomingOffer.Exclude) ? incomingPriority : Mathf.Max(0, 3 - priority);
                                // NON-STOCK CODE START
                                float currentShortestDistance = -1f;
                                if (canUseNewMatchOffers && (matchOffersMode != 2))
                                {
                                    //incoming only mode 0
                                    //outgoing only mode 1
                                    //balanced mode 2
                                    //incoming first mode 3
                                    priority = 7;
                                    incomingPriority = Mathf.Max(0, 2 - oldPriority);
                                    incomingPriorityExclude = (!incomingOffer.Exclude) ? incomingPriority : Mathf.Max(0, 3 - oldPriority);
                                }
                                else
                                {
                                    priority = oldPriority;
                                    incomingPriority = Mathf.Max(0, 2 - priority);
                                    incomingPriorityExclude = (!incomingOffer.Exclude) ? incomingPriority : Mathf.Max(0, 3 - priority);
                                }
                                // NON-STOCK CODE END
                                int validPriority = -1;
                                int validOutgoingIdex = -1;
                                float distanceOffsetPre = -1f;
                                int outgoingIdexInsideIncoming = outgoingIdex;
                                for (int incomingPriorityInside = priority; incomingPriorityInside >= incomingPriority; incomingPriorityInside--)
                                {
                                    int outgoingIdexWithPriority = (int)material * 8 + incomingPriorityInside;
                                    int outgoingCountWithPriority = m_outgoingCount[outgoingIdexWithPriority];
                                    //To let incomingPriorityInsideFloat!=0
                                    float incomingPriorityInsideFloat = (float)incomingPriorityInside + 0.1f;
                                    //Higher priority will get more chance to match
                                    //UseNewMatchOffers to find the shortest transfer building
                                    if ((distanceOffsetPre >= incomingPriorityInsideFloat) && !canUseNewMatchOffers)
                                    {
                                        break;
                                    }
                                    //Find the nearest offer to match in every priority.
                                    for (int i = outgoingIdexInsideIncoming; i < outgoingCountWithPriority; i++)
                                    {
                                        TransferOffer outgoingOfferPre = m_outgoingOffers[outgoingIdexWithPriority * 256 + i];
                                        if (incomingOffer.m_object != outgoingOfferPre.m_object && (!outgoingOfferPre.Exclude || incomingPriorityInside >= incomingPriorityExclude))
                                        {
                                            float incomingOutgoingDistance = Vector3.SqrMagnitude(outgoingOfferPre.Position - incomingPosition);
                                            // NON-STOCK CODE START
                                            var isLocalUse = IsLocalUse(incomingOffer, outgoingOfferPre, material);
                                            if (canUseNewMatchOffers)
                                            {
                                                //WareHouse first
                                                incomingOutgoingDistance = incomingOutgoingDistance / WareHouseFirst(incomingOffer, outgoingOfferPre, material);
                                                //Apply Priority
                                                incomingOutgoingDistance = ApplyPriority(incomingOffer, outgoingOfferPre, material, incomingOutgoingDistance);
                                                if ((incomingOutgoingDistance < currentShortestDistance) || currentShortestDistance == -1)
                                                {
                                                    if (isLocalUse)
                                                    {
                                                        if (!IsUnRoutedMatch(incomingOffer, outgoingOfferPre) && CanWareHouseTransfer(incomingOffer, outgoingOfferPre, material))
                                                        {
                                                            validPriority = incomingPriorityInside;
                                                            validOutgoingIdex = i;
                                                            currentShortestDistance = incomingOutgoingDistance;
                                                        }
                                                    }
                                                }
                                            }
                                            if (isLocalUse)
                                            {
                                                // NON-STOCK CODE END
                                                float distanceOffset = (!(distanceMultiplier < 0f)) ? (incomingPriorityInsideFloat / (1f + incomingOutgoingDistance * distanceMultiplier)) : (incomingPriorityInsideFloat - incomingPriorityInsideFloat / (1f - incomingOutgoingDistance * distanceMultiplier));
                                                if ((distanceOffset > distanceOffsetPre) && ((!canUseNewMatchOffers) || (IsMaxUnRoutedCountReached(incomingOffer, outgoingOfferPre, canUseNewMatchOffers))))
                                                {
                                                    validPriority = incomingPriorityInside;
                                                    validOutgoingIdex = i;
                                                    distanceOffsetPre = distanceOffset;
                                                    if ((incomingOutgoingDistance < maxDistance))
                                                    {
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    outgoingIdexInsideIncoming = 0;
                                }
                                // NON-STOCK CODE START
                                if (canUseNewMatchOffers)
                                {
                                    priority = oldPriority;
                                }
                                // NON-STOCK CODE END
                                if (validPriority == -1)
                                {
                                    break;
                                }
                                //Find a validPriority, get outgoingOffer
                                int matchedOutgoingOfferIdex = (int)material * 8 + validPriority;
                                TransferOffer outgoingOffer = m_outgoingOffers[matchedOutgoingOfferIdex * 256 + validOutgoingIdex];
                                int outgoingOfferAmount = outgoingOffer.Amount;
                                int matchedOfferAmount = Mathf.Min(incomingOfferAmount, outgoingOfferAmount);
                                if (matchedOfferAmount != 0)
                                {
                                    TransferManagerStartTransferDG(Singleton<TransferManager>.instance, material, outgoingOffer, incomingOffer, matchedOfferAmount);
                                }
                                incomingOfferAmount -= matchedOfferAmount;
                                outgoingOfferAmount -= matchedOfferAmount;
                                //matched outgoingOffer is empty now
                                if (outgoingOfferAmount == 0)
                                {
                                    int outgoingCountPost = m_outgoingCount[matchedOutgoingOfferIdex] - 1;
                                    m_outgoingCount[matchedOutgoingOfferIdex] = (ushort)outgoingCountPost;
                                    m_outgoingOffers[matchedOutgoingOfferIdex * 256 + validOutgoingIdex] = m_outgoingOffers[matchedOutgoingOfferIdex * 256 + outgoingCountPost];
                                    if (matchedOutgoingOfferIdex == offerIdex)
                                    {
                                        outgoingCount = outgoingCountPost;
                                    }
                                }
                                else
                                {
                                    outgoingOffer.Amount = outgoingOfferAmount;
                                    m_outgoingOffers[matchedOutgoingOfferIdex * 256 + validOutgoingIdex] = outgoingOffer;
                                }
                                incomingOffer.Amount = incomingOfferAmount;
                            }
                            while (incomingOfferAmount != 0);
                            //matched incomingOffer is empty now
                            if (incomingOfferAmount == 0)
                            {
                                incomingCount--;
                                m_incomingCount[offerIdex] = (ushort)incomingCount;
                                m_incomingOffers[offerIdex * 256 + incomingIdex] = m_incomingOffers[offerIdex * 256 + incomingCount];
                            }
                            else
                            {
                                incomingOffer.Amount = incomingOfferAmount;
                                m_incomingOffers[offerIdex * 256 + incomingIdex] = incomingOffer;
                                incomingIdex++;
                            }
                        }
                        //For RealConstruction, We only satisify incoming building
                        //use outgoingOffer to match incomingOffer
                        if ((outgoingIdex < outgoingCount) && (matchOffersMode != 0))
                        {
                            TransferOffer outgoingOffer = m_outgoingOffers[offerIdex * 256 + outgoingIdex];
                            // NON-STOCK CODE START
                            bool canUseNewMatchOffers = CanUseNewMatchOffers(material);
                            // NON-STOCK CODE END
                            Vector3 outgoingPosition = outgoingOffer.Position;
                            int outgoingOfferAmount = outgoingOffer.Amount;
                            do
                            {
                                int outgoingPriority = Mathf.Max(0, 2 - priority);
                                int outgoingPriorityExclude = (!outgoingOffer.Exclude) ? outgoingPriority : Mathf.Max(0, 3 - priority);
                                // NON-STOCK CODE START
                                float currentShortestDistance = -1f;
                                //incoming first mode can only match lower priority
                                if (canUseNewMatchOffers && (matchOffersMode == 1))
                                {
                                    //incoming only mode 0
                                    //outgoing only mode 1
                                    //balanced mode 2
                                    //incoming first mode 3
                                    priority = 7;
                                    outgoingPriority = Mathf.Max(0, 2 - oldPriority);
                                    outgoingPriorityExclude = (!outgoingOffer.Exclude) ? outgoingPriority : Mathf.Max(0, 3 - oldPriority);
                                }
                                else
                                {
                                    priority = oldPriority;
                                    outgoingPriority = Mathf.Max(0, 2 - priority);
                                    outgoingPriorityExclude = (!outgoingOffer.Exclude) ? outgoingPriority : Mathf.Max(0, 3 - priority);
                                }
                                // NON-STOCK CODE END
                                int validPriority = -1;
                                int validIncomingIdex = -1;
                                float distanceOffsetPre = -1f;
                                int incomingIdexInsideOutgoing = incomingIdex;
                                for (int outgoingPriorityInside = priority; outgoingPriorityInside >= outgoingPriority; outgoingPriorityInside--)
                                {
                                    int incomingIdexWithPriority = (int)material * 8 + outgoingPriorityInside;
                                    int incomingCountWithPriority = m_incomingCount[incomingIdexWithPriority];
                                    //To let outgoingPriorityInsideFloat!=0
                                    float outgoingPriorityInsideFloat = (float)outgoingPriorityInside + 0.1f;
                                    //Higher priority will get more chance to match
                                    if ((distanceOffsetPre >= outgoingPriorityInsideFloat) && !canUseNewMatchOffers)
                                    {
                                        break;
                                    }
                                    for (int j = incomingIdexInsideOutgoing; j < incomingCountWithPriority; j++)
                                    {
                                        TransferOffer incomingOfferPre = m_incomingOffers[incomingIdexWithPriority * 256 + j];
                                        if (outgoingOffer.m_object != incomingOfferPre.m_object && (!incomingOfferPre.Exclude || outgoingPriorityInside >= outgoingPriorityExclude))
                                        {
                                            float incomingOutgoingDistance = Vector3.SqrMagnitude(incomingOfferPre.Position - outgoingPosition);
                                            // NON-STOCK CODE START
                                            var isLocalUse = IsLocalUse(incomingOfferPre, outgoingOffer, material);
                                            if (canUseNewMatchOffers)
                                            {
                                                //WareHouse first
                                                incomingOutgoingDistance = incomingOutgoingDistance / WareHouseFirst(incomingOfferPre, outgoingOffer, material);
                                                //Apply Priority
                                                incomingOutgoingDistance = ApplyPriority(incomingOfferPre, outgoingOffer, material, incomingOutgoingDistance);
                                                if (incomingOfferPre.Building != 0)
                                                {
                                                    if (RejectLowPriority(material))
                                                    {
                                                        if (incomingOfferPre.Priority == 0)
                                                        {
                                                            incomingOutgoingDistance = incomingOutgoingDistance * 10000f;
                                                        }
                                                    }
                                                }
                                                if ((incomingOutgoingDistance < currentShortestDistance) || currentShortestDistance == -1)
                                                {
                                                    if (isLocalUse)
                                                    {
                                                        if (!IsUnRoutedMatch(incomingOfferPre, outgoingOffer) && CanWareHouseTransfer(incomingOfferPre, outgoingOffer, material))
                                                        {
                                                            validPriority = outgoingPriorityInside;
                                                            validIncomingIdex = j;
                                                            currentShortestDistance = incomingOutgoingDistance;
                                                        }
                                                    }
                                                }
                                            }

                                            if (isLocalUse)
                                            {
                                                // NON-STOCK CODE END
                                                float distanceOffset = (!(distanceMultiplier < 0f)) ? (outgoingPriorityInsideFloat / (1f + incomingOutgoingDistance * distanceMultiplier)) : (outgoingPriorityInsideFloat - outgoingPriorityInsideFloat / (1f - incomingOutgoingDistance * distanceMultiplier));
                                                if ((distanceOffset > distanceOffsetPre) && ((!canUseNewMatchOffers) || (IsMaxUnRoutedCountReached(incomingOfferPre, outgoingOffer, canUseNewMatchOffers))))
                                                {
                                                    validPriority = outgoingPriorityInside;
                                                    validIncomingIdex = j;
                                                    distanceOffsetPre = distanceOffset;
                                                    if (incomingOutgoingDistance < maxDistance)
                                                    {
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    incomingIdexInsideOutgoing = 0;
                                }
                                // NON-STOCK CODE START
                                if (canUseNewMatchOffers)
                                {
                                    priority = oldPriority;
                                }
                                // NON-STOCK CODE END
                                if (validPriority == -1)
                                {
                                    break;
                                }
                                //Find a validPriority, get incomingOffer
                                int matchedIncomingOfferIdex = (int)material * 8 + validPriority;
                                TransferOffer incomingOffers = m_incomingOffers[matchedIncomingOfferIdex * 256 + validIncomingIdex];
                                int incomingOffersAmount = incomingOffers.Amount;
                                int matchedOfferAmount = Mathf.Min(outgoingOfferAmount, incomingOffersAmount);
                                if (matchedOfferAmount != 0)
                                {
                                    TransferManagerStartTransferDG(Singleton<TransferManager>.instance, material, outgoingOffer, incomingOffers, matchedOfferAmount);
                                }
                                outgoingOfferAmount -= matchedOfferAmount;
                                incomingOffersAmount -= matchedOfferAmount;
                                //matched incomingOffer is empty now
                                if (incomingOffersAmount == 0)
                                {
                                    int incomingCountPost = m_incomingCount[matchedIncomingOfferIdex] - 1;
                                    m_incomingCount[matchedIncomingOfferIdex] = (ushort)incomingCountPost;
                                    m_incomingOffers[matchedIncomingOfferIdex * 256 + validIncomingIdex] = m_incomingOffers[matchedIncomingOfferIdex * 256 + incomingCountPost];
                                    if (matchedIncomingOfferIdex == offerIdex)
                                    {
                                        incomingCount = incomingCountPost;
                                    }
                                }
                                else
                                {
                                    incomingOffers.Amount = incomingOffersAmount;
                                    m_incomingOffers[matchedIncomingOfferIdex * 256 + validIncomingIdex] = incomingOffers;
                                }
                                outgoingOffer.Amount = outgoingOfferAmount;
                            }
                            while (outgoingOfferAmount != 0);
                            //matched outgoingOffer is empty now
                            if (outgoingOfferAmount == 0)
                            {
                                outgoingCount--;
                                m_outgoingCount[offerIdex] = (ushort)outgoingCount;
                                m_outgoingOffers[offerIdex * 256 + outgoingIdex] = m_outgoingOffers[offerIdex * 256 + outgoingCount];
                            }
                            else
                            {
                                outgoingOffer.Amount = outgoingOfferAmount;
                                m_outgoingOffers[offerIdex * 256 + outgoingIdex] = outgoingOffer;
                                outgoingIdex++;
                            }
                        }

                        // NON-STOCK CODE START
                        if ((matchOffersMode == 2) || (matchOffersMode == 3))
                        {
                            isLoopValid = (incomingIdex < incomingCount || outgoingIdex < outgoingCount);
                        }
                        else if (matchOffersMode == 1)
                        {
                            isLoopValid = (outgoingIdex < outgoingCount);
                        }
                        else if (matchOffersMode == 0)
                        {
                            isLoopValid = (incomingIdex < incomingCount);
                        }
                        // NON-STOCK CODE END
                    }
                }
                for (int k = 0; k < 8; k++)
                {
                    int num40 = (int)material * 8 + k;
                    m_incomingCount[num40] = 0;
                    m_outgoingCount[num40] = 0;
                }
                m_incomingAmount[(int)material] = 0;
                m_outgoingAmount[(int)material] = 0;
            }
            */

        }


        private static void ClearAllTransferOffers(TransferReason material)
        {
            for (int k = 0; k < 8; k++)
            {
                int material_offset = (int)material * 8 + k;
                m_incomingCount[material_offset] = 0;
                m_outgoingCount[material_offset] = 0;
            }
            m_incomingAmount[(int)material] = 0;
            m_outgoingAmount[(int)material] = 0;
        }


    }
}