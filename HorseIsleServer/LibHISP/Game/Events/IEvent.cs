namespace HISP.Game.Events
{
    internal interface IEvent
    {
        public abstract void StartEvent();
        public abstract void StopEvent();
    }
}
