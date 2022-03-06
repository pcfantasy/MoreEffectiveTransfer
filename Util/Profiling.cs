using MoreEffectiveTransfer.CustomManager;
using MoreEffectiveTransfer.Util;
using System.Diagnostics;

namespace MoreEffectiveTransfer
{
    internal static class Profiling
    {

        public static System.Diagnostics.Stopwatch timerVanilla = new System.Diagnostics.Stopwatch();
        public static System.Diagnostics.Stopwatch timerMETM = new System.Diagnostics.Stopwatch();
        public static System.Diagnostics.Stopwatch timerMETM_StartTransfers = new System.Diagnostics.Stopwatch();

        public static long timerCounterVanilla = 0;
        public static long timerCounterMETM = 0;


        [Conditional("PROFILE")]
        public static void PrintProfilingStats()
        {
            DebugLog.LogInfo("--- PROFILING STATISTICS ---");
            float msPerInvVanilla = (1.0f * Profiling.timerVanilla.ElapsedMilliseconds / Profiling.timerCounterVanilla / 1.0f);
            float msPerInvMETM = (1.0f * Profiling.timerMETM.ElapsedMilliseconds / Profiling.timerCounterMETM / 1.0f);
            float msPerInvMETM_ST = (1.0f * Profiling.timerMETM_StartTransfers.ElapsedMilliseconds / Profiling.timerCounterMETM / 1.0f);
            DebugLog.LogInfo($"- VANILLA TRANSFER MANAGER: NUM INVOCATIONS: {Profiling.timerCounterVanilla}, TOTAL MS: {Profiling.timerVanilla.ElapsedMilliseconds}, AVG TIME/INVOCATION: {msPerInvVanilla}ms");
            DebugLog.LogInfo($"-     NEW TRANSFER MANAGER: NUM INVOCATIONS: {Profiling.timerCounterMETM}, TOTAL MS: {Profiling.timerMETM.ElapsedMilliseconds}, AVG TIME/INVOCATION: {msPerInvMETM}ms");
            DebugLog.LogInfo($"-     NEW TRANSFER MANAGER: ./. MS StartTransfers: {Profiling.timerMETM_StartTransfers.ElapsedMilliseconds}, AVG TIME/INVOCATION: {msPerInvMETM_ST}ms");
            DebugLog.LogInfo($"-     NEW TRANSFER MANAGER: max queued transferjobs: {TransferJobPool.Instance.GetMaxUsage()}");
            DebugLog.LogInfo($"-     NEW TRANSFER MANAGER: max transfer ringbuffer usage: {CustomTransferDispatcher.Instance.GetMaxUsage()}");
            DebugLog.LogInfo($"-     NEW TRANSFER MANAGER: total chirps about routing issues: {PathFindFailure.GetTotalChirps()}");
            DebugLog.LogInfo("--- END PROFILING STATISTICS ---");
        }
    }

}