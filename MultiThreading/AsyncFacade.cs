using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MuonhoryoLibrary.Unity.Threading
{
    public abstract class AsyncFacade
    {
        protected AsyncFacade()
        {
            InitializeEvent += InitializeAction;
            StartEvent += StartAction;
            EndEvent += EndAction;
        }
        protected readonly AutoResetEvent Handler = new AutoResetEvent(false);
        protected Thread CurrentThread;
        private static object lockObj = new object();
        protected abstract ThreadManager ThreadManager { get; }

        protected void WaitAndReset()
        {
            Handler.WaitOne();
            Handler.Reset();
        }
        /// <summary>
        /// Check on nullable
        /// </summary>
        /// <param name="ev"></param>
        protected void DelegateEventExecutingToTM(Action ev)
        {
            if (ev != null)
                ThreadManager.AddActionsQueue(ev, Handler);
            else
                Handler.Set();
        }
        protected void RunAsyncAndWait(Action runningAsyncAction)
        {
            runningAsyncAction();
            WaitAndReset();
        }

        public event Action InitializeEvent;
        public event Action StartEvent;
        public event Action EndEvent;
        protected virtual void InitializeAction() { }
        protected virtual void StartAction() { }
        protected virtual void EndAction() { }

        public void InitializeAndStart()
        {
            InitializeEvent();
            CurrentThread = new Thread(new ThreadStart(AsyncExecute));
            Initialize();
            CurrentThread.Start();
        }
        protected abstract void Initialize();
        private void AsyncExecute()
        {
            lock (lockObj)
            {
                DelegateEventExecutingToTM(StartEvent);
                WaitAndReset();
                AsyncAction();
                ThreadManager.AddActionsQueue(EndEvent);
            }
        }
        protected abstract void AsyncAction();
    }
}
