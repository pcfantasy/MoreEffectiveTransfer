using ColossalFramework.Math;
using Harmony;
using System;
using System.Reflection;
using UnityEngine;

namespace MoreEffectiveTransfer.Patch
{
    [HarmonyPatch]
    class WarehouseAICalculateSpawnPositionPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(WarehouseAI).GetMethod("CalculateSpawnPosition", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Randomizer).MakeByRefType(), typeof(VehicleInfo), typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType()}, null);
        }
        public static void Postfix(ref Building data, ref Vector3 position, ref Vector3 target)
        {
            if (MoreEffectiveTransfer.advancedWarehouse)
            {
                //Move SpawnPosition
                var vector = position - data.m_position;
                vector = VectorUtils.NormalizeXZ(vector);
                vector = new Vector3(vector.z, 0, -vector.x);
                position += 8 * vector;
                target += 8 * vector;

                vector = data.m_position - position;
                vector = VectorUtils.NormalizeXZ(vector);
                vector = new Vector3(vector.x, 0, vector.z);
                position += 8 * vector;
                target += 8 * vector;
            }
        }
    }
}
