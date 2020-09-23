using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public enum AntState
    {
        Carrying,
        Exploring
    }

    public class AntBehaviour : MonoBehaviour
    {
        private float _carryCappacity = 3f;
        private float _carrying;

        private float time;
        private float interpolationPeriod = .5f;
        public Vector3 FoodLocation;
        public AntState State;
        private Vector3 homePosition;

        private float Lk => Vector3.Distance(homePosition, FoodLocation);
        private float Q = 100;

        // Start is called before the first frame update
        void Start()
        {
            var gg = FindObjectOfType<GraphGenerator>();
            var nm = GetComponent<NavMeshAgent>();
            nm.destination = gg.GetRandomInBounds();
            homePosition = FindObjectOfType<ColonyComponent>().transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            var nm = GetComponent<NavMeshAgent>();
            GetComponent<TrailRenderer>().emitting = _carrying > 0 ;
            State = _carrying > 0 ? AntState.Carrying : AntState.Exploring;
            if (State == AntState.Carrying)
            {
                if (!nm.pathPending)
                {
                    if (nm.remainingDistance <= 3f)
                    {
                        if (!nm.hasPath || nm.velocity.sqrMagnitude < 0.5f)
                        {
                            Unload();
                        }
                    }
                }
            }
            if (State == AntState.Exploring)
            {
                if (FoodLocation != homePosition)
                {
                    nm.destination = FoodLocation;
                    return;
                }

                time += Time.deltaTime * Random.value;

                if (time >= interpolationPeriod)
                {
                    time = time - interpolationPeriod;

                    var gg = FindObjectOfType<GraphGenerator>();
                    nm.destination = gg.GetRandomInBounds();
                }
            }
        }

        private void Unload()
        {
            var colony = FindObjectOfType<ColonyComponent>();
            colony.Arrived(this, _carrying);
            _carrying = 0;
        }

        public void FoodFound(EdgeComponent component)
        {
            if (component.HasFood && component.TryTake(_carryCappacity, out var taken, out var last))
            {
                _carrying = taken;
                FoodLocation = last ? homePosition : component.transform.position;
                var nm = GetComponent<NavMeshAgent>();
                nm.destination = homePosition;
            }
            else
                FoodLocation = homePosition;
        }
    }
}