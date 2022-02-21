using ColossalFramework;
using ColossalFramework.Plugins;
using MoreEffectiveTransfer.CustomAI;
using MoreEffectiveTransfer.Util;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;

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
                    _instance.Initialize();
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

            DebugLog.DebugMsg($"TransferJobPool initialized, pool count is {pooledJobs.Count}");
        }

        public void Delete()
        {
            DebugLog.DebugMsg($"Deleting instance: {_instance}");
            // unallocate object pool of work packages
            pooledJobs.Clear();
            pooledJobs = null;
            TransferJobPool._instance = null;
        }

        public TransferJob Lease()
        {
            lock (_poolLock)
            {
                _usageCount++;
                _maxUsageCount = (_usageCount > _maxUsageCount) ? _usageCount : _maxUsageCount;
                return pooledJobs.Pop();
            }
        }

        public void Return(TransferJob job)
        {
            lock (_poolLock)
            {
                _usageCount--;
                pooledJobs.Push(job);
            }
        }

    }


    /// <summary>
    /// CustomTransferDisptacher: coordinate with match maker thread
    /// </summary>
    public sealed class CustomTransferDispatcher
    {
        private static CustomTransferDispatcher _instance = null;
        public Queue<TransferJob> workQueue = null;
        public static readonly object _workQueueLock = new object();


        public static CustomTransferDispatcher Instance
        {
            get {
                    if (_instance == null)
                    {
                        _instance = new CustomTransferDispatcher();
                        _instance.Initialize();
                    }
                    return _instance;
            }
        }

        public void Initialize()
        {
            // allocate object pool of work packages
            workQueue = new Queue<TransferJob>(TransferManager.TRANSFER_REASON_COUNT);

            DebugLog.DebugMsg($"CustomTransferDispatcher initialized, workqueue count is {workQueue.Count}");
        }

        public void Delete()
        {
            DebugLog.DebugMsg($"Deleting instance: {_instance}");
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
                return workQueue.Dequeue();
            }
        }





    }

}
