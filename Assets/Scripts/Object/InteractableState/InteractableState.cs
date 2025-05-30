using UnityEngine;

namespace Object.InteractableState
{
    public abstract class InteractableState : IInteractableState
    {
        protected InteractableObject interactableObject;
        protected GameObject gameObject => interactableObject.gameObject;
        protected Material material => interactableObject.material;
        protected Outline outline => interactableObject.outline;

        public InteractableState(InteractableObject interactableObject)
        {
            this.interactableObject = interactableObject;

            // Disable previous state and enable current
            if (this.interactableObject.currentState != null) this.interactableObject.currentState.Disable();
            Enable();
        }

        public virtual void Enable() {}

        public virtual void Handle() {}

        public virtual void Disable() {}
    }
}