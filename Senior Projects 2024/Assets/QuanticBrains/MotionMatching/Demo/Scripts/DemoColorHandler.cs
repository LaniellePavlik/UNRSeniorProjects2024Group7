using System.Collections.Generic;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Demo.Scripts
{
    public class DemoColorHandler : MonoBehaviour
    {
        [SerializeField] private List<MeshRenderer> referenceMeshRenderers;
        private List<Material> _baseMaterials;

        void Start()
        {
            _baseMaterials = new List<Material>();
            foreach (var meshRenderer in referenceMeshRenderers)
            {
                _baseMaterials.Add(meshRenderer.material);
            }
        
            _baseMaterials.ForEach( x=> x.color = Color.white);
        }

        private float timeLeft = 2.5f;
        private Color targetColor = Color.white;

        void Update()
        {
            if (timeLeft <= Time.deltaTime)
            {
                _baseMaterials.ForEach( x=> x.color = targetColor);
            
                targetColor = new Color(Random.value, Random.value, Random.value);
                timeLeft = 2.5f;
                return;
            }
        
            //Compute interpolated color
            _baseMaterials.ForEach( x=> x.color = Color.Lerp(x.color, targetColor, Time.deltaTime / timeLeft));
            timeLeft -= Time.deltaTime;
        }
    }
}
