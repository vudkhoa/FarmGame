namespace Utils.DesignPattern.State
{
    public interface IState <TOwner> 
    {
        void Enter(TOwner owner);
        void Execute(TOwner owner);
        void Exit(TOwner owner);
    }
}