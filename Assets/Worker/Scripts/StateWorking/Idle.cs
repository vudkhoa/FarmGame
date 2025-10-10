using System;
using Utils.DesignPattern.State;
using Worker.Model;

public class Idle : IState<WorkerModel>
{
    public void Enter(WorkerModel model) 
    {
        model.State = Worker.Controller.WorkerState.Idle;
        model.StartTime = DateTime.MinValue;
        model.IdProduct = -1;
        model.IdPlot = -1;
        model.NotExecute = false;
    }

    public void Execute(WorkerModel model) { }
    public void Exit(WorkerModel model) { }

}
