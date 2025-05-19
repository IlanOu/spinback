namespace Object.InteractableState
{
    public class TextInteractableState : global::Object.InteractableState.InteractableState
    {
        public TextInteractableState(InteractableObject interactableObject) : base(interactableObject) {}

        public override void Enable() 
        {
            material.SetFloat("_OutlineSize", interactableObject.outlineSize);
            interactableObject.label3D.SetActive(true);
        }

        public override void Handle()
        {
            if (!interactableObject.IsFocused)
            {
                if (!interactableObject.alwaysShowLabel)
                {
                    interactableObject.UpdateState(new NoneInteractableState(interactableObject));
                }
                else
                {
                    interactableObject.UpdateState(new OutlineInteractableState(interactableObject));
                }
            }
        }

        public override void Disable() 
        {
            material.SetFloat("_OutlineSize", 0f);
            interactableObject.label3D.SetActive(false);
        }
    }
}