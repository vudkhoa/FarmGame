using System;
using System.Threading;
using System.Threading.Tasks;
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

        public CancellationTokenSource WorkCts;

        public Task CurrentTask;

        // State
        public StateMachine<WorkerModel> StateMachine = new();
    }
}