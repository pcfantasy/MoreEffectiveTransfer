using ColossalFramework;
using ColossalFramework.Plugins;
using MoreEffectiveTransfer.Util;
using System.Reflection;
using UnityEngine;

namespace MoreEffectiveTransfer.CustomManager
{
    public class CustomTransferManager : TransferManager
    {
        public static bool _init = false;

        public static void Init()
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

            InitDelegate();
        }

        public static TransferManager.TransferOffer[] m_outgoingOffers;
        public static TransferManager.TransferOffer[] m_incomingOffers;
        public static ushort[] m_outgoingCount;
        public static ushort[] m_incomingCount;
        public static int[] m_outgoingAmount;
        public static int[] m_incomingAmount;

        public static bool CanUseNewMatchOffers(TransferReason material)
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

        public static byte MatchOffersMode(TransferReason material)
        {
            //incoming only mode 0
            //outgoing only mode 1
            //balanced mode 2
            //incoming first mode 3
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
                    return 2;
                case TransferReason.Garbage:
                case TransferReason.Crime:
                case TransferReason.Fire:
                case TransferReason.Dead:
                case TransferReason.Taxi:
                    return 3;
                case TransferReason.GarbageMove:
                case TransferReason.CriminalMove:
                case TransferReason.DeadMove:
                case TransferReason.SnowMove:
                    return 1;
                default: return 2;
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

        public static bool CheckWareHouseForCity(TransferOffer offerIn, TransferOffer offerOut, TransferReason material)
        {
            if (!MoreEffectiveTransfer.warehouseOnlyForCity)
            {
                return false;
            }

            BuildingManager bM = Singleton<BuildingManager>.instance;
            if (bM.m_buildings.m_buffer[offerIn.Building].Info.m_buildingAI is WarehouseAI)
            {
                if (bM.m_buildings.m_buffer[offerOut.Building].Info.m_buildingAI is OutsideConnectionAI)
                    return true;
            }
            else if (bM.m_buildings.m_buffer[offerOut.Building].Info.m_buildingAI is WarehouseAI)
            {
                if (bM.m_buildings.m_buffer[offerIn.Building].Info.m_buildingAI is OutsideConnectionAI)
                    return true;
            }

            return false;
        }

        public static float WareHouseFirst(TransferOffer offerIn, TransferOffer offerOut, TransferReason material)
        {
            if (!MoreEffectiveTransfer.warehouseFirst)
            {
                return 1f;
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
                    return 1f;
            }


            BuildingManager bM = Singleton<BuildingManager>.instance;
            if (bM.m_buildings.m_buffer[offerIn.Building].Info.m_buildingAI is WarehouseAI)
            {
                return 1000f;
            }
            else if (bM.m_buildings.m_buffer[offerOut.Building].Info.m_buildingAI is WarehouseAI)
            {
                return 1000f;
            }

            return 1f;
        }

        public static float ApplyPriority(TransferOffer offerIn, TransferOffer offerOut, TransferReason material, bool isOfferIn)
        {
            if (!MoreEffectiveTransfer.applyPrority)
            {
                return 1f;
            }

            bool canApplyPriority = false;
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
                case TransferReason.Garbage:
                case TransferReason.Snow:
                case TransferReason.RoadMaintenance:
                case TransferReason.ParkMaintenance:
                case TransferReason.Crime:
                case TransferReason.Fire:
                case TransferReason.GarbageMove:
                case TransferReason.CriminalMove:
                case TransferReason.DeadMove:
                case TransferReason.Dead:
                case TransferReason.SnowMove:
                case TransferReason.Taxi:
                    canApplyPriority = true; break;
                default: canApplyPriority = false; break;
            }

            float priority = 1f;

            if (canApplyPriority)
            {
                if (isOfferIn)
                {
                    priority = offerIn.Priority + 1f;
                }
                else
                {
                    priority = offerOut.Priority + 1f;
                }
            }
            return priority;
        }

        public static void ForgetFailedBuilding(ushort targetBuilding, int idex)
        {
            if (MoreEffectiveTransfer.fixUnRouteTransfer)
            {
                if (targetBuilding != 0)
                {
                    if (MainDataStore.canNotConnectedBuildingIDCount[targetBuilding] != 0)
                    {
                        if (MainDataStore.refreshCanNotConnectedBuildingIDCount[targetBuilding] > 8)
                        {
                            //After several times we can refresh fail building list.
                            MainDataStore.canNotConnectedBuildingIDCount[targetBuilding]--;
                            MainDataStore.canNotConnectedBuildingID[targetBuilding, idex] = MainDataStore.canNotConnectedBuildingID[targetBuilding, MainDataStore.canNotConnectedBuildingIDCount[targetBuilding]];
                            MainDataStore.canNotConnectedBuildingID[targetBuilding, MainDataStore.canNotConnectedBuildingIDCount[targetBuilding]] = 0;
                            if (MoreEffectiveTransfer.debugMode)
                            {
                                DebugLog.LogToFileOnly("ForgetFailedBuilding begin, count = " + MainDataStore.canNotConnectedBuildingIDCount[targetBuilding].ToString());
                                DebugLog.LogToFileOnly("DebugInfo: m_targetBuilding id is " + targetBuilding.ToString());
                                DebugLog.LogToFileOnly("ForgetFailedBuilding end");
                            }
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

        public static bool IsUnRoutedMatch(TransferOffer offerIn, TransferOffer offerOut, TransferReason material)
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
                            ForgetFailedBuilding(targetBuilding, j);
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
                            ForgetFailedBuilding(targetBuilding, j);
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
                            ForgetFailedBuilding(targetBuilding, j);
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
                            ForgetFailedBuilding(targetBuilding, j);
                            return true;
                        }
                    }
                }
                return false;
                //info4.m_buildingAI.StartTransfer(building2, ref buildings2.m_buffer[(int)building2], material, offerOut);
            }
            return false;
        }

        public static void MatchOffers(TransferReason material)
        {
            if (!_init)
            {
                Init();
                _init = true;
            }
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
                                            if (canUseNewMatchOffers)
                                            {
                                                //ApplyPriority
                                                incomingOutgoingDistance = incomingOutgoingDistance / ApplyPriority(incomingOffer, outgoingOfferPre, material, false);
                                                //WareHouse first
                                                incomingOutgoingDistance = incomingOutgoingDistance / WareHouseFirst(incomingOffer, outgoingOfferPre, material);
                                                if ((incomingOutgoingDistance < currentShortestDistance) || currentShortestDistance == -1)
                                                {
                                                    if (!IsUnRoutedMatch(incomingOffer, outgoingOfferPre, material) && !CheckWareHouseForCity(incomingOffer, outgoingOfferPre, material))
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
                                            if (canUseNewMatchOffers)
                                            {
                                                //ApplyPriority
                                                incomingOutgoingDistance = incomingOutgoingDistance / ApplyPriority(incomingOfferPre, outgoingOffer, material, true);
                                                //WareHouse first
                                                incomingOutgoingDistance = incomingOutgoingDistance / WareHouseFirst(incomingOfferPre, outgoingOffer, material);
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
                                                    if (!IsUnRoutedMatch(incomingOfferPre, outgoingOffer, material) && !CheckWareHouseForCity(incomingOfferPre, outgoingOffer, material))
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
        }

        public static void InitDelegate()
        {
            TransferManagerStartTransferDG = FastDelegateFactory.Create<TransferManagerStartTransfer>(typeof(TransferManager), "StartTransfer", instanceMethod: true);
            TransferManagerGetDistanceMultiplierDG = FastDelegateFactory.Create<TransferManagerGetDistanceMultiplier>(typeof(TransferManager), "GetDistanceMultiplier", instanceMethod: false);
        }

        public delegate void TransferManagerStartTransfer(TransferManager TransferManager, TransferReason material, TransferOffer offerOut, TransferOffer offerIn, int delta);
        public static TransferManagerStartTransfer TransferManagerStartTransferDG;

        public delegate float TransferManagerGetDistanceMultiplier(TransferManager.TransferReason material);
        public static TransferManagerGetDistanceMultiplier TransferManagerGetDistanceMultiplierDG;
    }
}