using UnityEngine;

namespace Assets.Scripts
{
    public class EdgeComponent : MonoBehaviour
    {
        public Node Data;
        public bool HasFood => Data.HasFood;
        public float Amount => Data.FoodAmount;
        public float FoodLeft;

        public AntBehaviour AntPrefab;
        // Start is called before the first frame update
        void Start()
        {
            var ant = Instantiate(AntPrefab, transform.position, Quaternion.identity);
        }

        // Update is called once per frame
        void Update()
        {
            transform.localScale = Vector3.one * Mathf.Clamp(Amount, 0.3f, 2);
            FoodLeft = Data.FoodAmount;
        }
        public EdgeComponent OfData(Node node)
        {
            Data = node;
            transform.localScale = Vector3.one * Mathf.Clamp(Amount, 0.3f, 2);
            return this;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Ant"))
            {
                var ant = other.GetComponentInParent<AntBehaviour>();
                if (ant.State == AntState.Exploring)
                    ant.FoodFound(this);

            }
        }

        public bool TryTake(float capacity, out float taken, out bool last)
        {
            last = false;
            if (Data.FoodAmount - capacity > 0)
                taken = capacity;
            else
            {
                last = true;
                taken = Data.FoodAmount;
            }

            Data.FoodAmount -= taken;
            return taken > 0;
        }
    }
}
