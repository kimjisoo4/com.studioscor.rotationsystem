using UnityEngine;
using StudioScor.Utilities;

namespace StudioScor.RotationSystem
{
    public delegate void ChangedLookTargetHandler(IRotationSystemEvent rotationSystem, Transform currentTarget, Transform prevTarget);
    public delegate void ChangedRotationStateHandler(IRotationSystemEvent rotationSystemEvent, RotationState currentState, RotationState prevState);

    public interface IRotationSystem
    {
        public void SetInputDirection(Vector3 direction);
        public void SetLookDirection(Vector3 direction);
        public void SetLookTarget(Transform target);

        public void SetRotation(Vector3 eulerAngle, bool isImmediately = false);
        public void AddRotation(Quaternion rotation);
        public void UpdateRotation(float deltaTime);

        public bool TrySetDefaultState();
        public void ForceSetDefaultState();
        public bool TrySetState(RotationState rotationState);
        public void ForceSetState(RotationState rotationState);
    }
    public interface IRotationSystemGetter
    {
        public Transform LookTarget { get; }
        public Vector3 LookDirection { get; }
        public Vector3 InputDirection { get; }
        public Vector3 EulerRotation { get; }
    }
    public interface IRotationSystemEvent
    {
        public event ChangedLookTargetHandler OnChangedLookTarget;
        public event ChangedRotationStateHandler OnChangedRotationState;
    }


    [AddComponentMenu("StudioScor/RotationSystem/Rotation System Component", order: 0)]
    public class RotationSystemComponent : BaseMonoBehaviour, IRotationSystem, IRotationSystemEvent, IRotationSystemGetter
    {
        [Header(" [ Rotation System ] ")]
        [SerializeField] private FiniteStateMachineSystem<RotationState> _StateMachine;

        protected Transform _LookTarget;
        protected Vector3 _LookDirection;
        protected Vector3 _InputDirection;
        protected Vector3 _EulerRotation;

        public Transform LookTarget => _LookTarget;
        public Vector3 LookDirection => _LookDirection;
        public Vector3 InputDirection => _InputDirection;
        public Vector3 EulerRotation => _EulerRotation;

        public event ChangedLookTargetHandler OnChangedLookTarget;
        public event ChangedRotationStateHandler OnChangedRotationState;

        private void Awake()
        {
            Setup();
        }

        protected void Setup()
        {
            _StateMachine.Setup();
            _StateMachine.OnChangedState += StateMachine_OnChangedState;

            OnSetup();
        }


        protected virtual void OnSetup() { }
        
        public bool TrySetDefaultState()
        {
            return _StateMachine.TrySetDefaultState();
        }
        public void ForceSetDefaultState()
        {
            _StateMachine.ForceSetDefaultState();
        }
        public bool TrySetState(RotationState rotationState)
        {
            return _StateMachine.TrySetState(rotationState);
        }
        public void ForceSetState(RotationState rotationState)
        {
            _StateMachine.ForceSetState(rotationState);
        }
        public void SetInputDirection(Vector3 direction)
        {
            _InputDirection = direction;
        }
        public void SetLookDirection(Vector3 direction)
        {
            _LookDirection = direction;
        }

        public void SetLookTarget(Transform transform = null)
        {
            if (_LookTarget == transform)
                return;

            var prevTarget = _LookTarget;

            _LookTarget = transform;

            Callback_OnChangedLookTarget(prevTarget);
        }

        public void UpdateRotation(float deltaTime)
        {
            _StateMachine.CurrentState.ProcessUpdate(deltaTime);

            OnRotation();
        }

        public void SetRotation(Vector3 eulerAngle, bool isImmediately = false)
        {
            _EulerRotation = eulerAngle;

            if(isImmediately)
            {
                transform.eulerAngles = EulerRotation;
            }
        }
        public void AddRotation(Quaternion rotation)
        {
            transform.rotation *= rotation;
        }

        protected virtual void OnRotation()
        {
            transform.eulerAngles = EulerRotation;
        }


        private void StateMachine_OnChangedState(FiniteStateMachineSystem<RotationState> stateMachine, RotationState currentState, RotationState prevState)
        {
            Callback_OnChangedRotationState(prevState);
        }

        #region Callback
        protected void Callback_OnChangedLookTarget(Transform prevTarget)
        {
            Log("On Changed Look Target - Current : " + LookTarget + " | Prev : " + prevTarget);

            OnChangedLookTarget?.Invoke(this, LookTarget, prevTarget);
        }
        protected void Callback_OnChangedRotationState(RotationState prevState)
        {
            Log($"On Changed Rotation State - [ Current : {_StateMachine.CurrentState} | Prev : {prevState}");

            OnChangedRotationState?.Invoke(this, _StateMachine.CurrentState, prevState);
        }
        #endregion
    }
}