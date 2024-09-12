using UnityEngine;

namespace Cohort.Config {
    [CreateAssetMenu(menuName = "Cohort/Config/Game", fileName = "GameConfig", order = 0)]
    public class GameConfig : ScriptableObject {
        public static GameConfig Config {
            get {
                if (_instance == null) {
                    _instance = Resources.Load<GameConfig>("Config/GameConfig");
                }

                return _instance;
            }
        }
        private static GameConfig _instance;
        
        [SerializeField] private string litShader = "Universal Render Pipeline/Lit";
        [SerializeField] private string unlitShader = "Universal Render Pipeline/Unlit";
        [SerializeField] private Material _glbOpaque;
        [SerializeField] private Material _glbTransparent;
        
        public Material GetBaseMaterial(bool unlit, bool transparant) {
            Material mat;
            if (transparant) {
                mat = new Material(_glbOpaque);
            }
            else {
                mat = new Material(_glbTransparent);
            }

            mat.shader = Shader.Find((unlit) ? unlitShader : litShader);
            return mat;
        }
    }
}