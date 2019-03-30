using ColossalFramework;
using ColossalFramework.Plugins;
using MoreEffectiveTransfer.CustomAI;
using MoreEffectiveTransfer.Util;
using System;
using System.Reflection;
using UnityEngine;

namespace MoreEffectiveTransfer.CustomManager
{
    public class CustomTransferManager : TransferManager
    {
        private static bool _init = false;

        public static void TransferManagerAddOutgoingOfferPrefix(TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            //If no HelicopterDepot, change offer type.
            if (!CustomHelicopterDepotAI.haveFireHelicopterDepotFinal)
            {
                if (material == TransferReason.Fire2)
                {
                    material = TransferReason.Fire;
                    for (int i = offer.Priority; i >= 0; i--)
                    {
                        int num = (int)((byte)material * 8 + i);
                        int num2 = (int)m_outgoingCount[num];
                        if (num2 < 256)
                        {
                            int num3 = num * 256 + num2;
                            m_outgoingOffers[num3] = offer;
                            m_outgoingCount[num] = (ushort)(num2 + 1);
                            m_outgoingAmount[(int)material] += offer.Amount;
                            return;
                        }
                    }
                }
            }

            if (!CustomHelicopterDepotAI.haveSickHelicopterDepotFinal)
            {
                if (material == TransferReason.Sick2)
                {
                    material = TransferReason.Sick;
                    for (int i = offer.Priority; i >= 0; i--)
                    {
                        int num = (int)((byte)material * 8 + i);
                        int num2 = (int)m_outgoingCount[num];
                        if (num2 < 256)
                        {
                            int num3 = num * 256 + num2;
                            m_outgoingOffers[num3] = offer;
                            m_outgoingCount[num] = (ushort)(num2 + 1);
                            m_outgoingAmount[(int)material] += offer.Amount;
                            return;
                        }
                    }
                }
            }
        }

        private static float GetDistanceMultiplier(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Garbage:
                    return 5E-07f;
                case TransferReason.Crime:
                    return 1E-05f;
                case TransferReason.Sick:
                    return 1E-06f;
                case TransferReason.Dead:
                    return 1E-05f;
                case TransferReason.Worker0:
                    return 1E-07f;
                case TransferReason.Worker1:
                    return 1E-07f;
                case TransferReason.Worker2:
                    return 1E-07f;
                case TransferReason.Worker3:
                    return 1E-07f;
                case TransferReason.Student1:
                    return 2E-07f;
                case TransferReason.Student2:
                    return 2E-07f;
                case TransferReason.Student3:
                    return 2E-07f;
                case TransferReason.Fire:
                    return 1E-05f;
                case TransferReason.Bus:
                    return 1E-05f;
                case TransferReason.Oil:
                    return 1E-07f;
                case TransferReason.Ore:
                    return 1E-07f;
                case TransferReason.Logs:
                    return 1E-07f;
                case TransferReason.Grain:
                    return 1E-07f;
                case TransferReason.Goods:
                    return 1E-07f;
                case TransferReason.PassengerTrain:
                    return 1E-05f;
                case TransferReason.Coal:
                    return 1E-07f;
                case TransferReason.Family0:
                    return 1E-08f;
                case TransferReason.Family1:
                    return 1E-08f;
                case TransferReason.Family2:
                    return 1E-08f;
                case TransferReason.Family3:
                    return 1E-08f;
                case TransferReason.Single0:
                    return 1E-08f;
                case TransferReason.Single1:
                    return 1E-08f;
                case TransferReason.Single2:
                    return 1E-08f;
                case TransferReason.Single3:
                    return 1E-08f;
                case TransferReason.PartnerYoung:
                    return 1E-08f;
                case TransferReason.PartnerAdult:
                    return 1E-08f;
                case TransferReason.Shopping:
                    return 2E-07f;
                case TransferReason.Petrol:
                    return 1E-07f;
                case TransferReason.Food:
                    return 1E-07f;
                case TransferReason.LeaveCity0:
                    return 1E-08f;
                case TransferReason.LeaveCity1:
                    return 1E-08f;
                case TransferReason.LeaveCity2:
                    return 1E-08f;
                case TransferReason.Entertainment:
                    return 2E-07f;
                case TransferReason.Lumber:
                    return 1E-07f;
                case TransferReason.GarbageMove:
                    return 5E-07f;
                case TransferReason.MetroTrain:
                    return 1E-05f;
                case TransferReason.PassengerPlane:
                    return 1E-05f;
                case TransferReason.PassengerShip:
                    return 1E-05f;
                case TransferReason.DeadMove:
                    return 5E-07f;
                case TransferReason.DummyCar:
                    return -1E-08f;
                case TransferReason.DummyTrain:
                    return -1E-08f;
                case TransferReason.DummyShip:
                    return -1E-08f;
                case TransferReason.DummyPlane:
                    return -1E-08f;
                case TransferReason.Single0B:
                    return 1E-08f;
                case TransferReason.Single1B:
                    return 1E-08f;
                case TransferReason.Single2B:
                    return 1E-08f;
                case TransferReason.Single3B:
                    return 1E-08f;
                case TransferReason.ShoppingB:
                    return 2E-07f;
                case TransferReason.ShoppingC:
                    return 2E-07f;
                case TransferReason.ShoppingD:
                    return 2E-07f;
                case TransferReason.ShoppingE:
                    return 2E-07f;
                case TransferReason.ShoppingF:
                    return 2E-07f;
                case TransferReason.ShoppingG:
                    return 2E-07f;
                case TransferReason.ShoppingH:
                    return 2E-07f;
                case TransferReason.EntertainmentB:
                    return 2E-07f;
                case TransferReason.EntertainmentC:
                    return 2E-07f;
                case TransferReason.EntertainmentD:
                    return 2E-07f;
                case TransferReason.Taxi:
                    return 1E-05f;
                case TransferReason.CriminalMove:
                    return 5E-07f;
                case TransferReason.Tram:
                    return 1E-05f;
                case TransferReason.Snow:
                    return 5E-07f;
                case TransferReason.SnowMove:
                    return 5E-07f;
                case TransferReason.RoadMaintenance:
                    return 5E-07f;
                case TransferReason.SickMove:
                    return 1E-07f;
                case TransferReason.ForestFire:
                    return 1E-05f;
                case TransferReason.Collapsed:
                    return 1E-05f;
                case TransferReason.Collapsed2:
                    return 1E-05f;
                case TransferReason.Fire2:
                    return 1E-05f;
                case TransferReason.Sick2:
                    return 1E-06f;
                case TransferReason.FloodWater:
                    return 5E-07f;
                case TransferReason.EvacuateA:
                    return 1E-05f;
                case TransferReason.EvacuateB:
                    return 1E-05f;
                case TransferReason.EvacuateC:
                    return 1E-05f;
                case TransferReason.EvacuateD:
                    return 1E-05f;
                case TransferReason.EvacuateVipA:
                    return 1E-05f;
                case TransferReason.EvacuateVipB:
                    return 1E-05f;
                case TransferReason.EvacuateVipC:
                    return 1E-05f;
                case TransferReason.EvacuateVipD:
                    return 1E-05f;
                case TransferReason.Ferry:
                    return 1E-05f;
                case TransferReason.CableCar:
                    return 1E-05f;
                case TransferReason.Blimp:
                    return 1E-05f;
                case TransferReason.Monorail:
                    return 1E-05f;
                case TransferReason.TouristBus:
                    return 1E-05f;
                case TransferReason.ParkMaintenance:
                    return 5E-07f;
                case TransferReason.TouristA:
                    return 2E-07f;
                case TransferReason.TouristB:
                    return 2E-07f;
                case TransferReason.TouristC:
                    return 2E-07f;
                case TransferReason.TouristD:
                    return 2E-07f;
                case TransferReason.Mail:
                    return 1E-05f;
                case TransferReason.UnsortedMail:
                    return 5E-07f;
                case TransferReason.SortedMail:
                    return 5E-07f;
                case TransferReason.OutgoingMail:
                    return 5E-07f;
                case TransferReason.IncomingMail:
                    return 5E-07f;
                case TransferReason.AnimalProducts:
                    return 1E-07f;
                case TransferReason.Flours:
                    return 1E-07f;
                case TransferReason.Paper:
                    return 1E-07f;
                case TransferReason.PlanedTimber:
                    return 1E-07f;
                case TransferReason.Petroleum:
                    return 1E-07f;
                case TransferReason.Plastics:
                    return 1E-07f;
                case TransferReason.Glass:
                    return 1E-07f;
                case TransferReason.Metals:
                    return 1E-07f;
                case TransferReason.LuxuryProducts:
                    return 1E-07f;
                default:
                    return 1E-07f;
            }
        }

        private static void Init()
        {
            var inst = Singleton<TransferManager>.instance;
            var incomingCount = typeof(TransferManager).GetField("m_incomingCount", BindingFlags.NonPublic | BindingFlags.Instance);
            var incomingOffers = typeof(TransferManager).GetField("m_incomingOffers", BindingFlags.NonPublic | BindingFlags.Instance);
            var incomingAmount = typeof(TransferManager).GetField("m_incomingAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            var outgoingCount = typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance);
            var outgoingOffers = typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.NonPublic | BindingFlags.Instance);
            var outgoingAmount = typeof(TransferManager).GetField("m_outgoingAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            if (inst == null)
            {
                CODebugBase<LogChannel>.Error(LogChannel.Core, "No instance of TransferManager found!");
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, "No instance of TransferManager found!");
                return;
            }
            m_incomingCount = incomingCount.GetValue(inst) as ushort[];
            m_incomingOffers = incomingOffers.GetValue(inst) as TransferManager.TransferOffer[];
            m_incomingAmount = incomingAmount.GetValue(inst) as int[];
            m_outgoingCount = outgoingCount.GetValue(inst) as ushort[];
            m_outgoingOffers = outgoingOffers.GetValue(inst) as TransferManager.TransferOffer[];
            m_outgoingAmount = outgoingAmount.GetValue(inst) as int[];
        }

        private static TransferManager.TransferOffer[] m_outgoingOffers;

        private static TransferManager.TransferOffer[] m_incomingOffers;

        private static ushort[] m_outgoingCount;

        private static ushort[] m_incomingCount;

        private static int[] m_outgoingAmount;

        private static int[] m_incomingAmount;

        private bool CanUseNewMatchOffers(ushort buildingID, TransferReason material)
        {
            //For outside building, do not use NewMatchOffers
            BuildingManager bM = Singleton<BuildingManager>.instance;
            if (bM.m_buildings.m_buffer[buildingID].m_flags.IsFlagSet(Building.Flags.Untouchable))
            {
                //For more outside connection MOD
                switch (material)
                {
                    case TransferReason.Fire:
                    case TransferReason.Garbage:
                    case TransferReason.GarbageMove:
                    case TransferReason.Crime:
                    case TransferReason.DeadMove:
                        return true;
                    default: return false;
                }
            }
            if (bM.m_buildings.m_buffer[buildingID].Info.m_buildingAI is WarehouseAI)
            {
                return false;
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
                case TransferReason.Fire:
                case TransferReason.Garbage:
                case TransferReason.GarbageMove:
                case TransferReason.Crime:
                case TransferReason.CriminalMove:
                case TransferReason.Dead:
                case TransferReason.DeadMove:
                case TransferReason.Snow:
                case TransferReason.SnowMove:
                case TransferReason.RoadMaintenance:
                case TransferReason.ParkMaintenance:
                case TransferReason.Taxi:
                    return true;
                default: return false;
            }
        }

        private byte MatchOffersMode(TransferReason material)
        {
            //incoming first mode 0
            //outgoing first mode 1
            //balanced mode 2
            //balanced mode with priority 3
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
                    return 3;
                case TransferReason.Garbage:
                case TransferReason.Snow:
                case TransferReason.RoadMaintenance:
                case TransferReason.ParkMaintenance:
                    return 3;
                case TransferReason.Crime:
                case TransferReason.Fire:
                case TransferReason.GarbageMove:
                case TransferReason.CriminalMove:
                case TransferReason.DeadMove:
                case TransferReason.Dead:
                case TransferReason.SnowMove:
                    return 1;
                case TransferReason.Taxi:
                    return 0;
                default: return 2;
            }
        }

        private bool IsUnRoutedMatch(TransferOffer offerIn, TransferOffer offerOut, TransferReason material)
        {
            if (!MoreEffectiveTransfer.fixUnRouteTransfer)
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
                            return true;
                        }
                    }
                }
                return false;
                //info4.m_buildingAI.StartTransfer(building2, ref buildings2.m_buffer[(int)building2], material, offerOut);
            }
            return false;
        }

        private void MatchOffers(TransferReason material)
        {
            if (!_init)
            {
                Init();
                _init = true;
            }
            if (material != TransferReason.None)
            {
                float distanceMultiplier = GetDistanceMultiplier(material);
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
                    if (matchOffersMode == 2 || matchOffersMode == 3)
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
                        if (incomingIdex < incomingCount && (matchOffersMode != 1))
                        {
                            TransferOffer incomingOffer = m_incomingOffers[offerIdex * 256 + incomingIdex];
                            // NON-STOCK CODE START
                            bool canUseNewMatchOffers = CanUseNewMatchOffers(incomingOffer.Building, material);
                            // NON-STOCK CODE END
                            Vector3 incomingPosition = incomingOffer.Position;
                            int incomingOfferAmount = incomingOffer.Amount;
                            // NON-STOCK CODE START
                            TransferReason material2 = material;
                            do
                            {
                                // NON-STOCK CODE END
                                do
                                {
                                    int incomingPriority = Mathf.Max(0, 2 - priority);
                                    int incomingPriorityExclude = (!incomingOffer.Exclude) ? incomingPriority : Mathf.Max(0, 3 - priority);
                                    // NON-STOCK CODE START
                                    float currentShortestDistance = -1f;
                                    if (canUseNewMatchOffers)
                                    {
                                        priority = 7;
                                        if (matchOffersMode != 3)
                                        {
                                            incomingPriority = 0;
                                        }
                                        else
                                        {
                                            incomingPriority = Mathf.Max(0, 2 - oldPriority);
                                        }
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
                                                if (canUseNewMatchOffers)
                                                {
                                                    if ((incomingOutgoingDistance < currentShortestDistance) || currentShortestDistance == -1)
                                                    {
                                                        if (!IsUnRoutedMatch(incomingOffer, outgoingOfferPre, material))
                                                        {
                                                            validPriority = incomingPriorityInside;
                                                            validOutgoingIdex = i;
                                                            currentShortestDistance = incomingOutgoingDistance;
                                                        }
                                                    }
                                                }
                                                // NON-STOCK CODE END
                                                float distanceOffset = (!(distanceMultiplier < 0f)) ? (incomingPriorityInsideFloat / (1f + incomingOutgoingDistance * distanceMultiplier)) : (incomingPriorityInsideFloat - incomingPriorityInsideFloat / (1f - incomingOutgoingDistance * distanceMultiplier));
                                                if ((distanceOffset > distanceOffsetPre) && !canUseNewMatchOffers)
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
                                        StartTransfer(material, outgoingOffer, incomingOffer, matchedOfferAmount);
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
                                // NON-STOCK CODE START
                                if (Loader.isEmployOvereducatedWorkersRunning)
                                {
                                    if (incomingOfferAmount == 0 || material2 < TransferManager.TransferReason.Worker0 || TransferManager.TransferReason.Worker3 <= material2)
                                    {
                                        break;
                                    }
                                    material2++;
                                }
                                else
                                {
                                    break;
                                }
                            } while (true);
                        // NON-STOCK CODE END
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
                        if (outgoingIdex < outgoingCount && (matchOffersMode != 0))
                        {
                            TransferOffer outgoingOffer = m_outgoingOffers[offerIdex * 256 + outgoingIdex];
                            // NON-STOCK CODE START
                            bool canUseNewMatchOffers = CanUseNewMatchOffers(outgoingOffer.Building, material);
                            // NON-STOCK CODE END
                            Vector3 outgoingPosition = outgoingOffer.Position;
                            int outgoingOfferAmount = outgoingOffer.Amount;
                            // NON-STOCK CODE START
                            TransferReason material2 = material;
                            do
                            {
                                // NON-STOCK CODE END
                                do
                                {
                                    int outgoingPriority = Mathf.Max(0, 2 - priority);
                                    int outgoingPriorityExclude = (!outgoingOffer.Exclude) ? outgoingPriority : Mathf.Max(0, 3 - priority);
                                    // NON-STOCK CODE START
                                    float currentShortestDistance = -1f;
                                    if (canUseNewMatchOffers)
                                    {
                                        priority = 7;
                                        if (matchOffersMode != 3)
                                        {
                                            outgoingPriority = 0;
                                        }
                                        else
                                        {
                                            outgoingPriority = Mathf.Max(0, 2 - oldPriority);
                                        }
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
                                                if (canUseNewMatchOffers)
                                                {
                                                    //Fix warehouse always import issue;
                                                    bool wareHouseStopIncoming = false;
                                                    if (incomingOfferPre.Building!= 0)
                                                    {
                                                        if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[incomingOfferPre.Building].Info.m_buildingAI is WarehouseAI)
                                                        {
                                                            if (incomingOfferPre.Priority == 0 && (outgoingOffer.Priority == 0 || outgoingOffer.Priority == 1))
                                                            {
                                                                wareHouseStopIncoming = true;
                                                            }
                                                        }
                                                    }
                                                    if (((incomingOutgoingDistance < currentShortestDistance) || currentShortestDistance == -1) && !wareHouseStopIncoming)
                                                    {
                                                        if (!IsUnRoutedMatch(incomingOfferPre, outgoingOffer, material))
                                                        {
                                                            validPriority = outgoingPriorityInside;
                                                            validIncomingIdex = j;
                                                            currentShortestDistance = incomingOutgoingDistance;
                                                        }
                                                    }
                                                }
                                                // NON-STOCK CODE END
                                                float distanceOffset = (!(distanceMultiplier < 0f)) ? (outgoingPriorityInsideFloat / (1f + incomingOutgoingDistance * distanceMultiplier)) : (outgoingPriorityInsideFloat - outgoingPriorityInsideFloat / (1f - incomingOutgoingDistance * distanceMultiplier));
                                                if ((distanceOffset > distanceOffsetPre) && !canUseNewMatchOffers)
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
                                        StartTransfer(material, outgoingOffer, incomingOffers, matchedOfferAmount);
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
                                // NON-STOCK CODE START
                                // EmployOvereducatedWorkers function
                                if (Loader.isEmployOvereducatedWorkersRunning)
                                {
                                    if (outgoingOfferAmount == 0 || material2 < TransferManager.TransferReason.Worker0 || TransferManager.TransferReason.Worker3 <= material2)
                                    {
                                        break;
                                    }
                                    material2++;
                                }
                                else
                                {
                                    break;
                                }
                            } while (true);
                            // NON-STOCK CODE END
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
                        if (matchOffersMode == 2 || matchOffersMode == 3)
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
        }

        private void StartTransfer(TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            bool active = offerIn.Active;
            bool active2 = offerOut.Active;
            if (active && offerIn.Vehicle != 0)
            {
                Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                ushort vehicle = offerIn.Vehicle;
                VehicleInfo info = vehicles.m_buffer[(int)vehicle].Info;
                offerOut.Amount = delta;
                info.m_vehicleAI.StartTransfer(vehicle, ref vehicles.m_buffer[(int)vehicle], material, offerOut);
            }
            else if (active2 && offerOut.Vehicle != 0)
            {
                Array16<Vehicle> vehicles2 = Singleton<VehicleManager>.instance.m_vehicles;
                ushort vehicle2 = offerOut.Vehicle;
                VehicleInfo info2 = vehicles2.m_buffer[(int)vehicle2].Info;
                offerIn.Amount = delta;
                info2.m_vehicleAI.StartTransfer(vehicle2, ref vehicles2.m_buffer[(int)vehicle2], material, offerIn);
            }
            else if (active && offerIn.Citizen != 0u)
            {
                Array32<Citizen> citizens = Singleton<CitizenManager>.instance.m_citizens;
                uint citizen = offerIn.Citizen;
                CitizenInfo citizenInfo = citizens.m_buffer[(int)((UIntPtr)citizen)].GetCitizenInfo(citizen);
                if (citizenInfo != null)
                {
                    offerOut.Amount = delta;
                    citizenInfo.m_citizenAI.StartTransfer(citizen, ref citizens.m_buffer[(int)((UIntPtr)citizen)], material, offerOut);
                }
            }
            else if (active2 && offerOut.Citizen != 0u)
            {
                Array32<Citizen> citizens2 = Singleton<CitizenManager>.instance.m_citizens;
                uint citizen2 = offerOut.Citizen;
                CitizenInfo citizenInfo2 = citizens2.m_buffer[(int)((UIntPtr)citizen2)].GetCitizenInfo(citizen2);
                if (citizenInfo2 != null)
                {
                    offerIn.Amount = delta;
                    // NON-STOCK CODE START
                    // For RealCity
                    // Remove cotenancy, otherwise we can not caculate family money
                    bool flag2 = false;
                    bool flag = false;
                    if (Loader.isRealCityRunning)
                    {
                        flag2 = (material == TransferManager.TransferReason.Single0 || material == TransferManager.TransferReason.Single1 || material == TransferManager.TransferReason.Single2 || material == TransferManager.TransferReason.Single3 || material == TransferManager.TransferReason.Single0B || material == TransferManager.TransferReason.Single1B || material == TransferManager.TransferReason.Single2B || material == TransferManager.TransferReason.Single3B);
                        flag = (citizenInfo2.m_citizenAI is ResidentAI) && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[offerIn.Building].Info.m_class.m_service == ItemClass.Service.Residential);
                    }

                    if (flag && flag2)
                    {
                        if (material == TransferManager.TransferReason.Single0 || material == TransferManager.TransferReason.Single0B)
                        {
                            material = TransferManager.TransferReason.Family0;
                        }
                        else if (material == TransferManager.TransferReason.Single1 || material == TransferManager.TransferReason.Single1B)
                        {
                            material = TransferManager.TransferReason.Family1;
                        }
                        else if (material == TransferManager.TransferReason.Single2 || material == TransferManager.TransferReason.Single2B)
                        {
                            material = TransferManager.TransferReason.Family2;
                        }
                        else if (material == TransferManager.TransferReason.Single3 || material == TransferManager.TransferReason.Single3B)
                        {
                            material = TransferManager.TransferReason.Family3;
                        }
                        citizenInfo2.m_citizenAI.StartTransfer(citizen2, ref citizens2.m_buffer[(int)((UIntPtr)citizen2)], material, offerIn);
                    }
                    else
                    {
                        /// NON-STOCK CODE END ///
                        citizenInfo2.m_citizenAI.StartTransfer(citizen2, ref citizens2.m_buffer[(int)((UIntPtr)citizen2)], material, offerIn);
                    }
                }
            }
            else if (active2 && offerOut.Building != 0)
            {
                Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
                ushort building = offerOut.Building;
                ushort building1 = offerIn.Building;
                BuildingInfo info3 = buildings.m_buffer[(int)building].Info;
                offerIn.Amount = delta;
                // NON-STOCK CODE START
                // New Outside Interaction Mod
                if ((material == TransferManager.TransferReason.DeadMove || material == TransferManager.TransferReason.GarbageMove) && Singleton<BuildingManager>.instance.m_buildings.m_buffer[offerOut.Building].m_flags.IsFlagSet(Building.Flags.Untouchable))
                {
                    StartMoreTransfer(building, ref buildings.m_buffer[(int)building], material, offerIn);
                }
                else
                {
                    // NON-STOCK CODE END
                    info3.m_buildingAI.StartTransfer(building, ref buildings.m_buffer[(int)building], material, offerIn);
                }
            }
            else if (active && offerIn.Building != 0)
            {
                Array16<Building> buildings2 = Singleton<BuildingManager>.instance.m_buildings;
                ushort building2 = offerIn.Building;
                BuildingInfo info4 = buildings2.m_buffer[(int)building2].Info;
                offerOut.Amount = delta;
                info4.m_buildingAI.StartTransfer(building2, ref buildings2.m_buffer[(int)building2], material, offerOut);
            }
        }

        public void StartMoreTransfer(ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            if (material == TransferManager.TransferReason.GarbageMove)
            {
                VehicleInfo randomVehicleInfo2 = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Garbage, ItemClass.SubService.None, ItemClass.Level.Level1);
                if (randomVehicleInfo2 != null)
                {
                    Array16<Vehicle> vehicles2 = Singleton<VehicleManager>.instance.m_vehicles;
                    ushort num2;
                    if (Singleton<VehicleManager>.instance.CreateVehicle(out num2, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo2, data.m_position, material, false, true))
                    {
                        randomVehicleInfo2.m_vehicleAI.SetSource(num2, ref vehicles2.m_buffer[(int)num2], buildingID);
                        randomVehicleInfo2.m_vehicleAI.StartTransfer(num2, ref vehicles2.m_buffer[(int)num2], material, offer);
                        vehicles2.m_buffer[num2].m_flags |= (Vehicle.Flags.Importing);
                    }
                }
            }
            else if (material == TransferManager.TransferReason.DeadMove)
            {
                VehicleInfo randomVehicleInfo2 = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.HealthCare, ItemClass.SubService.None, ItemClass.Level.Level2);
                if (randomVehicleInfo2 != null)
                {
                    Array16<Vehicle> vehicles2 = Singleton<VehicleManager>.instance.m_vehicles;
                    ushort num2;
                    if (Singleton<VehicleManager>.instance.CreateVehicle(out num2, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo2, data.m_position, material, false, true))
                    {
                        randomVehicleInfo2.m_vehicleAI.SetSource(num2, ref vehicles2.m_buffer[(int)num2], buildingID);
                        randomVehicleInfo2.m_vehicleAI.StartTransfer(num2, ref vehicles2.m_buffer[(int)num2], material, offer);
                        vehicles2.m_buffer[num2].m_flags |= (Vehicle.Flags.Importing);
                    }
                }
            }
        }
    }
}