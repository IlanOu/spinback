using UnityEngine;

public class OutlineInteractableState : InteractableState
{
    public OutlineInteractableState(InteractableObject interactableObject) : base(interactableObject) {}

    public override void Enable() 
    {
        material.SetFloat("_OutlineSize", interactableObject.outlineSize);
    }

    public override void Handle()
    {
        if (interactableObject.isLookingAt && interactableObject.IsZooming())
        {
            interactableObject.UpdateState(new TextInteractableState(interactableObject));
        }
    }

    public override void Disable() 
    {
        material.SetFloat("_OutlineSize", 0f);
    }
}