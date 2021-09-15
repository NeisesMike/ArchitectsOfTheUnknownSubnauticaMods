using RotA.Mono.Creatures.GargEssentials;
using Story;
using UnityEngine;

namespace RotA.Mono
{
    /// <summary>
    /// This goes on the Gargantuan to play a PDA line automatically when in range.
    /// </summary>
    public class GargantuanEncounterPDA : MonoBehaviour
    {
        StoryGoal goal = new StoryGoal("GargantuanEncounter", Story.GoalType.Story, 0f);
        public float maxDistance = 300f;

        private void Start()
        {
            if (StoryGoalManager.main.IsGoalComplete(goal.key))
            {
                Destroy(this);
            }
            else
            {
                InvokeRepeating(nameof(CheckDistance), Random.value, 0.5f);
            }
        }

        private void CheckDistance()
        {
            if (Vector3.Distance(transform.position, Player.main.transform.position) > maxDistance) return;

            if (GargantuanConditions.PlayerInPrecursorBase()) return;
            
            if (StoryGoalManager.main.OnGoalComplete(goal.key))
            {
                CustomPDALinesManager.PlayPDAVoiceLine(Mod.assetBundle.LoadAsset<AudioClip>("PDAGargEncounter"), "PDAGargEncounter");
                CancelInvoke();
                Invoke(nameof(UnlockCyclopsScanner), 14f);
            }
        }

        private void UnlockCyclopsScanner()
        {
            KnownTech.Add(Mod.cyclopsScannerModule.TechType);
            Destroy(this);
        }
    }
}
