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
            onImage.enabled = true;
            offImage.enabled = false;
        }

        public void OnToggle(bool value)
        {
            onImage.enabled = value;
            offImage.enabled = !value;
        }
    }
}