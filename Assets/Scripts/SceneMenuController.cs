using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMenuController : MonoBehaviour
{
    public void OpenWelcome(bool isOn)
    {
        if (!isOn)
            return;

        OpenFromMenu("WelcomeScene");
    }

    public void OpenConcept(bool isOn)
    {
        if (!isOn)
            return;

        OpenFromMenu("ConceptScene");
    }

    public void OpenBasicPicking(bool isOn)
    {
        if (!isOn)
            return;

        OpenFromMenu("BasicPickingScene");
    }

    public void OpenMixedPicking(bool isOn)
    {
        if (!isOn)
            return;

        OpenFromMenu("MixedPickingScene");
    }

    public void OpenComplex(bool isOn)
    {
        if (!isOn)
            return;

        OpenFromMenu("ComplexScene");
    }

    // #27: resume entry point. Loads only scenes that are in the build; returns false otherwise.
    public bool OpenScene(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
            return false;

        ResearchLogger.SetNextSceneVia(InteractionEvents.ViaResume);
        SceneManager.LoadScene(sceneName);
        return true;
    }

    // #29: menu_select is written before the load, so it lands in the visit the trainee leaves.
    // After the facilitator's Beenden the session has ended and both calls are no-ops.
    private static void OpenFromMenu(string sceneName)
    {
        ResearchLogger.LogMenuSelect(sceneName, false);
        ResearchLogger.SetNextSceneVia(InteractionEvents.ViaMenu);
        SceneManager.LoadScene(sceneName);
    }
}
