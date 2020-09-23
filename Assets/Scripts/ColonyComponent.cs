using UnityEngine;

namespace Assets.Scripts
{
    public class ColonyComponent : MonoBehaviour
    { 

        // Start is called before the first frame update
        void Start()
        { 
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Arrived(AntBehaviour antBehaviour, float food)
        { 
            Debug.Log($"Ant arrived with {food}"); 
        }
    }
}
