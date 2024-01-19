using Configgy;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class VoxelHandOverlay : MonoBehaviour
    {
        [Configgable("Voxel/Overlay", "Show Input Prompt")]
        private static ConfigToggle displayInputPrompt = new ConfigToggle(true);

        private Text blockNameText;
        private Text inputPromptText;
        private GameObject container;

        bool initialized;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (initialized)
                return;

            container = transform.LocateComponent<RectTransform>("Container").gameObject;
            container.SetActive(false);

            inputPromptText = transform.LocateComponent<Text>("Text_InputPrompt");
            blockNameText = transform.LocateComponent<Text>("Text_BlockName");

            UFGInput.SecretButton.OnValueChanged += UpdateInputPrompt;
            displayInputPrompt.OnValueChanged += SetInputPrompt;

            SetInputPrompt(displayInputPrompt.Value);
            UpdateInputPrompt(UFGInput.SecretButton.Value);

            initialized = true;
        }

        private void SetInputPrompt(bool value)
        {
            if (inputPromptText == null)
                return;

            inputPromptText.gameObject.SetActive(value);
        }

       
        public void SetOpen(bool value)
        {
            Initialize();
            container.SetActive(value);

        }

        public void UpdateBlockText(VoxelData selected)
        {
            Initialize();
            StopAllCoroutines();
            blockNameText.text = selected?.DisplayName ?? "";
            StartCoroutine(FadeOutText());
        }

        private IEnumerator FadeOutText()
        {
            blockNameText.color = Color.white;
            yield return new WaitForSeconds(1f);
            float delay = 2f;
            float timer = delay;
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                float t = 1-(timer/delay);
                blockNameText.color = Color.Lerp(Color.white, Color.clear, t);
                yield return new WaitForEndOfFrame();
            }
            blockNameText.color = Color.clear;
        }

        private void UpdateInputPrompt(KeyCode value)
        {
            if (inputPromptText == null)
                return;

            inputPromptText.text = $"[<color=orange>{UFGInput.SecretButton.Value}</color>] Block Palette";
        }

        private void OnDestroy()
        {
            UFGInput.SecretButton.OnValueChanged -= UpdateInputPrompt;
            displayInputPrompt.OnValueChanged -= SetInputPrompt;

        }

    }
}
