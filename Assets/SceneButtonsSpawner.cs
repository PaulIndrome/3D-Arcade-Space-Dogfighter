using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Soulspace
{
    [RequireComponent(typeof(UIDocument))]
    public class SceneButtonsSpawner : MonoBehaviour
    {
        [SerializeField] UIDocument mainMenuDocument;
        [SerializeField] private VisualTreeAsset sceneButtonTemplate;

        private void Awake() {
            mainMenuDocument = GetComponent<UIDocument>();
        }

        private void Start() {
            VisualElement buttonParent = mainMenuDocument.rootVisualElement.Q("ButtonList");

            for(int i = 0; i < SceneManager.sceneCountInBuildSettings; i++){
                SceneButton sceneButton = new SceneButton(i, sceneButtonTemplate);

                buttonParent.Add(sceneButton.button);
            }
        }

        class SceneButton {
            public Button button;
            private int sceneIndex = -1;

            public SceneButton(int sceneBuildIndex, VisualTreeAsset template){
                sceneIndex = sceneBuildIndex;

                string scenePath = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
                string[] scenePathSplits = scenePath.Split('/');
                scenePathSplits = scenePathSplits[scenePathSplits.Length - 1].Split('.');
                scenePath = scenePathSplits[0];
                TemplateContainer buttonTemplate = template.Instantiate();

                button = buttonTemplate.Query<Button>();
                button.RegisterCallback<NavigationSubmitEvent>(OnSceneButtonInteracted);
                button.RegisterCallback<ClickEvent>(OnSceneButtonInteracted);

                button.text = string.Format("{0}_{1}", sceneIndex, scenePath);
            }

            private void OnSceneButtonInteracted(EventBase eventBase){
                if(sceneIndex < 0) return;
                SceneManager.LoadScene(sceneIndex);
            }
        }
    }
}
