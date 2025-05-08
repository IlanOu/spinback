public interface INPCMovementStrategy
{
    void StartMovement();
    bool IsDone { get; }
}