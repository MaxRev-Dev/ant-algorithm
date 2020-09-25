using UnityEngine;

namespace Assets.Scripts
{
    public class EdgeComponent : MonoBehaviour
    {
        public EdgeComponent SetPosition(int vertexId, (int posX, int posY) v)
        {
            VertexId = vertexId;
            PosY = v.posY;
            PosX = v.posX;
            return this;
        }

        public int PosX { get; private set; }
        public int PosY { get; private set; }
        public AntBehaviour AntPrefab;
        // private static bool _instance;
        public int VertexId { get; set; }

        // Start is called before the first frame update
        void Start()
        {
            // if (!_instance)
            Instantiate(AntPrefab, transform.position, Quaternion.identity, transform)
                .Home(this);
            // _instance = true;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Ant"))
            {
            }
        }
    }
}
