namespace Utils.DesignPattern.State
{
    public interface IState <TOwner> 
    {
        void Enter(TOwner owner);
        void Execute(TOwner owner, float offset);
        void Exit(TOwner owner);
        void CancelNow(TOwner owner);
    }
}