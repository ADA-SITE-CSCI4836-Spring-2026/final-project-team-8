public struct TimeChangedEvent
{
    public float NewMaxTime;
    public float CurrentTime;
    public TimeChangedEvent(float maxTime, float currentTime)
    {
        NewMaxTime   = maxTime;
        CurrentTime  = currentTime;
    }
}
