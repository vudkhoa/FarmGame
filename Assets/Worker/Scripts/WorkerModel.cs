using System;
using System.Threading;
using Utils.DesignPattern.State;
using Worker.Controller;

namespace Worker.Model
{
    public class WorkerModel
    {
        public int Id;
        public WorkerState State;
        public int IdProduct;
        public int IdPlot;
        public int TimeTask;
        public DateTime StartTime;
        public bool NotExecute;

        // State
        public StateMachine<WorkerModel> StateMachine = new();
        //public CancellationTokenSource CancelSource { get; set; }

        //public void StopWork()
        //{
        //    if (this.CancelSource != null)
        //    {
        //        this.CancelSource.Cancel();
        //        this.CancelSource.Dispose();
        //        this.CancelSource = null;
        //    }
        //}
    }
}