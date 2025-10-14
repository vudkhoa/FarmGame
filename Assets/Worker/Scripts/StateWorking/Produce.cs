using Bag.Controller;
using Plots.Controller;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utils.DesignPattern.State;
using Worker.Model;

public class Produce : IState<WorkerModel>
{
    private int _exited = 0;

    public void Enter(WorkerModel model)
    {
        model.State = Worker.Controller.WorkerState.Produce;
        model.StartTime = DateTime.Now;
          
        model.WorkCts?.Cancel();
        model.WorkCts?.Dispose();
        model.WorkCts = new CancellationTokenSource();
        _exited = 0;
    }

    public void Execute(WorkerModel model, float offset) 
    {
        //UnityEngine.Debug.Log(model.Id + " Produce: " + DateTime.Now + " " + offset);
        model.StartTime = DateTime.Now;
        model.State = Worker.Controller.WorkerState.Produce;
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
        UnityEngine.Debug.Log(model.Id + " Produce: " + DateTime.Now + " " + time + " " + offset + " " + model.StartTime);
        
        model.CurrentTask = DelayThenWorkAsync(time, model, model.WorkCts.Token);
    }

    public async Task DelayThenWorkAsync(float seconds, WorkerModel model, CancellationToken token)
    {
        try
        {
            //UnityEngine.Debug.Log(model.Id + " Produce Await");
            await Task.Delay(TimeSpan.FromSeconds(seconds), token);

            token.ThrowIfCancellationRequested();

            Working(model);
        }
        catch (OperationCanceledException)
        {
            //UnityEngine.Debug.Log($"{model.Id} Produce CANCELED");

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
            //UnityEngine.Debug.Log(model.Id + model.IdPlot + " " + model.IdProduct + " Produce Not Working");
            SafeExit(model);
            return;
        }
        UnityEngine.Debug.Log(model.Id + " Produce: " + DateTime.Now);
        //UnityEngine.Debug.Log(model.Id + " Produce Working " + (model.IdProduct - 1) + " " + model.IdPlot);   
        BagController.Instance.MoveToPlot(model.IdProduct - 1, model.IdPlot);
        SafeExit(model);
    }

    public void Exit(WorkerModel model) 
    {
        UnityEngine.Debug.Log(model.Id + model.IdPlot + " " + (model.IdProduct - 1) + " Succesful Produce");
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
