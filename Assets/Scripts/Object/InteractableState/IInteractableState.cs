namespace Object.InteractableState
{
    public interface IInteractableState
    {
        public void Enable();
        public void Handle();
        public void Disable();
    }
}