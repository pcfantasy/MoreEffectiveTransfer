using ColossalFramework;
using HarmonyLib;
using MoreEffectiveTransfer.Util;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using System;
using System.Linq;

namespace MoreEffectiveTransfer.Util
{
    [StructLayout(LayoutKind.Sequential)]
    struct PATHFINDPAIR
    {
        ushort sourceBuilding;
        ushort targetBuilding;

        public PATHFINDPAIR(ushort source, ushort target)
        {
            this.sourceBuilding = source;
            this.targetBuilding = target;
        }

        public string PrintKey()
        {
            return $"(src:{sourceBuilding} / tgt:{targetBuilding})";
        }
    }


    public sealed class PathFindFailure
    {
        const int MAX_PATHFIND = 128;
        static Dictionary<PATHFINDPAIR, long> _pathfindFails = new Dictionary<PATHFINDPAIR, long>(MAX_PATHFIND);
        static Dictionary<ushort, int> _pathfindBuildingsCounter = new Dictionary<ushort, int>(MAX_PATHFIND);

        const int MAX_OUTSIDECONNECTIONS = 16;
        static Dictionary<ushort, long> _outsideConnectionFails = new System.Collections.Generic.Dictionary<ushort, long>(MAX_OUTSIDECONNECTIONS);


        static readonly object _dictionaryLock = new object();
        static long lru_lastCleaned;
        static long last_chirp;
        static int total_chirps_sent;
        const long  LRU_INTERVALL = TimeSpan.TicksPerMillisecond * 1000 * 15; //15 sec
        const long  CHIRP_INTERVALL = TimeSpan.TicksPerMillisecond * 1000 * 60; //1 min


        public static int GetTotalChirps() => total_chirps_sent;


        /// <summary>
        /// Add or update failure pair
        /// </summary>
        private static void AddFailPair(ushort source, ushort target)
        {
            long _info;
            PATHFINDPAIR _pair = new PATHFINDPAIR(source, target);
            
            if (_pathfindFails.TryGetValue(_pair, out _info))
            {
                _info = DateTime.Now.Ticks;
                _pathfindFails[_pair] = _info;
            }
            else
            {
                if (_pathfindFails.Count >= MAX_PATHFIND)
                {
                    DebugLog.LogDebug(DebugLog.REASON_PATHFIND, $"Pathfindfailure: Count is {_pathfindFails.Count}, Removing key {_pathfindFails.OrderBy(x => x.Value).First().Key.PrintKey()}");
                    lock (_dictionaryLock)
                    {
                        _pathfindFails.Remove(_pathfindFails.OrderBy(x => x.Value).First().Key);
                    }
                }

                _pathfindFails.Add(_pair, DateTime.Now.Ticks);
                DebugLog.LogDebug(DebugLog.REASON_PATHFIND, $"Pathfindfailure: Added key {_pair.PrintKey()}, Count is {_pathfindFails.Count}");
            }


            UpdateBuildingFailCount(source);
            UpdateBuildingFailCount(target);
        }


        /// <summary>
        /// Add or update outside connection fail
        /// </summary>
        private static void AddOutsideConnectionFail(ushort buildingID)
        {
            long _info;
            if (_outsideConnectionFails.TryGetValue(buildingID, out _info))
            {
                _info = DateTime.Now.Ticks;
                _outsideConnectionFails[buildingID] = _info;
            }
            else
                _outsideConnectionFails.Add(buildingID, DateTime.Now.Ticks);

            DebugLog.LogDebug(DebugLog.REASON_PATHFIND, $"Pathfindfailure: Added outsideconnection fail, connection: {buildingID} ({Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info.name})");
        }


        /// <summary>
        /// Increase buidign fail count
        /// </summary>
        private static void UpdateBuildingFailCount(ushort buildingID)
        {
            int failcount;
            if (_pathfindBuildingsCounter.TryGetValue(buildingID, out failcount))
            {
                failcount++;
                _pathfindBuildingsCounter[buildingID] = failcount;
            }
            else
                _pathfindBuildingsCounter.Add(buildingID, 1);
        }


        /// <summary>
        /// Cleanup old entries by last used
        /// </summary>
        public static void RemoveOldEntries()
        {
            long diffTime = DateTime.Now.Ticks - lru_lastCleaned;
            int failpair_remove_count = 0;
            int failoutside_remove_count = 0;

            if (diffTime > LRU_INTERVALL)
            {
                lru_lastCleaned = DateTime.Now.Ticks;

                lock (_dictionaryLock)
                {
                    foreach (var item in _pathfindFails.Where(kvp => kvp.Value < (DateTime.Now.Ticks - LRU_INTERVALL)).ToList())
                    {
                        _pathfindFails.Remove(item.Key);
                        failpair_remove_count++;
                    }

                    foreach (var item in _outsideConnectionFails.Where(kvp => kvp.Value < (DateTime.Now.Ticks - LRU_INTERVALL)).ToList())
                    {
                        _outsideConnectionFails.Remove(item.Key);
                        failoutside_remove_count++;
                    }
                }

                DebugLog.LogDebug(DebugLog.REASON_PATHFIND, $"Pathfindfailure: LRU removed {failpair_remove_count} pairs + {failoutside_remove_count} outsideconnections, new count is {_pathfindFails.Count} pairs + {_outsideConnectionFails.Count} outsideconnections");
            }
        }


        /// <summary>
        /// Returns true when pair exists in exclusion list
        /// </summary>
        public static bool Find(ushort source, ushort target)
        {
            long _info;
            PATHFINDPAIR _pair = new PATHFINDPAIR(source, target);
            if (_pathfindFails.TryGetValue(_pair, out _info))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns true when buildingID in outsideconnection fail list
        /// </summary>
        public static bool CheckOutsideConnectionFail(ushort buildingID)
        {
            long _info;
            if (_outsideConnectionFails.TryGetValue(buildingID, out _info))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Called from CarAIPatch: record a failed pair
        /// (does not apply to outside connections)
        /// </summary>
        public static void RecordPathFindFailure(ushort vehicleID, ref Vehicle data)
        {
                var sourceBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding];
                var targetBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding];

#if (DEBUG)
                var instB = default(InstanceID);
                instB.Building = data.m_sourceBuilding;
                var sourceName = Singleton<InstanceManager>.instance.GetName(instB);
                instB.Building = data.m_targetBuilding;
                var targetName = Singleton<InstanceManager>.instance.GetName(instB);
                DebugLog.LogDebug(DebugLog.REASON_PATHFIND, $"Pathfindfailure: from '{sourceName}'[{data.m_sourceBuilding}: {sourceBuilding.Info?.name}({sourceBuilding.Info?.m_class})] --> '{targetName}'[{data.m_targetBuilding}: {targetBuilding.Info?.name}({targetBuilding.Info?.m_class})]");
#endif

            if (((data.m_targetBuilding != 0) && !(targetBuilding.Info?.m_buildingAI is OutsideConnectionAI)) &&
                    ((data.m_sourceBuilding != 0) && !(sourceBuilding.Info?.m_buildingAI is OutsideConnectionAI)))
            {
                AddFailPair(data.m_sourceBuilding, data.m_targetBuilding);
            }
            else if ((data.m_targetBuilding != 0) && (targetBuilding.Info?.m_buildingAI is OutsideConnectionAI))
            {
                AddOutsideConnectionFail(data.m_targetBuilding);
            }
            else if ((data.m_sourceBuilding != 0) && (sourceBuilding.Info?.m_buildingAI is OutsideConnectionAI))
            {
                AddOutsideConnectionFail(data.m_sourceBuilding);
            }
        }


        /// <summary>
        /// Create chirper message about pathfind issues
        /// </summary>
        public static void SendPathFindChirp()
        {
            long diffTime = DateTime.Now.Ticks - last_chirp;
            if (diffTime > CHIRP_INTERVALL)
            {
                last_chirp = DateTime.Now.Ticks;

                if ((ModSettings.optionPathfindChirper) && (_pathfindBuildingsCounter.Count > 0))
                {
                    // get top failed building
                    var pair = _pathfindBuildingsCounter.OrderByDescending(x => x.Value).First();
                    ushort buildingKey = pair.Key;
                    int buildingFailedCount = pair.Value;

                    var instB = default(InstanceID);
                    instB.Building = buildingKey;
                    var senderBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingKey];
                    var buildingName = Singleton<InstanceManager>.instance.GetName(instB);
                        
                    if (buildingFailedCount > 1)
                    {
                        uint sender = FindCitizenOfBuilding(senderBuilding);
                        string hashtag = ((senderBuilding.Info.m_class.GetZone() == ItemClass.Zone.ResidentialLow) || (senderBuilding.Info.m_class.GetZone() == ItemClass.Zone.ResidentialHigh)) ? "#ilivethere" : "#iworkthere";
                        Singleton<MessageManager>.instance.QueueMessage(new CustomCitizenMessage(sender, $"@mayor: FIX YOUR ROAD NETWORK!\n{_pathfindBuildingsCounter.Count} unrouted transfers recenty! Most common:\n{buildingName}({senderBuilding.Info?.name}) -- #ROUTING #{buildingFailedCount} FAILS! {hashtag}!", null));
                        DebugLog.LogDebug(DebugLog.REASON_PATHFIND, $"@mayor: FIX YOUR ROAD NETWORK!\n{_pathfindBuildingsCounter.Count} unrouted transfers recenty! Most common:\n{buildingName}({senderBuilding.Info?.name}) -- #ROUTING #{buildingFailedCount} FAILS! {hashtag}!");
                        total_chirps_sent++;
                    }
                }

                // clear building fail counter
                _pathfindBuildingsCounter.Clear();
            }
        }


        private static uint FindCitizenOfBuilding(Building building)
        {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            uint mCitizenUnits = building.m_citizenUnits;

            while (mCitizenUnits != 0)
            {
                uint mNextUnit = citizenManager.m_units.m_buffer[mCitizenUnits].m_nextUnit;
                CitizenUnit.Flags flags = CitizenUnit.Flags.Work | CitizenUnit.Flags.Home;
                if ((ushort)(citizenManager.m_units.m_buffer[mCitizenUnits].m_flags & flags) != 0)
                {
                    return citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen0 != 0 ? citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen0 :
                            citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen1 != 0 ? citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen1 :
                            citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen2 != 0 ? citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen2 :
                            citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen3 != 0 ? citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen3 :
                            citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen4 != 0 ? citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen4 :
                            (uint)0;
                }

                mCitizenUnits = mNextUnit;
            }
            return (uint)0;
        }

    }
}
