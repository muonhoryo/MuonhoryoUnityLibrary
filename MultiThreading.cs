using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace MuonhoryoLibrary.Unity
{
    /// <summary>
    /// Execute added actions as queue in main thread in Update().
    /// If current frame time more then MaxFrameTime,stops until next Update().
    /// </summary>
    public class ThreadManager : MonoBehaviour, ISingltone<ThreadManager>
    {
        /// <summary>
        /// Maximum time for executing actions queue in one frame. Default value=5f.
        /// </summary>
        protected virtual float MaxFrameTime { get=>5; }
        private static readonly object locker = new object();
        /// <summary>
        /// Reserves Id for deferred queue adding. 
        /// </summary>
        public sealed class ThreadQueueReservator
        {
            public ThreadQueueReservator()
            {
                Id = NextId++;
            }
            public readonly short Id;
            public bool isBeingUsed { get; private set; } = false;
            internal void SetUse()
            {
                isBeingUsed = true;
            }
        }
        private static short NextId = short.MinValue;
        private sealed class ThreadActionQueue : IEnumerable<Action>
        {
            private sealed class ThreadActionQueveEnumerator : IEnumerator<Action>
            {
                private ThreadActionQueveEnumerator() { }
                public ThreadActionQueveEnumerator(List<Action> ActionsQueve)
                {
                    this.ActionsQueve = ActionsQueve;
                }
                private Action GetCurrentAction()
                {
                    if (ActionsQueve.Count <= 0)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    Action action = ActionsQueve[0];
                    ActionsQueve.RemoveAt(0);
                    return action;
                }
                object IEnumerator.Current => GetCurrentAction();
                Action IEnumerator<Action>.Current => GetCurrentAction();
                bool IEnumerator.MoveNext()
                {
                    if (ActionsQueve.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                void IEnumerator.Reset() { }
                void IDisposable.Dispose() { }
                private readonly List<Action> ActionsQueve;
            }
            private ThreadActionQueue() { }
            public ThreadActionQueue(List<Action> actionsList, Action OnQueveDone) : this(actionsList, OnQueveDone, NextId++) { }
            public ThreadActionQueue(List<Action> actionsList, Action OnQueryDone, short id)
            {
                QueveEnumerator = new ThreadActionQueveEnumerator(actionsList);
                OnQueveDone = OnQueryDone;
                QueveId = id;
            }
            private readonly ThreadActionQueveEnumerator QueveEnumerator;
            IEnumerator<Action> IEnumerable<Action>.GetEnumerator() => QueveEnumerator;
            IEnumerator IEnumerable.GetEnumerator() => QueveEnumerator;
            public readonly short QueveId;
            public readonly Action OnQueveDone;
        }
        private readonly List<ThreadActionQueue> ThreadActionsQueues = new List<ThreadActionQueue> { };
        //Singltone
        private static ThreadManager singltone;
        ThreadManager ISingltone<ThreadManager>.Singltone { get => singltone; set => singltone = value; }

        /// <summary>
        /// Add actions in queue. Executed actions are removed from queue.
        /// </summary>
        /// <param name="actionsList"></param>
        /// <param name="onQueueDoneAction"></param>
        /// <returns></returns>
        public short AddActionsQueue(List<Action> actionsList, Action onQueueDoneAction)
        {
            lock (locker)
            {
                ThreadActionQueue queve = new ThreadActionQueue(actionsList, onQueueDoneAction);
                ThreadActionsQueues.Add(queve);
                return queve.QueveId;
            }
        }
        /// <summary>
        /// Add actions in queue with reserved id.
        /// </summary>
        /// <param name="actionsList"></param>
        /// <param name="onQueueDoneAction"></param>
        /// <param name="reservator"></param>
        public void AddActionsQueue(List<Action> actionsList, Action onQueueDoneAction, ThreadQueueReservator reservator)
        {
            if (!reservator.isBeingUsed)
            {
                lock (locker)
                {
                    ThreadActionsQueues.Add(new ThreadActionQueue(actionsList, onQueueDoneAction, reservator.Id));
                    reservator.SetUse();
                }
            }
        }
        /// <summary>
        /// Cancel executing actions queue at queueId.
        /// </summary>
        /// <param name="queueId"></param>
        public void RemoveActionsQueue(short queueId)
        {
            lock (locker)
            {
                for (int i = 0; i < ThreadActionsQueues.Count; i++)
                {
                    if (ThreadActionsQueues[i].QueveId == queueId)
                    {
                        ThreadActionsQueues.RemoveAt(i);
                    }
                }
            }
        }
        private void Awake()
        {
            Singltone.Initialization(this, delegate { Destroy(this); }, delegate { });
        }
        private void Update()
        {
            lock (locker)
            {
                while (ThreadActionsQueues.Count > 0)
                {
                    foreach (Action action in ThreadActionsQueues[0])
                    {
                        action();
                        if (Time.timeSinceLevelLoad > MaxFrameTime)
                        {
                            return;
                        }
                    }
                    ThreadActionsQueues[0].OnQueveDone.Invoke();
                    ThreadActionsQueues.RemoveAt(0);
                }
            }
        }
    }
}
