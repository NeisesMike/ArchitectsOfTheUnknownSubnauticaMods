using System;
using RotA.Mono.Creatures.GargEssentials;
using UnityEngine;

namespace RotA.Commands
{
    public class GargDebugger : MonoBehaviour
    {
        private GargantuanBehaviour _behaviour;
        private Creature _creature;

        private float _timeDebugAgain;

        private const float kDebugInterval = 0.1f;

        private void Awake()
        {
            _behaviour = GetComponent<GargantuanBehaviour>();
            _creature = GetComponent<Creature>();
        }

        private void Update()
        {
            if (Time.time < _timeDebugAgain) return;
            
            ErrorMessage.AddMessage($"Current creature action: {_creature.prevBestAction}");
            _timeDebugAgain = Time.time + kDebugInterval;
        }
    }
}