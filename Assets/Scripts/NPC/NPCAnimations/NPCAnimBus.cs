using System;
using UnityEngine;

namespace NPC.NPCAnimations
{
    public static class NPCAnimBus
    {
        /*  Bool  : animations en boucle     (Walk, IsTalking…)
            Trigger : animations ponctuelles (Dance, Clap, Wave…)            */
        public static event Action<GameObject, NPCAnimationsType, bool> OnBool;
        public static event Action<GameObject, NPCAnimationsType>       OnTrigger;

        public static void Bool   (GameObject who, NPCAnimationsType type, bool value = true)
            => OnBool?.Invoke(who, type, value);

        public static void Trigger(GameObject who, NPCAnimationsType type)
            => OnTrigger?.Invoke(who, type);
    }
}