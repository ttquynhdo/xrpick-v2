//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

//public class MenuSelectionController : MonoBehaviour
//{
//    [SerializeField] private Toggle welcomeToggle;
//    [SerializeField] private Toggle conceptToggle;
//    [SerializeField] private Toggle basicPickingToggle;
//    [SerializeField] private Toggle mixedPickingToggle;
//    [SerializeField] private Toggle complexPickingToggle;

//    private void Start()
//    {
//        HighlightCurrentScene();
//    }

//    private void HighlightCurrentScene()
//    {
//        string currentScene = SceneManager.GetActiveScene().name;

//        // Tắt tất cả nhưng không gọi OnValueChanged
//        welcomeToggle.SetIsOnWithoutNotify(false);
//        conceptToggle.SetIsOnWithoutNotify(false);
//        basicPickingToggle.SetIsOnWithoutNotify(false);
//        mixedPickingToggle.SetIsOnWithoutNotify(false);
//        complexPickingToggle.SetIsOnWithoutNotify(false);

//        switch (currentScene)
//        {
//            case "WelcomeScene":
//                welcomeToggle.SetIsOnWithoutNotify(true);
//                break;

//            case "ConceptScene":
//                conceptToggle.SetIsOnWithoutNotify(true);
//                break;

//            case "BasicPickingScene":
//                basicPickingToggle.SetIsOnWithoutNotify(true);
//                break;

//            case "MixedPickingScene":
//                mixedPickingToggle.SetIsOnWithoutNotify(true);
//                break;

//            case "ComplexPickingScene":
//                complexPickingToggle.SetIsOnWithoutNotify(true);
//                break;
//        }
//    }
//}