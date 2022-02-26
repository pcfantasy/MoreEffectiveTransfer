﻿using ColossalFramework;
using MoreEffectiveTransfer.Util;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;


namespace MoreEffectiveTransfer.CustomManager
{
    /// <summary>
    /// TransferJob: individual work package for match maker thread
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class TransferJob
    {
        public TransferManager.TransferReason material;
        public ushort m_outgoingCount;
        public ushort m_incomingCount;
        public int m_outgoingAmount;
        public int m_incomingAmount;

        public TransferManager.TransferOffer[] m_outgoingOffers; //Size: TransferManager.TRANSFER_OFFER_COUNT * TRANSFER_PRIORITY_COUNT
        public TransferManager.TransferOffer[] m_incomingOffers; //Size: TransferManager.TRANSFER_OFFER_COUNT * TRANSFER_PRIORITY_COUNT

        public TransferJob()
        {
            m_outgoingOffers = new TransferManager.TransferOffer[TransferManager.TRANSFER_OFFER_COUNT * TransferManager.TRANSFER_PRIORITY_COUNT];
            m_incomingOffers = new TransferManager.TransferOffer[TransferManager.TRANSFER_OFFER_COUNT * TransferManager.TRANSFER_PRIORITY_COUNT];
        }
    }


    /// <summary>
    /// TransferJobPool: pool of TransferJobs
    /// </summary>
    public sealed class TransferJobPool
    {
        private static TransferJobPool _instance = null;
        private Stack<TransferJob> pooledJobs = null;
        private static readonly object _poolLock = new object();
        private int _usageCount = 0;
        private int _maxUsageCount = 0;

        public static TransferJobPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TransferJobPool();
                }
                return _instance;
            }
        }

        public void Initialize()
        {
            // allocate object pool of work packages
            pooledJobs = new Stack<TransferJob>(TransferManager.TRANSFER_REASON_COUNT);

            for(int i = 0; i < TransferManager.TRANSFER_REASON_COUNT; i++)
            {
                pooledJobs.Push(new TransferJob());
            }

            DebugLog.LogDebug($"TransferJobPool initialized, pool stack size is {pooledJobs.Count}");
        }

        public void Delete()
        {
            DebugLog.LogDebug($"Deleting instance: {_instance}");
            // unallocate object pool of work packages
            pooledJobs.Clear();
            pooledJobs = null;
            TransferJobPool._instance = null;
        }

        public TransferJob Lease()
        {
            lock (_poolLock)
            {
                if (pooledJobs.Count > 0)
                {
                    _usageCount++;
                    _maxUsageCount = (_usageCount > _maxUsageCount) ? _usageCount : _maxUsageCount;
                    return pooledJobs.Pop();
                }
                else
                {
                    DebugLog.LogError("TransferJobPool: pooled jobs exhausted!");
                    return null;
                }
            }
        }

        public void Return(TransferJob job)
        {
            lock (_poolLock)
            {
                _usageCount--;
                job.material = TransferManager.TransferReason.None; //flag as unused
                pooledJobs.Push(job);
            }
        }

        public int GetMaxUsage() => _maxUsageCount;
    }


    /// <summary>
    /// CustomTransferDisptacher: coordinate with match maker thread
    /// </summary>
    public sealed class CustomTransferDispatcher
    {
        private static CustomTransferDispatcher _instance = null;
        private Queue<TransferJob> workQueue = null;
        private static readonly object _workQueueLock = new object();
        public static EventWaitHandle _waitHandle = new AutoResetEvent(false);
        public static Thread _transferThread = null;


        // References to game functionalities:
        private static TransferManager _TransferManager = null;

        // Vanilla TransferManager internal fields and arrays
        private TransferManager.TransferOffer[] m_outgoingOffers;
        private TransferManager.TransferOffer[] m_incomingOffers;
        private ushort[] m_outgoingCount;
        private ushort[] m_incomingCount;
        private int[] m_outgoingAmount;
        private int[] m_incomingAmount;


        public static CustomTransferDispatcher Instance
        {
            get {
                    if (_instance == null)
                    {
                        _instance = new CustomTransferDispatcher();
                    }
                    return _instance;
            }
        }

        public void Initialize()
        {
            // bind vanilla transfermanager fields
            _TransferManager = Singleton<TransferManager>.instance;
            if (_TransferManager == null)
            {
                DebugLog.LogError("ERROR: No instance of TransferManager found!",true);
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


            // allocate object pool of work packages
            workQueue = new Queue<TransferJob>(TransferManager.TRANSFER_REASON_COUNT);

            DebugLog.LogDebug($"CustomTransferDispatcher initialized, workqueue count is {workQueue.Count}");
        }

        public void Delete()
        {
            DebugLog.LogDebug($"Deleting instance: {_instance}");
            // unallocate object pool of work packages
            workQueue.Clear();
            workQueue = null;
            CustomTransferDispatcher._instance = null;
        }

        /// <summary>
        /// Thread-safe Enqueue
        /// </summary>
        /// <param name="job"></param>
        public void EnqueueWork(TransferJob job)
        {
            lock(_workQueueLock)
            {
                workQueue.Enqueue(job);
                _waitHandle.Set();
                DebugLog.LogDebug($"Enqueued job at position {workQueue.Count}.");
            }
        }

        /// <summary>
        /// Thread-safe Dequeue
        /// </summary>
        /// <returns></returns>
        public TransferJob DequeueWork()
        {
            lock (_workQueueLock)
            {
                if (workQueue.Count > 0)
                    return workQueue.Dequeue();
                else
                    return null;
            }
        }


        /// <summary>
        /// to be called from MatchOffers Prefix Patch:
        /// take requested material and submit all offers as TransferJob
        /// </summary>
        public void SubmitMatchOfferJob(TransferManager.TransferReason material)
        {
            if (material == TransferManager.TransferReason.None) return;

            TransferJob job = TransferJobPool.Instance.Lease();
            if (job == null)
            {
                DebugLog.LogError("NO MORE TRANSFER JOBS AVAILABLE, DROPPING TRANSFER REQUESTS!");
                return;
            }

            // set job header info
            job.material = material;
            job.m_incomingCount = 0;
            job.m_outgoingCount = 0;
            job.m_incomingAmount = m_incomingAmount[(int)material];
            job.m_outgoingAmount = m_outgoingAmount[(int)material];

            int offer_offset;

            for (int priority = 7, jobInIdx=0, jobOutIdx=0; priority >= 0; --priority)
            {
                offer_offset = (int)material * 8 + priority;
                job.m_incomingCount += m_incomingCount[offer_offset];
                job.m_outgoingCount += m_outgoingCount[offer_offset];

                // linear copy to job's offer arrays
                //** TODO: evaluate speedup via unsafe pointer memcpy **

                for (int offerIndex = 0; offerIndex < m_incomingCount[offer_offset]; offerIndex++, jobInIdx++)
                    job.m_incomingOffers[jobInIdx] = m_incomingOffers[offer_offset * 256 + offerIndex];

                for (int offerIndex = 0; offerIndex < m_outgoingCount[offer_offset]; offerIndex++, jobOutIdx++)
                    job.m_outgoingOffers[jobOutIdx] = m_outgoingOffers[offer_offset * 256 + offerIndex];

            }

            // DEBUG mode: print job summary
            DebugJobSummarize(job);

            // Enqueue in work queue for match-making thread
            EnqueueWork(job);

            // clear this material transfer:
            ClearAllTransferOffers(material);

        } //SubmitMatchOfferJob


        /// <summary>
        /// to be called from MatchOffers Postfix Patch:
        /// receive match-maker results and start transfers
        /// </summary>
        public void StartTransfers()
        {
            //TODO

        }


        private void ClearAllTransferOffers(TransferManager.TransferReason material)
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


        [Conditional("DEBUG")]
        private void DebugJobSummarize(TransferJob job)
        {
            DebugLog.LogDebug($"TRANSFER JOB: {job.material.ToString()}, amount in/out: {job.m_incomingAmount}/{job.m_outgoingAmount}; total offer count in/out: {job.m_incomingCount}/{job.m_outgoingCount}");
        }

    }

}
