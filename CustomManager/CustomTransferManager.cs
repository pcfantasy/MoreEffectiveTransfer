using ColossalFramework;
using ColossalFramework.Plugins;
using MoreEffectiveTransfer.CustomAI;
using MoreEffectiveTransfer.Util;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace MoreEffectiveTransfer.CustomManager
{
    public class CustomTransferManager : TransferManager
    {
        public static bool _init = false;

        // Matching logic
        private enum OFFER_MATCHMODE : int { INCOMING_FIRST = 1, OUTGOING_FIRST = 2, BALANCED = 3 };
        private enum WAREHOUSE_OFFERTYPE : int {  INCOMING = 1, OUTGOING = 2 };


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

            CustomCommonBuildingAI.InitDelegate();
        }

        public delegate void TransferManagerStartTransfer(TransferManager TransferManager, TransferReason material, TransferOffer offerOut, TransferOffer offerIn, int delta);
        public static TransferManagerStartTransfer TransferManagerStartTransferDG;
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

        public static void CheckInit()
        {
            if (_init)
            {
                DebugLog.LogToFileOnly("Checking initializations...");
                DebugLog.LogToFileOnly($"- _TransferManager instance: {_TransferManager}");
                DebugLog.LogToFileOnly($"- _InstanceManager instance: {_InstanceManager}");
                DebugLog.LogToFileOnly($"- _BuildingManager instance: {_BuildingManager}");
                DebugLog.LogToFileOnly($"- _VehicleManager instance: {_VehicleManager}");
                DebugLog.LogToFileOnly($"- _CitizenManager instance: {_CitizenManager}");
                DebugLog.LogToFileOnly($"- _DistrictManager instance: {_DistrictManager}");

                DebugLog.LogToFileOnly("Checking delegates...");
                DebugLog.LogToFileOnly($"- TransferManagerStartTransferDG instance: {TransferManagerStartTransferDG}");
                DebugLog.LogToFileOnly($"- CustomCommonBuildingAI.CalculateOwnVehicles instance: {CustomCommonBuildingAI.CalculateOwnVehicles}");

                if ((_TransferManager != null) && (_InstanceManager != null) && (_BuildingManager != null) && (_VehicleManager != null) && (_CitizenManager != null) &&
                    (_DistrictManager != null) && (TransferManagerStartTransferDG != null) && (CustomCommonBuildingAI.CalculateOwnVehicles != null))
                    DebugLog.LogToFileOnly("ALL INIT CHECKS PASSED. This should work.");
                else
                {
                    DebugLog.LogAll("PROBLEM DETECTED! SOME MODS ARE CAUSING INCOMPATIBILITIES! Generating mod list and harmony report...");
                    DebugLog.ReportAllHarmonyPatches();
                    DebugLog.ReportAllMods();
                    DebugLog.LogAll("PROBLEM DETECTED! SOME MODS ARE CAUSING INCOMPATIBILITIES! Please check log >MoreEffectiveTransfer.txt< in CSL directory!", true);
                }
            }
        }


        public static bool CanUseNewMatchOffers(TransferReason material)
        {
            switch (material)
            {
                // Goods & Service for new transfer manager:

                // Block 1: Services:
                case TransferReason.Garbage:
                case TransferReason.GarbageMove:
                case TransferReason.GarbageTransfer:
                case TransferReason.Crime:
                case TransferReason.CriminalMove:
                case TransferReason.Fire:
                case TransferReason.Fire2:
                case TransferReason.ForestFire:                
                case TransferReason.Sick:
                case TransferReason.Sick2:
                case TransferReason.SickMove:
                case TransferReason.Dead:
                case TransferReason.DeadMove:               
                case TransferReason.Collapsed:
                case TransferReason.Collapsed2:
                case TransferReason.Snow:
                case TransferReason.SnowMove:
                case TransferReason.RoadMaintenance:            
                case TransferReason.ParkMaintenance:
                case TransferReason.Mail:
                case TransferReason.UnsortedMail:
                case TransferReason.SortedMail:
                case TransferReason.IncomingMail:
                case TransferReason.OutgoingMail:
                case TransferReason.Taxi:                      
                
                // Block 2: Goods
                    
                case TransferReason.Goods:
                case TransferReason.Oil:
                case TransferReason.Ore:
                case TransferReason.Logs:
                case TransferReason.Grain:
                case TransferReason.Coal:
                case TransferReason.Petrol:
                case TransferReason.Food:
                case TransferReason.Lumber:
                case TransferReason.AnimalProducts:
                case TransferReason.Flours:
                case TransferReason.Paper:
                case TransferReason.PlanedTimber:
                case TransferReason.Petroleum:
                case TransferReason.Plastics:
                case TransferReason.Glass:
                case TransferReason.Metals:
                case TransferReason.LuxuryProducts:
                case TransferReason.Fish:

                    return true;
                
              // Default: use vanilla transfermanager (esp. citizens)
              default: 
                    return false;
            }
        }


        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static OFFER_MATCHMODE GetMatchOffersMode(TransferReason material)
        {
            //incoming first: pick highest priority outgoing offers by distance
            //outgoing first: try to fulfill all outgoing offers by descending priority. incoming offer mapped by distance only (priority not relevant).
            //balanced: outgoing/incoming together by priorty descending
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
                case TransferReason.Fish:
                //warehouse incoming behaviour: empty=prio 0; balanced=prio 0-2; fill=prio 2;
                //warehouse outgoing behaviour: empty=prio 2 ; balanced=prio 0-2; fill=prio 0;
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
                case TransferReason.Collapsed:          //Collapsed: outgoing (passive)
                case TransferReason.Collapsed2:         //Collapsed2: helicopter
                case TransferReason.Snow:               //outgoing (passive) from netsegements, incoming (active) from snowdumps
                case TransferReason.Mail:               //outgoing (passive) from buidings, incoming(active) from postoffice
                case TransferReason.OutgoingMail:       //outside connections incoming(passive)
                    return OFFER_MATCHMODE.OUTGOING_FIRST;

                case TransferReason.GarbageMove:        //GarbageMove: outgoing (active) from emptying landfills, incoming (passive) from receiving landfills/wastetransferfacilities/wasteprocessingcomplex
                case TransferReason.CriminalMove:       //TODO: unclear
                case TransferReason.SickMove:           //TODO: unclear
                case TransferReason.DeadMove:           //outgoing (active) from emptying, incoming (passive) from receiving
                case TransferReason.SnowMove:           //outgoing (active) from emptying snowdumps, incoming (passive) from receiving
                case TransferReason.RoadMaintenance:    //incoming (passive) from netsegments, outgoing (active) from maintenance depot
                case TransferReason.ParkMaintenance:    //incoming (passive) from park main gate building, 
                case TransferReason.IncomingMail:       //outside connections outgoing(active), incoming(passive) from postsortingfacilities
                case TransferReason.SortedMail:         //outside connections outgoing(active), incoming(passive) from postoffice
                case TransferReason.UnsortedMail:       //outgoing(active) from ???, incoming(passive) from postsortingfacilities
                case TransferReason.Taxi:               //incoming(passive) from citizens and taxistands, outgoing(active) from depots/taxis
                    return OFFER_MATCHMODE.INCOMING_FIRST;

                default: 
                    return OFFER_MATCHMODE.BALANCED;
            }
        }


        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static bool IsLocalUse(ref TransferOffer offerIn, ref TransferOffer offerOut, TransferReason material, int priority, out float distanceModifier)
        {
            const int PRIORITY_THRESHOLD_LOCAL = 3;     //upper prios also get non-local fulfillment
            const float LOCAL_DISTRICT_MODIFIER = 0.25f;   //modifier for distance within same district
            bool isMoveTransfer = false;
            bool isLocal = false;
            distanceModifier = 1.0f;

            // guard: current option setting?
            if (!MoreEffectiveTransfer.optionPreferLocalService)
                return true;

            // priority above threshold -> any service is OK!
            if (priority >= PRIORITY_THRESHOLD_LOCAL)
            { 
                isLocal = true;
                // continue logic to set distanceModifier for service within same district
            }

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
                case TransferReason.Collapsed:
                case TransferReason.Collapsed2:
                case TransferReason.ParkMaintenance:
                case TransferReason.Mail:
                case TransferReason.SortedMail:
                case TransferReason.Taxi:
                    break;

                // Goods subject to prefer local:
                // -none-

                // Material Transfers for services subject to policy:
                case TransferReason.GarbageMove:
                case TransferReason.GarbageTransfer:
                case TransferReason.CriminalMove:
                case TransferReason.SickMove:
                case TransferReason.DeadMove:
                case TransferReason.SnowMove:
                    isMoveTransfer = true;      //Move Transfers: incoming offer is passive, allow move/emptying to global district buildings
                    break;

                default:
                    return true;                //guard: dont apply district logic to other materials
            }

            // determine buildings or vehicle parent buildings
            ushort buildingIncoming = 0, buildingOutgoing = 0;

            if (offerIn.Building != 0) buildingIncoming = offerIn.Building;
            else if (offerIn.Vehicle != 0) buildingIncoming = _VehicleManager.m_vehicles.m_buffer[offerIn.Vehicle].m_sourceBuilding;
            else if (offerIn.Citizen != 0) buildingIncoming = _CitizenManager.m_citizens.m_buffer[offerIn.Citizen].m_homeBuilding;

            if (offerOut.Building != 0) buildingOutgoing = offerOut.Building;
            else if (offerOut.Vehicle != 0) buildingOutgoing = _VehicleManager.m_vehicles.m_buffer[offerOut.Vehicle].m_sourceBuilding;
            else if (offerOut.Citizen != 0) buildingOutgoing = _CitizenManager.m_citizens.m_buffer[offerOut.Citizen].m_homeBuilding;

            // get respective districts
            byte districtIncoming = _DistrictManager.GetDistrict(_BuildingManager.m_buildings.m_buffer[buildingIncoming].m_position);
            byte districtOutgoing = _DistrictManager.GetDistrict(_BuildingManager.m_buildings.m_buffer[buildingOutgoing].m_position);

            // return true if: both within same district, or active offer is outside district ("in global area")
            if ((districtIncoming == districtOutgoing)
                  || (offerIn.Active && districtIncoming == 0)
                  || (offerOut.Active && districtOutgoing == 0)
                  || (isMoveTransfer && districtIncoming == 0)
               )
            {
                isLocal = true;

                // really same district? set modifier!
                if ((districtIncoming == districtOutgoing) && (districtIncoming != 0))
                    distanceModifier = LOCAL_DISTRICT_MODIFIER;
            }

            return isLocal;
        }


        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static float WarehouseFirst(ref TransferOffer offer, TransferReason material, WAREHOUSE_OFFERTYPE whInOut)
        {
            const float WAREHOUSE_MODIFIER = 0.1f;   //modifier for distance for warehouse

            if (!MoreEffectiveTransfer.optionWarehouseFirst)
                return 1f;

            if (offer.Exclude)  //TransferOffer.Exclude is only ever set by WarehouseAI!
            {
                Building.Flags isFilling = (_BuildingManager.m_buildings.m_buffer[offer.Building].m_flags & Building.Flags.Filling);
                Building.Flags isEmptying = (_BuildingManager.m_buildings.m_buffer[offer.Building].m_flags & Building.Flags.Downgrading);

                // Filling Warehouses dont like to fulfill outgoing offers,
                // emptying warehouses dont like to fulfill incoming offers
                if ((whInOut == WAREHOUSE_OFFERTYPE.INCOMING && isEmptying != Building.Flags.None) ||
                    (whInOut == WAREHOUSE_OFFERTYPE.OUTGOING && isFilling != Building.Flags.None))
                    return WAREHOUSE_MODIFIER * 2;   //distance factorSqrt x2 further away

                return WAREHOUSE_MODIFIER;       //WarehouseDIstanceFactorSqr = 1 / 10
            }

            return 1f;
        }


        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static bool WarehouseCanTransfer(ref TransferOffer incomingOffer, ref TransferOffer outgoingOffer, TransferReason material, WAREHOUSE_OFFERTYPE whInOut)
        {
            if (!MoreEffectiveTransfer.optionWarehouseReserveTrucks)
                return true;

            if (!(outgoingOffer.Exclude && outgoingOffer.Active))   //further checks only relevant if outgoing from warehouse and active
                return true;

            // is outgoing a warehouse with active delivery, and is couterpart incoming an outside connection?
            if ((outgoingOffer.Exclude) && (outgoingOffer.Active) && (incomingOffer.Building != 0))
            {
                if (_BuildingManager.m_buildings.m_buffer[incomingOffer.Building].Info?.m_buildingAI is OutsideConnectionAI)
                {
                    int total = (_BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].Info?.m_buildingAI as WarehouseAI).m_truckCount;
                    int count = 0, cargo = 0, capacity = 0, outside = 0;

                    //Building.Flags isEmptying = (_BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].m_flags & Building.Flags.Downgrading);

                    float maxExport = (total * 0.75f);

                    CustomCommonBuildingAI.CalculateOwnVehicles(_BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].Info?.m_buildingAI as WarehouseAI,
                                           outgoingOffer.Building, ref _BuildingManager.m_buildings.m_buffer[outgoingOffer.Building], material, ref count, ref cargo, ref capacity, ref outside);

                    DebugLog.DebugMsg($"       ** checking canTransfer: total: {total}, ccco: {count}/{cargo}/{capacity}/{outside} => {((float)(outside + 1f) > maxExport)}");
                    if ((float)(outside + 1f) > maxExport)
                        return false;
                }
            }

            return true;
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
                DebugLog.DebugMsg($"   in #{i}: act {incomingOffer.Active}, excl {incomingOffer.Exclude}, amt {incomingOffer.Amount}, bvcnt {incomingOffer.Building}/{incomingOffer.Vehicle}/{incomingOffer.Citizen}/{incomingOffer.NetSegment}/{incomingOffer.TransportLine} name={bname}");
            }

            for (int i = 0; i < offerCountOutgoing; i++)
            {
                TransferOffer outgoingOffer = m_outgoingOffers[offset * 256 + i];
                String bname = DebugInspectOffer(ref outgoingOffer);
                DebugLog.DebugMsg($"   out #{i}: act {outgoingOffer.Active}, excl {outgoingOffer.Exclude}, amt {outgoingOffer.Amount}, bvcnt {outgoingOffer.Building}/{outgoingOffer.Vehicle}/{outgoingOffer.Citizen}/{outgoingOffer.NetSegment}/{outgoingOffer.TransportLine} name={bname}");
            }
        }


        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe public static void MatchOffers(TransferReason material)
        {
            const int ALL_PRIORITIES = 0;
            const int REJECT_LOW_PRIORITY = 1;  //reject priorities below

            // delayed initialization until first call
            if (!_init)
            {
                Init();
                CheckInit();
            }


            // guard: ignore transferreason.none
            if (material == TransferReason.None)
                return;

            // function scope variables
            int offer_offset;

            // DEBUG LOGGING
            DebugLog.DebugMsg($"-- TRANSFER REASON: {material.ToString()}, amt in {m_incomingAmount[(int)material]}, amt out {m_outgoingAmount[(int)material]}");
#if (DEBUG)
            for (int priority = 7; priority >= 0; --priority)
            {
                offer_offset = (int)material * 8 + priority;
                int offerCountIncoming = m_incomingCount[offer_offset];
                int offerCountOutgoing = m_outgoingCount[offer_offset];

                DebugLog.DebugMsg($"   #Offers@priority {priority} : {offerCountIncoming} in, {offerCountOutgoing} out");
                DebugPrintAllOffers(material, offer_offset, offerCountIncoming, offerCountOutgoing);
            }
#endif


            // Select offer matching algorithm
            OFFER_MATCHMODE match_mode = GetMatchOffersMode(material);


            // OUTGOING FIRST mode - try to fulfill all outgoing requests by finding incomings by distance
            // -------------------------------------------------------------------------------------------
            if (match_mode == OFFER_MATCHMODE.OUTGOING_FIRST)
            {
                DebugLog.DebugMsg($"   ###MatchMode OUTGOING FIRST###");
                bool has_counterpart_offers = true;

                // 1st loop: all OUTGOING offers by descending priority
                for (int priority = 7; priority >= ALL_PRIORITIES; --priority)
                {
                    offer_offset = (int)material * 8 + priority;
                    int prio_lower_limit = Math.Max(0, 2 - priority);

                    // loop all offers within this priority
                    for (int offerIndex = 0; offerIndex < m_outgoingCount[offer_offset]; offerIndex++)
                    {
                        if (m_incomingAmount[(int)material] <= 0 || !has_counterpart_offers)
                        {
                            DebugLog.DebugMsg($"   ### MATCHMODE EXIT, amt in {m_incomingAmount[(int)material]}, amt out {m_outgoingAmount[(int)material]}, has_counterparts {has_counterpart_offers} ###");
                            goto END_OFFERMATCHING;
                        }

                        has_counterpart_offers = MatchOutgoingOffer(material, offer_offset, priority, prio_lower_limit, offerIndex);
                    }
                } //end loop priority

            } //end OFFER_MATCHMODE.OUTGOING_FIRST


            // INCOMING FIRST mode - try to fulfill all incoming offers by finding outgoings by distance
            // -------------------------------------------------------------------------------------------
            if (match_mode == OFFER_MATCHMODE.INCOMING_FIRST)
            {
                DebugLog.DebugMsg($"   ###MatchMode INCOMING FIRST###");
                bool has_counterpart_offers = true;

                // 1st loop: all INCOMING offers by descending priority
                for (int priority = 7; priority >= ALL_PRIORITIES; --priority)
                {
                    offer_offset = (int)material * 8 + priority;
                    int prio_lower_limit = Math.Max(0, 2 - priority);

                    // loop all offers within this priority
                    for (int offerIndex = 0; offerIndex < m_incomingCount[offer_offset]; offerIndex++)
                    {
                        if (m_outgoingAmount[(int)material] <= 0 || !has_counterpart_offers)
                        {
                            DebugLog.DebugMsg($"   ### MATCHMODE EXIT, amt in {m_incomingAmount[(int)material]}, amt out {m_outgoingAmount[(int)material]}, has_counterparts {has_counterpart_offers} ###");
                            goto END_OFFERMATCHING;
                        }

                        has_counterpart_offers = MatchIncomingOffer(material, offer_offset, priority, prio_lower_limit, offerIndex);
                    }
                } //end loop priority

            } //end OFFER_MATCHMODE.INCOMING_FIRST


            // BALANCED mode - match incoming/outgoing one by one by distance, descending priority
            // -------------------------------------------------------------------------------------------
            if (match_mode == OFFER_MATCHMODE.BALANCED)
            {
                DebugLog.DebugMsg($"   ###MatchMode BALANCED###");
                bool has_counterpart_offers = true;

                // loop incoming offers by descending priority
                for (int priority = 7; priority >= REJECT_LOW_PRIORITY; --priority)
                {
                    offer_offset = (int)material * 8 + priority;
                    int prio_lower_limit = Math.Max(0, 2 - priority);   //2 and higher: match all couterparts, 0: match only 7 down to 2, 1: match 7..1                    
                    int maxoffers = Math.Max(m_incomingCount[offer_offset], m_outgoingCount[offer_offset]);
                                                                        
                    for (int offerIndex = 0, indexIn=0, indexOut=0; offerIndex < maxoffers; offerIndex++, indexIn++, indexOut++) // loop all incoming+outgoing offers within this priority
                    {
                        if (m_incomingAmount[(int)material] <= 0 || m_outgoingAmount[(int)material] <= 0 || !has_counterpart_offers)
                        {
                            DebugLog.DebugMsg($"   ### MATCHMODE EXIT, amt in {m_incomingAmount[(int)material]}, amt out {m_outgoingAmount[(int)material]}, has_counterparts {has_counterpart_offers} ###");
                            goto END_OFFERMATCHING;
                        }

                        has_counterpart_offers = false;
                        if (indexIn  < m_incomingCount[offer_offset]) has_counterpart_offers |= MatchIncomingOffer(material, offer_offset, priority, prio_lower_limit, indexIn);
                        if (indexOut < m_outgoingCount[offer_offset]) has_counterpart_offers |= MatchOutgoingOffer(material, offer_offset, priority, prio_lower_limit, indexOut);
                    }

                } //end loop priority

            } //end OFFER_MATCHMODE.BALANCED


        END_OFFERMATCHING:
            // finally: clear everything, including unmatched offers!
            ClearAllTransferOffers(material);
        }


        /// <returns>counterpartmacthesleft?</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private static bool MatchIncomingOffer(TransferReason material, int offer_offset, int priority, int prio_lower_limit, int offerIndex)
        {
            // Get incoming offer reference:
            ref TransferOffer incomingOffer = ref m_incomingOffers[offer_offset * 256 + offerIndex];

            // guard: offer valid?
            if (incomingOffer.Amount <= 0) return true;

            DebugLog.DebugMsg($"   ###Matching INCOMING offer: {DebugInspectOffer(ref incomingOffer)}, priority: {priority}, remaining amount outgoing: {m_outgoingAmount[(int)material]}");

            int bestmatch_position = -1;
            float bestmatch_distance = float.MaxValue;
            bool counterpartMatchesLeft = false;

            // loop through all matching counterpart offers and find closest one
            for (int counterpart_prio = 7; counterpart_prio >= prio_lower_limit; --counterpart_prio)
            {
                int counterpart_offset = (int)material * 8 + counterpart_prio;
                for (int counterpart_index = 0; counterpart_index < m_outgoingCount[counterpart_offset]; counterpart_index++)
                {
                    ref TransferOffer outgoingOffer = ref m_outgoingOffers[counterpart_offset * 256 + counterpart_index];

                    // guards: out=in same? exclude offer (already used?)
                    if ((outgoingOffer.Amount == 0) || (outgoingOffer.m_object == incomingOffer.m_object)) continue;

                    //guard: if both are warehouse, prevent low prio inter-warehouse transfers
                    if ((incomingOffer.Exclude) && (outgoingOffer.Exclude) && (counterpart_prio < (prio_lower_limit + 1))) continue;

                    // CHECK OPTION: preferlocalservice
                    float districtFactor = 1f;
                    bool isLocalAllowed = IsLocalUse(ref incomingOffer, ref outgoingOffer, material, priority, out districtFactor);

                    // CHECK OPTION: WarehouseFirst
                    float distanceFactor = WarehouseFirst(ref outgoingOffer, material, WAREHOUSE_OFFERTYPE.OUTGOING);

                    // CHECK OPTION: WarehouseReserveTrucks
                    bool canTransfer = WarehouseCanTransfer(ref incomingOffer, ref outgoingOffer, material, WAREHOUSE_OFFERTYPE.OUTGOING);

                    float distance = Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position) * distanceFactor * districtFactor;
                    if ((isLocalAllowed && canTransfer) && (distance < bestmatch_distance))
                    {
                        bestmatch_position = counterpart_offset * 256 + counterpart_index;
                        bestmatch_distance = distance;
                    }

                    counterpartMatchesLeft = true;
                    DebugLog.DebugMsg($"       -> Matching outgoing offer: {DebugInspectOffer(ref outgoingOffer)}, amt {outgoingOffer.Amount}, local: {isLocalAllowed}, distance: {distance}@{districtFactor}/{distanceFactor}, bestmatch: {bestmatch_distance}");
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

                // reduce offer amount
                incomingOffer.Amount -= deltaamount;
                m_outgoingOffers[bestmatch_position].Amount -= deltaamount;
                m_incomingAmount[(int)material] -= deltaamount;
                m_outgoingAmount[(int)material] -= deltaamount;
            }

            return counterpartMatchesLeft;
        }


        /// <returns>counterpartmacthesleft?</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe private static bool MatchOutgoingOffer(TransferReason material, int offer_offset, int priority, int prio_lower_limit, int offerIndex)
        {
            // Get Outgoing offer reference:
            ref TransferOffer outgoingOffer = ref m_outgoingOffers[offer_offset * 256 + offerIndex];

            // guard: offer valid?
            if (outgoingOffer.Amount <= 0) return true;

            DebugLog.DebugMsg($"   ###Matching OUTGOING offer: {DebugInspectOffer(ref outgoingOffer)}, priority: {priority}, remaining amount incoming: {m_incomingAmount[(int)material]}");

            int bestmatch_position = -1;
            float bestmatch_distance = float.MaxValue;
            bool counterpartMatchesLeft = false;

            // loop through all matching counterpart offers and find closest one
            for (int counterpart_prio = 7; counterpart_prio >= prio_lower_limit; --counterpart_prio)
            {
                int counterpart_offset = (int)material * 8 + counterpart_prio;
                for (int counterpart_index = 0; counterpart_index < m_incomingCount[counterpart_offset]; counterpart_index++)
                {
                    ref TransferOffer incomingOffer = ref m_incomingOffers[counterpart_offset * 256 + counterpart_index];

                    // guards: out=in same? exclude offer (already used?)
                    if ((incomingOffer.Amount == 0) || (outgoingOffer.m_object == incomingOffer.m_object)) continue;

                    //guard: if both are warehouse, prevent low prio inter-warehouse transfers
                    if ((outgoingOffer.Exclude) && (incomingOffer.Exclude) && (counterpart_prio < (prio_lower_limit + 1))) continue;

                    // CHECK OPTION: preferlocalservice
                    float districtFactor = 1f;
                    bool isLocalAllowed = IsLocalUse(ref incomingOffer, ref outgoingOffer, material, priority, out districtFactor);

                    // CHECK OPTION: WarehouseFirst
                    float distanceFactor = WarehouseFirst(ref incomingOffer, material, WAREHOUSE_OFFERTYPE.INCOMING);

                    // CHECK OPTION: WarehouseReserveTrucks
                    bool canTransfer = WarehouseCanTransfer(ref incomingOffer, ref outgoingOffer, material, WAREHOUSE_OFFERTYPE.OUTGOING);

                    float distance = Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position) * distanceFactor * districtFactor;
                    if ((isLocalAllowed && canTransfer) && (distance < bestmatch_distance))
                    {
                        bestmatch_position = counterpart_offset * 256 + counterpart_index;
                        bestmatch_distance = distance;
                    }

                    counterpartMatchesLeft = true;
                    DebugLog.DebugMsg($"       -> Matching incoming offer: {DebugInspectOffer(ref incomingOffer)}, amt {incomingOffer.Amount}, local: {isLocalAllowed}, distance: {distance}@{districtFactor}/{distanceFactor}, bestmatch: {bestmatch_distance}");
                }
            }

            // Select bestmatch
            if (bestmatch_position != -1)
            {
                DebugLog.DebugMsg($"       -> Selecting bestmatch: {DebugInspectOffer(ref m_incomingOffers[bestmatch_position])}");
                // ATTENTION: last incomingOffer is NOT necessarily bestmatch!

                // Start the transfer
                int deltaamount = Math.Min(outgoingOffer.Amount, m_incomingOffers[bestmatch_position].Amount);
                TransferManagerStartTransferDG(Singleton<TransferManager>.instance, material, outgoingOffer, m_incomingOffers[bestmatch_position], deltaamount);

                // reduce offer amount
                outgoingOffer.Amount -= deltaamount;
                m_incomingOffers[bestmatch_position].Amount -= deltaamount;
                m_incomingAmount[(int)material] -= deltaamount;
                m_outgoingAmount[(int)material] -= deltaamount;
            }

            return counterpartMatchesLeft;
        }


        [MethodImpl(256)] //=[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ClearAllTransferOffers(TransferReason material)
        {
            for (int k = 0; k < 8; ++k)
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