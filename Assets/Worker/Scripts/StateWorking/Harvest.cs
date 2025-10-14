using Plots.Controller;
using Sell.Controller;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utils.DesignPattern.State;
using Worker.Model;

public class Harvest : IState<WorkerModel>
{
    private int _exited = 0;

    public void Enter(WorkerModel model) 
    {
        model.State = Worker.Controller.WorkerState.Harvest;
        model.StartTime = DateTime.Now;
        
        model.WorkCts?.Cancel();
        model.WorkCts?.Dispose();
        model.WorkCts = new CancellationTokenSource();
        _exited = 0;
    }

    public void Execute(WorkerModel model, float offset) 
    {
        
        //UnityEngine.Debug.Log(model.Id + " Harvest " + model.IdPlot);
        model.StartTime = DateTime.Now;
        model.State = Worker.Controller.WorkerState.Harvest;
        float time = model.TimeTask;
        if (offset > 0)
        {
            model.StartTime = DateTime.Now.AddSeconds(-offset);
            float totalTime = (float)(DateTime.Now - model.StartTime).TotalSeconds;
            if (totalTime >= time)
            {
                time = 0;
            }
            else
            {
                time = model.TimeTask - totalTime;
            }
            time = time + 1.5f;
        }
        UnityEngine.Debug.Log(model.Id + " Harvest: " + DateTime.Now + " " + time + " " + offset + " " + model.StartTime + " Plot: " + model.IdPlot);
        
        model.CurrentTask = DelayThenWorkAsync(time, model, model.WorkCts.Token);
    }

    public async Task DelayThenWorkAsync(float seconds, WorkerModel model, CancellationToken token)
    {
        try
        {
            //UnityEngine.Debug.Log(model.Id + " Harvest " + model.IdPlot + " Harvest Await " + seconds);
            await Task.Delay(TimeSpan.FromSeconds(seconds), token);

            token.ThrowIfCancellationRequested();

            Working(model);
        }
        catch (OperationCanceledException)
        {
            //UnityEngine.Debug.Log($"{model.Id} Harvest CANCELED");

            SafeExit(model);
        }
        catch (Exception ex)
        {
            //UnityEngine.Debug.LogError($"{model.Id} Produce ERROR: {ex}");
            //SafeExit(model);
        }
    }

    private void Working(WorkerModel model)
    {
        if (model.NotExecute)
        {
            //UnityEngine.Debug.Log(model.Id + " Harvest Not Working");
            SafeExit(model);
            return;
        }
        //UnityEngine.Debug.Log(PlotController.Instance.PlotModelList[model.IdPlot].Data.ProductType);
        if (PlotController.Instance.PlotModelList[model.IdPlot].Data.CurAmount < PlotController.Instance.PlotModelList[model.IdPlot].Lifetime)
        {
            UnityEngine.Debug.Log("Canceled");
            SafeExit(model);
            return;
        }

        SellController.Instance.CaculateAmountByIndex
        (
            SellController.Instance.FindIndexByProductType
            (
                PlotController.Instance.PlotModelList[model.IdPlot].Data.ProductType
            ),
            PlotController.Instance.PlotModelList[model.IdPlot].Data.CurAmount
        );
        PlotController.Instance.SetNullById(model.IdPlot);
        UnityEngine.Debug.Log(model.Id + " Harvest Working " + DateTime.Now);
        SafeExit(model);
    }

    public void Exit(WorkerModel model)
    {
        UnityEngine.Debug.Log(model.Id + " Succesful Harvest");
        model.StateMachine.Change(model, new Idle());
    }

    private void SafeExit(WorkerModel model)
    {
        if (System.Threading.Interlocked.Exchange(ref _exited, 1) == 1) return;

        model.WorkCts?.Cancel();
        model.WorkCts?.Dispose();
        model.WorkCts = null;

        Exit(model);
    }

    public void CancelNow(WorkerModel model)
    {
        model.WorkCts?.Cancel();

        SafeExit(model);
    }
}