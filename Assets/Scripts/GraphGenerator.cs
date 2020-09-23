using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class GraphGenerator : MonoBehaviour
    {
        public int VerticesCount = 100;
        public int PlaneSize = 100;
        public int SafePad = 10;
        public EdgeComponent EdgePrefab;
        public EdgeComponent[] Pool { get; set; }

        public static T[] GetRow<T>(T[,] matrix, in int row)
        {
            var rowLength = matrix.GetLength(1);
            var rowVector = new T[rowLength];

            for (var i = 0; i < rowLength; i++)
                rowVector[i] = matrix[row, i];

            return rowVector;
        }
        private void Awake()
        {
            if (VerticesCount < 1 || VerticesCount > 1000)
            {
                VerticesCount = 50;
                Debug.Log("Out of Range => Vertices count was set to default");
            }

            var relations = new bool[VerticesCount, VerticesCount];
            for (int i = 0; i < VerticesCount; i++)
            {
                for (int j = 0; j < VerticesCount; j++)
                {
                    if (i != j)
                        relations[i, j] = Random.Range(0, 2) == 1;
                }
            }

            // todo: create node paths

            var vertices = Enumerable.Range(0, VerticesCount);
            var nodes = vertices.Select(x => new Node(x,
                Random.Range(0, 2) == 1 ?
                    Random.Range(1, 30) : 0));
            var center = PlaneSize / 2;
            Pool = nodes
                .Select(x => x.SetPosition(
                    RandomExclude(-center + SafePad, center - SafePad, -20, 20)))
                .Select(x => Instantiate(EdgePrefab,
                    new Vector3(x.PosX, 0.2f, x.PosY), Quaternion.identity, transform).OfData(x))
                .ToArray();

        }


        private (int, int) RandomExclude(int min, int max, int innerMin, int innerMax)
        {
            var or = Random.Range(0, 2) == 1;
            return (or ? Random.Range(min, max) :
                Random.Range(0, 2) == 1 ? Random.Range(min, innerMin + 1) : Random.Range(max, innerMax + 1),
                !or ? Random.Range(min, max) :
                Random.Range(0, 2) == 1 ? Random.Range(min, innerMin + 1) : Random.Range(max, innerMax + 1));
        }

        // Start is called before the first frame update
        void Start()
        {
            var home = FindObjectOfType<ColonyComponent>();
            var list = new[] { home.gameObject }.ToList();
            list.AddRange(Pool.Select(x => x.gameObject));
            foreach (var component in list)
            {
                foreach (var other in Pool)
                {
                    if (other.gameObject == component) continue;
                    //DrawLine(component.transform.position, other.gameObject.transform.position, Color.blue);
                }
            }
        }
        void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            var myLine = new GameObject();
            myLine.transform.position = start;
            myLine.AddComponent<LineRenderer>();
            var lr = myLine.GetComponent<LineRenderer>();
            lr.material = (Material)Resources.Load("Materials/GraphLineMat");
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = 0.01f;
            lr.endWidth = 0.01f;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
        // Update is called once per frame
        void Update()
        {

        }

        public Vector3 GetRandomInBounds()
        {
            var center = PlaneSize / 2;
            return new Vector3(Random.Range(-center, center), 0, Random.Range(-center, center));
        }
    }
}
