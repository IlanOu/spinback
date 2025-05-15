public class TextInteractableState : InteractableState
{
    public TextInteractableState(InteractableObject interactableObject) : base(interactableObject) {}

    public override void Enable() 
    {
        material.SetFloat("_OutlineSize", interactableObject.outlineSize);
        interactableObject.label3D.SetActive(true);
    }

    public override void Handle()
    {
        if (!interactableObject.IsZooming())
        {
            interactableObject.UpdateState(new NoneInteractableState(interactableObject));
        }
    }

    public override void Disable() 
    {
        material.SetFloat("_OutlineSize", 0f);
        interactableObject.label3D.SetActive(false);
    }
}