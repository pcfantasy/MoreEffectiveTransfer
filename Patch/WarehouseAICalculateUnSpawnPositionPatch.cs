using ColossalFramework.Math;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace MoreEffectiveTransfer.Patch
{
    [HarmonyPatch]
    class WarehouseAICalculateUnspawnPositionPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(WarehouseAI).GetMethod("CalculateUnspawnPosition", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Randomizer).MakeByRefType(), typeof(VehicleInfo), typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType() }, null);
        }
        public static void Postfix(ref Building data, ref Vector3 position, ref Vector3 target)
        {
            if (MoreEffectiveTransfer.warehouseSpawnUnSpawnFix)
            {
                if (data.Info.m_buildingAI is WarehouseAI)
                {
                    //Move UnspawnPosition
                    var moveDistance = data.Width * 8f / 3f;
                    var vector = position - data.m_position;
                    vector = VectorUtils.NormalizeXZ(vector);
                    vector = new Vector3(-vector.z, 0, vector.x);
                    position += moveDistance * vector;
                    target += moveDistance * vector;

                    vector = data.m_position - position;
                    vector = VectorUtils.NormalizeXZ(vector);
                    vector = new Vector3(vector.x, 0, vector.z);
                    position += 8 * vector;
                    target += 8 * vector;
                }
            }
        }
    }
}
