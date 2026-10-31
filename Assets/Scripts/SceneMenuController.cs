using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMenuController : MonoBehaviour
{
    public void OpenWelcome(bool isOn)
    {
        if (!isOn)
            return;

        SceneManager.LoadScene("WelcomeScene");
    }

    public void OpenConcept(bool isOn)
    {
        if (!isOn)
            return;

        SceneManager.LoadScene("ConceptScene");
    }

    public void OpenBasicPicking(bool isOn)
    {
        if (!isOn)
            return;

        SceneManager.LoadScene("BasicPickingScene");
    }

    public void OpenMixedPicking(bool isOn)
    {
        if (!isOn)
            return;

        SceneManager.LoadScene("MixedPickingScene");
    }

    public void OpenComplex(bool isOn)
    {
        if (!isOn)
            return;

        SceneManager.LoadScene("ComplexScene");
    }

}

//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class SceneMenuController : MonoBehaviour
//{
//    public void OpenWelcome(bool isOn)
//    {
//        if (!isOn)
//            return;

//        LoadSceneIfDifferent("WelcomeScene");
//    }

//    public void OpenConcept(bool isOn)
//    {
//        if (!isOn)
//            return;

//        LoadSceneIfDifferent("ConceptScene");
//    }

//    public void OpenBasicPicking(bool isOn)
//    {
//        if (!isOn)
//            return;

//        LoadSceneIfDifferent("BasicPickingScene");
//    }

//    public void OpenMixedPicking(bool isOn)
//    {
//        if (!isOn)
//            return;

//        LoadSceneIfDifferent("MixedPickingScene");
//    }

//    public void OpenComplex(bool isOn)
//    {
//        if (!isOn)
//            return;

//        LoadSceneIfDifferent("ComplexScene");
//    }

//    private void LoadSceneIfDifferent(string sceneName)
//    {
//        string currentScene = SceneManager.GetActiveScene().name;

//        if (currentScene == sceneName)
//            return;

//        SceneManager.LoadScene(sceneName);
//    }
//}