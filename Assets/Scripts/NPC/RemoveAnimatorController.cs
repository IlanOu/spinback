using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RemoveAnimatorController : MonoBehaviour
{
    void Start()
    {
        Animator[] animators = transform.GetComponentsInChildren<Animator>();
        foreach (Animator animator in animators)
        {
            animator.runtimeAnimatorController = null;
        }
    }
}
