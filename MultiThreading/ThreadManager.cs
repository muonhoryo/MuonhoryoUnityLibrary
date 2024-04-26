using MuonhoryoLibrary.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MuonhoryoLibrary.Unity.Threading
{
    /// <summary>
    /// Execute added actions as queue in main thread in Update().
    /// If current frame time more then MaxFrameTime,stops until next Update().
    /// </summary>
    public class ThreadManager : MonoBehaviour, ISingltone<ThreadManager>
    {
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
        /// <summary>
        /// Maximum time for executing actions queue in one frame. Default value=5f.
        /// </summary>
        protected virtual float MaxFrameTime { get => 5; }
        private static readonly object locker = new object();
        /// <summary>
        /// Reserves Id for deferred queue adding. 
        /// </summary>
        private static short NextId = short.MinValue;
        private sealed class ThreadActionQueue : IEnumerable<Action>
        {
            private sealed class ThreadActionQueueEnumerator : IEnumerator<Action>
            {
                private ThreadActionQueueEnumerator() { }
                public ThreadActionQueueEnumerator(SingleLinkedList<Action> ActionsQueue)
                {
                    if (ActionsQueue == null)
                    {
                        throw new ArgumentException("ActionsQueue");
                    }
                    this.ActionsQueue = ActionsQueue;
                }
                private Action GetCurrentAction()
                {
                    if (ActionsQueue.Count_ <= 0)
                    {
                        throw new IndexOutOfRangeException("ActionsQueue.Count");
                    }
                    Action action = ActionsQueue[0];
                    ActionsQueue.RemoveFirst();
                    return action;
                }
                object IEnumerator.Current => GetCurrentAction();
                Action IEnumerator<Action>.Current => GetCurrentAction();
                bool IEnumerator.MoveNext()
                {
                    if (ActionsQueue.Count_ > 0)
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
                private readonly SingleLinkedList<Action> ActionsQueue;
            }
            private ThreadActionQueue() { }
            public ThreadActionQueue(SingleLinkedList<Action> actionsList, Action OnQueueDone) :
                this(actionsList, OnQueueDone, NextId++)
            { }
            public ThreadActionQueue(SingleLinkedList<Action> actionsList, Action OnQueueDone, short id)
            {
                QueueEnumerator = new ThreadActionQueueEnumerator(actionsList);
                this.OnQueueDone = OnQueueDone;
                QueueId = id;
            }
            public ThreadActionQueue(IEnumerable<Action> collection, Action OnQueueDone, short id)
            {
                SingleLinkedList<Action> list = new SingleLinkedList<Action> { };
                foreach (Action action in collection)
                {
                    list.AddLast(action);
                }
                QueueEnumerator = new ThreadActionQueueEnumerator(list);
                this.OnQueueDone = OnQueueDone;
                QueueId = id;
            }
            public ThreadActionQueue(IEnumerable<Action> collection, Action OnQueueDone) :
                this(collection, OnQueueDone, NextId++)
            { }
            private readonly ThreadActionQueueEnumerator QueueEnumerator;
            IEnumerator<Action> IEnumerable<Action>.GetEnumerator() => QueueEnumerator;
            IEnumerator IEnumerable.GetEnumerator() => QueueEnumerator;
            public readonly short QueueId;
            public readonly Action OnQueueDone;
        }
        private readonly SingleLinkedList<ThreadActionQueue> ThreadActionsQueues =
            new SingleLinkedList<ThreadActionQueue> { };
        //Singltone
        private static ThreadManager singltone;
        ThreadManager ISingltone<ThreadManager>.Singltone { get => singltone; set => singltone = value; }
        private short AddQueue(ThreadActionQueue queue)
        {
            ThreadActionsQueues.AddLast(queue);
            return queue.QueueId;
        }
        private void Awake()
        {
            SingltoneInitializations.InitializationForFirstExample(this, () => { });
            if (!enabled)
                enabled = true;
        }
        private void Update()
        {
            lock (locker)
            {
                while (ThreadActionsQueues.Count_ > 0)
                {
                    foreach (Action action in ThreadActionsQueues[0])
                    {
                        action();
                        if (Time.timeSinceLevelLoad > MaxFrameTime)
                        {
                            return;
                        }
                    }
                    ThreadActionsQueues[0].OnQueueDone.Invoke();
                    ThreadActionsQueues.RemoveFirst();
                }
            }
        }
        private void OnDisable()
        {
            enabled = true;
        }
        void ISingltone<ThreadManager>.Destroy()
        {
            Destroy(this);
        }
        private void AddQueueWithReservator(ThreadActionQueue queue, ThreadQueueReservator reservator)
        {
            if (!reservator.isBeingUsed)
            {
                lock (locker)
                {
                    ThreadActionsQueues.AddLast(queue);
                    reservator.SetUse();
                }
            }
            else
            {
                throw new InvalidIdReservatorException("reservator");
            }
        }


        /// <summary>
        /// Add actions in queue with reserved id.
        /// </summary>
        /// <param name="actionsList"></param>
        /// <param name="onQueueDoneAction"></param>
        /// <param name="reservator"></param>
        public void AddActionsQueue(IEnumerable<Action> actions, Action onQueueDoneAction,
            ThreadQueueReservator reservator)
        {
            AddQueueWithReservator(new ThreadActionQueue(actions, onQueueDoneAction), reservator);
        }
        /// <summary>
        /// Add actions in queue with reserved id.
        /// </summary>
        /// <param name="actionsList"></param>
        /// <param name="onQueueDoneAction"></param>
        /// <param name="reservator"></param>
        public void AddActionsQueue(SingleLinkedList<Action> actions, Action onQueueDoneAction,
            ThreadQueueReservator reservator)
        {
            AddQueueWithReservator(new ThreadActionQueue(actions, onQueueDoneAction), reservator);
        }

        /// <summary>
        /// Add actions in queue. Executed actions are removed from queue.
        /// </summary>
        /// <param name="actionsList"></param>
        /// <param name="onQueueDoneAction"></param>
        /// <returns></returns>
        public short AddActionsQueue(IEnumerable<Action> actions, Action onQueueDoneAction)
        {
            lock (locker)
            {
                ThreadActionQueue queue = new ThreadActionQueue(actions, onQueueDoneAction);
                return AddQueue(queue);
            }
        }
        /// <summary>
        /// Add actions in queue. Executed actions are removed from queue.
        /// </summary>
        /// <param name="actionsList"></param>
        /// <param name="onQueueDoneAction"></param>
        /// <returns></returns>
        public short AddActionsQueue(SingleLinkedList<Action> actions, Action onQueueDoneAction)
        {
            lock (locker)
            {
                ThreadActionQueue queue = new ThreadActionQueue(actions, onQueueDoneAction);
                return AddQueue(queue);
            }
        }
        /// <summary>
        /// Add actions in queue. Executed actions are removed from queue.
        /// </summary>
        /// <param name="actionsList"></param>
        /// <param name="onQueueDoneAction"></param>
        /// <returns></returns>
        public short AddActionsQueue(Action mainThreadAction, Action onEndAction)
        {
            return AddActionsQueue(new Action[1] { mainThreadAction }, onEndAction);
        }
        /// <summary>
        /// Add actions in queue. Executed actions are removed from queue.
        /// </summary>
        /// <param name="actionsList"></param>
        /// <param name="onQueueDoneAction"></param>
        /// <returns></returns>
        public short AddActionsQueue(Action mainThreadAction, AutoResetEvent handler)
        {
            return AddActionsQueue(mainThreadAction, () => handler.Set());
        }
        /// <summary>
        /// Add actions in queue. Executed actions are removed from queue.
        /// </summary>
        /// <param name="actionsList"></param>
        /// <param name="onQueueDoneAction"></param>
        /// <returns></returns>
        public short AddActionsQueue(Action mainThreadAction)
        {
            return AddActionsQueue(mainThreadAction, () => { });
        }
        /// <summary>
        /// Add actions in queue. Executed actions are removed from queue.
        /// </summary>
        /// <param name="actionsList"></param>
        /// <param name="onQueueDoneAction"></param>
        /// <returns></returns>
        public short AddActionsQueue(IEnumerable<Action> mainThreadActions, AutoResetEvent handler)
        {
            return AddActionsQueue(mainThreadActions, () => handler.Set());
        }
        /// <summary>
        /// Cancel executing actions queue at queueId.
        /// </summary>
        /// <param name="queueId"></param>
        public void RemoveActionsQueue(short queueId)
        {
            lock (locker)
            {
                ThreadActionsQueues.RemoveAtPredicate((ThreadActionQueue queue) => queue.QueueId == queueId);
            }
        }
    }
}
