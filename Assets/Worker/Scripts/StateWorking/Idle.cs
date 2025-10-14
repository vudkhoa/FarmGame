using Plots.Controller;
using System;
using Utils.DesignPattern.State;
using Worker.Model;

public class Idle : IState<WorkerModel>
{
    public void Enter(WorkerModel model) 
    {
        if (model.IdPlot != -1 && PlotController.Instance.PlotModelList != null)
        {
            PlotController.Instance.PlotModelList[model.IdPlot].Data.WorkerId = -1;
        }
        model.State = Worker.Controller.WorkerState.Idle;
        model.StartTime = DateTime.MinValue;
        model.IdProduct = -1;
        model.IdPlot = -1;
        model.NotExecute = false;

        if (Worker.Controller.WorkerController.Instance.WorkerList != null &&
            Worker.Controller.WorkerController.Instance.View != null)
        {
            Worker.Controller.WorkerController.Instance.CurIdle++;
            Worker.Controller.WorkerController.Instance.View.SetAmount
                (   
                    Worker.Controller.WorkerController.Instance.CurIdle,
                    Worker.Controller.WorkerController.Instance.WorkerList.Count
                );
        }
    }

    public void Execute(WorkerModel model, float offset) { }

    public void Exit(WorkerModel model) { }

    public void CancelNow(WorkerModel model) { }

}
