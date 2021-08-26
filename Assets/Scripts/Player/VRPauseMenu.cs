using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class VRPauseMenu : MonoBehaviour
{
    public XRNode inputSource;
    public GameObject menuCanvas;
    public GameObject lights;
    private bool toggleCooldown = false;

    // Update is called once per frame
    void Update()
    {
        InputDevice device2 = InputDevices.GetDeviceAtXRNode(inputSource);
        bool togglePauseMenu = false;

        device2.TryGetFeatureValue(CommonUsages.secondaryButton, out togglePauseMenu);
        if(togglePauseMenu && !toggleCooldown) {
            menuCanvas.SetActive(!menuCanvas.activeSelf);
            Invoke("ResetToggle", 0.25f);
            toggleCooldown = true;
        }
    }

    void ResetToggle() {
        toggleCooldown = false;
    }

    public void Unpause() {
        menuCanvas.SetActive(false);
    }

    public void Lights() {
        lights.SetActive(!lights.activeSelf);
    }

    public void ReturnToMainMenu()
    {
        StartCoroutine(LoadMainMenuAsync());
    }

    IEnumerator LoadMainMenuAsync()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            GameObject.Destroy(GameObject.Find("StructureComplexity"));
            yield return null;
        }
    }
}
