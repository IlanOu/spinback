using System.Collections;
using UnityEngine;
using UnityEngine.AI;


namespace NPC
{

    // Manages the behavior of NPCs in a nightclub environment
    public class NpcController : MonoBehaviour
    {
        // References
        private NavMeshAgent _navAgent;
        private Animator _animator;
        
        // Behavior parameters
        [SerializeField] private float minWanderDistance = 5f;
        [SerializeField] private float maxWanderDistance = 15f;
        [SerializeField] private float minWaitTime = 5f;
        [SerializeField] private float maxWaitTime = 20f;
        [SerializeField] private float interactionRadius = 3f;
        
        // States
        private enum NpcState { Idle, Walking, Dancing, Talking }
        private NpcState _currentState;
        private NpcController _talkingPartner;
        
        // Animation parameter names
        private readonly string _isWalkingParam = "IsWalking";
        private readonly string _isDancingParam = "IsDancing";
        private readonly string _isTalkingParam = "IsTalking";
        
        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _currentState = NpcState.Idle;
        }
        
        private void Start()
        {
            // Start behavior cycle
            StartCoroutine(BehaviorRoutine());
        }
        
        private IEnumerator BehaviorRoutine()
        {
            while (true)
            {
                // Choose a random behavior based on current state
                switch (_currentState)
                {
                    case NpcState.Idle:
                        yield return ChooseRandomBehavior();
                        break;
                        
                    case NpcState.Walking:
                        // Wait until we reach destination
                        yield return new WaitUntil(() => _navAgent.remainingDistance <= _navAgent.stoppingDistance);
                        SetState(NpcState.Idle);
                        break;
                        
                    case NpcState.Dancing:
                        // Dance for a random duration
                        yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
                        SetState(NpcState.Idle);
                        break;
                        
                    case NpcState.Talking:
                        // Talk until conversation ends
                        if (_talkingPartner == null || _talkingPartner._currentState != NpcState.Talking)
                        {
                            SetState(NpcState.Idle);
                        }
                        yield return new WaitForSeconds(1f);
                        break;
                }
                
                yield return null;
            }
        }
        
        private IEnumerator ChooseRandomBehavior()
        {
            // Wait for a moment before deciding
            yield return new WaitForSeconds(Random.Range(1f, 3f));
            
            // Choose a random behavior
            float choice = Random.value;
            
            if (choice < 0.4f)
            {
                // 40% chance to walk somewhere
                WanderToRandomPoint();
            }
            else if (choice < 0.7f)
            {
                // 30% chance to dance
                StartDancing();
            }
            else
            {
                // 30% chance to try talking to someone
                TryFindConversationPartner();
            }
        }
        
        private void WanderToRandomPoint()
        {
            // Find a random point on the NavMesh to walk to
            Vector3 randomDirection = Random.insideUnitSphere * Random.Range(minWanderDistance, maxWanderDistance);
            randomDirection += transform.position;
            NavMeshHit hit;
            
            if (NavMesh.SamplePosition(randomDirection, out hit, maxWanderDistance, NavMesh.AllAreas))
            {
                _navAgent.SetDestination(hit.position);
                SetState(NpcState.Walking);
            }
        }
        
        private void StartDancing()
        {
            // Stop moving and start dancing
            _navAgent.ResetPath();
            SetState(NpcState.Dancing);
        }
        
        private void TryFindConversationPartner()
        {
            // Look for nearby NPCs to talk to
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, interactionRadius);
            
            foreach (Collider col in nearbyColliders)
            {
                NpcController otherNpc = col.GetComponent<NpcController>();
                
                // Check if it's a valid conversation partner
                if (otherNpc != null && otherNpc != this && otherNpc._currentState == NpcState.Idle)
                {
                    // Start conversation
                    StartConversation(otherNpc);
                    return;
                }
            }
            
            // No partner found, go back to idle
            SetState(NpcState.Idle);
        }
        
        private void StartConversation(NpcController partner)
        {
            // Stop moving
            _navAgent.ResetPath();
            
            // Face each other
            Vector3 directionToPartner = partner.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(directionToPartner);
            
            // Set talking state for both NPCs
            _talkingPartner = partner;
            SetState(NpcState.Talking);
            
            // Tell partner to talk back
            partner.JoinConversation(this);
        }
        
        public void JoinConversation(NpcController initiator)
        {
            // Stop current activity
            _navAgent.ResetPath();
            
            // Face the conversation initiator
            Vector3 directionToInitiator = initiator.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(directionToInitiator);
            
            // Set talking state
            _talkingPartner = initiator;
            SetState(NpcState.Talking);
        }
        
        private void SetState(NpcState newState)
        {
            // Update state
            _currentState = newState;
            
            // Update animations
            _animator.SetBool(_isWalkingParam, newState == NpcState.Walking);
            _animator.SetBool(_isDancingParam, newState == NpcState.Dancing);
            _animator.SetBool(_isTalkingParam, newState == NpcState.Talking);
        }
        
        // Helper method to end conversation if needed
        public void EndConversation()
        {
            if (_currentState == NpcState.Talking && _talkingPartner != null)
            {
                _talkingPartner.SetState(NpcState.Idle);
                _talkingPartner = null;
                SetState(NpcState.Idle);
            }
        }
    }
}