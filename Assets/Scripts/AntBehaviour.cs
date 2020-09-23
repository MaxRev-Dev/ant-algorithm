using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public enum AntState
    {
        Exploring,
        Returning,
        Idle
    }

    public class AntBehaviour : MonoBehaviour
    {
        public AntState State;

        private float Q = 100;
        private List<int> visitedVerts = new List<int>();
        private int currentIndex;

        private HashSet<Edge> edges;

        private float alfa, beta;
        private List<EdgeComponent> pool;
        private EdgeComponent _home;

        // Start is called before the first frame update
        void Start()
        {
            var g = FindObjectOfType<GraphGenerator>();
            alfa = g.alfa;
            beta = g.beta;
            ShooldReset();
        }

        private void ShooldReset()
        {
            visitedVerts.Clear();
            visitedVerts.Add(_home.VertexId);
            State = AntState.Exploring;
            var nm = GetComponent<NavMeshAgent>();
            nm.destination = _home.transform.position;
            currentIndex = 0;
        }

        // Update is called once per frame
        void Update()
        {
            var nm = GetComponent<NavMeshAgent>();

            GetComponent<TrailRenderer>().emitting = State == AntState.Returning;

            if (nm.pathPending)
                return;
            if (!(nm.remainingDistance <= 0.1f))
                return;
            if (nm.hasPath && !(nm.velocity.sqrMagnitude < 0.01f))
                return;
            switch (State)
            {
                case AntState.Returning:
                    MoveToPreviousNode();
                    break;
                case AntState.Exploring:
                    MoveToNextNode();
                    break;
            }
        }

        private void MoveToPreviousNode()
        {
            //Debug.Log($"moving to previous node from {currentIndex}");
            var nm = GetComponent<NavMeshAgent>();
            if (TryGetPrevPosition(out var to))
            {
                nm.destination = to;
                return;
            }

            Debug.Log("clearing and restarting");
            ShooldReset();
        }

        public EdgeComponent TryGetNextNode(IList<int> visited)
        {
            var graphGenerator = FindObjectOfType<GraphGenerator>();
            pool = graphGenerator.Pool;
            var remaining = pool.Where(x => !visited.Contains(x.VertexId)).ToList();
            if (edges != default)
            {
                if (!remaining.Any()) return default;
                var selected = edges
                    .OrderByDescending(x => x.P);

                return remaining
                    .Join(selected, x => x.VertexId, x => x.VertTo, (x, y) => x)
                    .First();
            }

            if (remaining.Any())
            {
                var node = remaining[Random.Range(0, remaining.Count)];
                return node;
            }

            return default;
        }

        private void MoveToNextNode()
        {
            // Debug.Log($"moving to next node from {currentIndex }");
            var nm = GetComponent<NavMeshAgent>();
            var newNode = TryGetNextNode(visitedVerts);
            if (newNode)
            {
                if (!visitedVerts.Contains(newNode.VertexId))
                {
                    currentIndex++;
                    visitedVerts.Add(newNode.VertexId);
                }
                nm.destination = newNode.transform.position;
            }
            else
            {
                State = AntState.Returning;

                Print("visited", visitedVerts);

                if (edges == default)
                {
                    var drgen = FindObjectOfType<GraphGenerator>();
                    edges = new HashSet<Edge>(drgen.Edges.Select(x => (Edge)x.Clone()));
                    pool = drgen.Pool;
                }

                var sum = 0f;
                for (int i = 0; i <= visitedVerts.Count - 2; i++)
                {
                    var edge = GetEdgeFrom(i);
                    var edgeFrom = pool.First(x => edge.VertFrom == x.VertexId);
                    var edgeTo = pool.First(x => edge.VertTo == x.VertexId);

                    var distRaw = Vector3.Distance(edgeFrom.transform.position, edgeTo.transform.position);

                    sum += distRaw;
                }

                for (int i = 0; i <= visitedVerts.Count - 2; i++)
                {
                    var edge = GetEdgeFrom(i);
                    edge.Value = (edge.Value + Q / sum) * .6f;
                }

                var sumEva = Mathf.Pow(edges.Sum(x => x.Value), alfa);

                for (int i = 0; i <= visitedVerts.Count - 2; i++)
                {
                    var edge = GetEdgeFrom(i);
                    var edgeFrom = pool.First(x => edge.VertFrom == x.VertexId);
                    var edgeTo = pool.First(x => edge.VertTo == x.VertexId);
                    var distRaw = Vector3.Distance(edgeFrom.transform.position, edgeTo.transform.position);
                    var distance = 1f / distRaw;
                    edge.P = (Mathf.Pow(edge.Value, alfa) * Mathf.Pow(distance, beta) /
                             (sumEva * Mathf.Pow(distance, beta)));
                }

            }
        }

        public static void Print<T>(string mess, List<T> items)
        {
            Debug.Log(mess + ": " + string.Join("|", items));
        }

        private Edge GetEdgeFrom(int i)
        {
            return edges.First(x => x.Is(visitedVerts[i], visitedVerts[i + 1]));
        }

        private bool TryGetPrevPosition(out Vector3 val)
        {
            if (currentIndex > 0)
            {
                var v = visitedVerts[--currentIndex];
                val = pool.First(x => x.VertexId == v).transform.position;
                return true;
            }

            var prim = visitedVerts.First();
            val = pool.First(x => x.VertexId == prim).transform.position;
            return false;
        }

        public void Home(EdgeComponent edge)
        {
            _home = edge;
            ShooldReset();
        }
    }
}