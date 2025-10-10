using Plots.Controller;
using Sell.Controller;
using System;
using System.Threading.Tasks;
using Utils.DesignPattern.State;
using Worker.Model;

public class Harvest : IState<WorkerModel>
{
    public void Enter(WorkerModel model) 
    {
        model.State = Worker.Controller.WorkerState.Harvest;
    }

    public void Execute(WorkerModel model) 
    {
        model.StartTime = DateTime.Now;
        _ = DelayThenWork(model.TimeTask, model);
    }

    public async Task DelayThenWork(float seconds, WorkerModel model)
    {
        //UnityEngine.Debug.Log("Await");
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        Working(model);
    }

    private void Working(WorkerModel model)
    {
        if (model.NotExecute)
        {
            this.Exit(model);
        }
        SellController.Instance.IncreaseByIndex
        (
            SellController.Instance.FindByProductType
            (
                PlotController.Instance.PlotModelList[model.IdPlot].Data.Type
            ),
            PlotController.Instance.PlotModelList[model.IdPlot].Data.CurAmount
        );
        PlotController.Instance.SetNullById(model.IdPlot);
        this.Exit(model);
    }

    public void Exit(WorkerModel model)
    {
        model.StateMachine.Change(model, new Idle());
    }
}