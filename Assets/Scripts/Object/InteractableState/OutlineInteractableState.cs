namespace Object.InteractableState
{
    public class OutlineInteractableState : global::Object.InteractableState.InteractableState
    {
        public OutlineInteractableState(InteractableObject interactableObject) : base(interactableObject) {}

        public override void Enable() 
        {
            if (outline != null)
            {
                outline.enabled = true;
                outline.OutlineWidth = interactableObject.outlineSize;
            }
            else
            {
                material.SetFloat("_OutlineSize", interactableObject.outlineSize);
            }
        }

        public override void Handle()
        {
            if (interactableObject.IsFocused)
            {
                interactableObject.UpdateState(new TextInteractableState(interactableObject));
            }
        }

        public override void Disable() 
        {
            if (outline != null)
            {
                outline.enabled = false;
            }
            else
            {
                material.SetFloat("_OutlineSize", 0f);
            }
        }
    }
}