
public interface ITimeRewindable
{
    public void InitializeStateRecording(float interval, bool adaptiveRecording);
    public void TruncateHistoryAfter(float timePoint);
    public void RecordState(float time);
    public void RewindToTime(float targetTime);
    public void ClearStates();
}