using Bag.Controller;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utils.DesignPattern.State;
using Worker.Model;

public class Produce : IState<WorkerModel>
{
    public void Enter(WorkerModel model)
    {
        model.State = Worker.Controller.WorkerState.Produce;
    }

    public void Execute(WorkerModel model) 
    {
        UnityEngine.Debug.Log("Produce");
        model.StartTime = DateTime.Now;
        _ = DelayThenWork(model.TimeTask, model);
    }

    public async Task DelayThenWork(float seconds, WorkerModel model)
    {
        UnityEngine.Debug.Log("Await");
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        Working(model);
    }

    private void Working(WorkerModel model)
    {
        if (model.NotExecute)
        {
            UnityEngine.Debug.Log("Working");
            this.Exit(model);
        }
        UnityEngine.Debug.Log(model.Id + " Not Working");
        BagController.Instance.MoveToPlot(model.IdProduct - 1, model.IdPlot);
        this.Exit(model);
    }

    public void Exit(WorkerModel model) 
    {
        UnityEngine.Debug.Log("Succesful Produce");
        model.StateMachine.Change(model, new Idle());
    }

}
