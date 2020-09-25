using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public enum AntState
    {
        Exploring,
        Returning
    }

    public class AntBehaviour : MonoBehaviour
    {
        public AntState State;

        private float Q => 1f / pool.Count;
        private List<int> visitedVerts = new List<int>();
        private int currentIndex;
        [SerializeField]
        private HashSet<Edge> edges;

        private float alfa, beta;
        private List<EdgeComponent> pool;
        private EdgeComponent _home;
        private bool nextLayer;
        private float p;

        // Start is called before the first frame update
        void Start()
        {
            var g = FindObjectOfType<GraphGenerator>();
            alfa = g.alfa;
            beta = g.beta;
            p = g.p;
            ShooldReset();
            edges = g.Edges;
            pool = g.Pool;
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
            nm.destination = to;
            ShooldReset();
        }

        public EdgeComponent TryGetNextNode(IList<int> visited)
        {
            var remaining = pool.Where(x => !visited.Contains(x.VertexId)).ToList();
            if (nextLayer)
            {
                if (!remaining.Any())
                    return default;
                if (remaining.Count == 1)
                    return remaining.Single();

                var last = visited.Last();

                var targets = new List<Edge>();
                foreach (EdgeComponent t in remaining)
                    targets.Add(edges.First(x => x.Is(last, t.VertexId)));

                var g = FindObjectOfType<GraphGenerator>();
                var sumEva = targets.Sum(x =>
                {
                    var distance = 1f / g.Distance(x);
                    return Mathf.Pow(x.Value, alfa) * Mathf.Pow(distance, beta);
                });

                var next = new Dictionary<int, float>();
                foreach (var edge in targets)
                {
                    var distance = 1f / g.Distance(edge);
                    next[last == edge.VertFrom ? edge.VertTo : edge.VertFrom] =
                        (Mathf.Pow(edge.Value, alfa) * Mathf.Pow(distance, beta)) / sumEva; 
                }
                Debug.Log("sum=> " + next.Values.Sum());
                var r = Random.value;
                var to = remaining.Select(x =>
                    new { a = edges.First(v => v.Is(last, x.VertexId)), x }).ToArray();

                return to
                    .Where(x => r < next[x.x.VertexId])
                    .Select(x => x.x)
                    .FirstOrDefault();
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
                CalculatePath();
                nextLayer = true;
            }
        }
        private void CalculatePath()
        {
            var g = FindObjectOfType<GraphGenerator>();

            var visitedLocal = new List<Edge>();
            for (int i = 0; i < visitedVerts.Count - 1; i++)
            {
                visitedLocal.Add(GetEdgeFrom(i));
            }

            var sum = visitedLocal.Sum(i => g.Distance(i));
            var last = edges.First(x => x.Is(0, pool.Count - 1));
            sum += g.Distance(last);


            foreach (var edge in visitedLocal)
            {
                edge.Value = (edge.Value + Q / sum) * p;
            }

            foreach (var edge in edges)
            {
                edge.Value = edge.Value * (1 - p);
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
                val = pool[visitedVerts[--currentIndex]].transform.position;
                return true;
            }
            val = pool[visitedVerts.First()].transform.position;
            return false;
        }

        public void Home(EdgeComponent edge)
        {
            _home = edge;
            ShooldReset();
        }
    }
}