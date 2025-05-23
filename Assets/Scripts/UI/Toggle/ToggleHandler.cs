using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Toggle
{
    public class ToggleHandler: MonoBehaviour
    {
        [SerializeField] private Image onImage;
        [SerializeField] private Image offImage;

        private void Start()
        {
            onImage.enabled = false;
            offImage.enabled = true;
        }

        public void OnToggle(bool value)
        {
            onImage.enabled = value;
            offImage.enabled = !value;
        }
    }
}