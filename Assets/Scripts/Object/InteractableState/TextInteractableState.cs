namespace Object.InteractableState
{
    public class TextInteractableState : global::Object.InteractableState.InteractableState
    {
        public TextInteractableState(InteractableObject interactableObject) : base(interactableObject) {}

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

            if (interactableObject.label3D != null)
            {
                interactableObject.label3D.SetActive(true);
            }
            else
            {
                if (interactableObject.alwaysShowLabel)
                {
                    interactableObject.UpdateState(new OutlineInteractableState(interactableObject));
                }
                else
                {
                    interactableObject.UpdateState(new NoneInteractableState(interactableObject));
                }
            }
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
            if (outline != null)
            {
                outline.enabled = false;
            }
            else
            {
                material.SetFloat("_OutlineSize", 0f);
            }

            if (interactableObject.label3D != null)
            {
                interactableObject.label3D.SetActive(false);
            }
        }
    }
}