namespace Utils.DesignPattern.State
{
    public class StateMachine <TOwner>
    {
        public IState<TOwner> Current { get; private set; }
        
        public void Change(TOwner owner, IState<TOwner> next)
        {
            Current = next;
            Current?.Enter(owner);
        }

        public void Execute(TOwner owner, float offset = 0)
        {
            Current?.Execute(owner, offset);
        }
    
        public void CancelNow(TOwner owner)
        {
            Current?.CancelNow(owner);
        }
    }
}