using UnityEngine;
namespace Psyfer 
{
    public class SwitchScene : MonoBehaviour
    {
        [SerializeField]
        private string _nextScene;

        public void NextScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(_nextScene);
        } 
    }
}
