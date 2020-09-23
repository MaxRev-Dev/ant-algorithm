using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class Edge : ICloneable
    {
        public int VertFrom { get; }
        public int VertTo { get; } 
        public float Value { get; set; } = 1;
        public float P { get; set; }
         
        public Edge(int vertFrom, int vertTo)
        {
            VertFrom = vertFrom;
            VertTo = vertTo; 
        }

        public bool Is(int v1, int v2)
        {
            return VertTo == v1 && VertFrom == v2 ||
                   VertFrom == v1 && VertTo == v2;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
    public class GraphGenerator : MonoBehaviour
    {
        public int VerticesCount = 100;
        public int PlaneSize = 100;
        public int SafePad = 1;
        public EdgeComponent EdgePrefab;
        public float alfa = .1f;
        public float beta = .6f;
        public List<EdgeComponent> Pool { get; set; }
        public HashSet<Edge> Edges { get; set; }

        private void Awake()
        {
            if (VerticesCount < 1 || VerticesCount > 1000)
            {
                VerticesCount = 50;
                Debug.Log("Out of Range => Vertices count was set to default");
            }

            var vertices = Enumerable.Range(0, VerticesCount).ToArray();
            var center = PlaneSize / 2;
              
            Pool = vertices
                .Select(x =>
                {
                    var (posX, posY) =
                        RandomExclude(-center + SafePad, center - SafePad, -SafePad, SafePad);
                    var item = Instantiate(EdgePrefab,
                        new Vector3(posX, 0.2f, posY), Quaternion.identity, transform);  
                    item.VertexId = x;
                    return item;
                }).ToList();

            var prm = Permutations(Pool, 2).Select(x => x.ToArray()).ToArray();

            var zip = prm
                .Select(x => new Edge(x[0].VertexId, x[1].VertexId)).ToArray();
            Debug.Log(zip.Length);
            Edges = new HashSet<Edge>(zip);
        }

        private (int, int) RandomExclude(int min, int max, int innerMin, int innerMax)
        {
            var or = Random.Range(0, 2) == 1;
            return (or ? Random.Range(min, max) :
                Random.Range(0, 2) == 1 ? Random.Range(min, innerMin + 1) : Random.Range(max, innerMax + 1),
                !or ? Random.Range(min, max) :
                Random.Range(0, 2) == 1 ? Random.Range(min, innerMin + 1) : Random.Range(max, innerMax + 1));
        }

        public Vector3 GetRandomInBounds()
        {
            var center = PlaneSize / 2;
            return new Vector3(Random.Range(-center, center), 0, Random.Range(-center, center));
        }

        public EdgeComponent GetNextNode(IEnumerable<EdgeComponent> visited)
        {
            var remaining = Pool.Except(visited).ToList();
            var node = remaining[Random.Range(0, remaining.Count)];
            return node;
        }


        public static IEnumerable<IEnumerable<T>> Permutations<T>(IEnumerable<T> array, int elementsInArray)
        {
            var range = array as T[] ?? array.ToArray();
            var i = 1;
            foreach (var item in range)
            {
                if (elementsInArray == 1)
                {
                    yield return new[] { item };
                }
                else
                {
                    foreach (var result in Permutations(range.Skip(i++), elementsInArray - 1))
                        yield return new[] { item }.Concat(result);
                }
            }
        }
    }
}
