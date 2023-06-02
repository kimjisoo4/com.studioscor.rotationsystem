using UnityEngine;
using StudioScor.Utilities;

namespace StudioScor.RotationSystem
{
    public delegate void ChangedLookTargetHandler(IRotationSystemEvent rotationSystem, Transform currentTarget, Transform prevTarget);
    public delegate void ChangedRotationStateHandler(IRotationSystemEvent rotationSystemEvent, RotationState currentState, RotationState prevState);

    public static class RotationSystemUtility
    {
        #region GetRotationSystem
        public static IRotationSystem GetRotationSystem(this GameObject gameObject)
        {
            return gameObject.GetComponent<IRotationSystem>();
        }
        public static IRotationSystem GetRotationSystem(this Component component)
        {
            var rotationSystem = component as IRotationSystem;

            if (rotationSystem is not null)
                return rotationSystem;

            return component.GetComponent<IRotationSystem>();
        }
        public static bool TryGetRotationSystem(this GameObject gameObject, out IRotationSystem rotationSystem)
        {
            return gameObject.TryGetComponent(out rotationSystem);
        }
        public static bool TryGetRotationSystem(this Component component, out IRotationSystem rotationSystem)
        {
            rotationSystem = component as IRotationSystem;

            if (rotationSystem is not null)
                return true;

            return component.TryGetComponent(out rotationSystem);
        }
        #endregion
    }

    public interface IRotationSystem
    {
        public void SetInputDirection(Vector3 direction);
        public void SetLookDirection(Vector3 direction);
        public void SetLookTarget(Transform target);

        public void OnRotationToInput();
        public void OnRotationToLook();
        public void OnRotationToTarget();

        public void SetTurnSpeed(float turnSpeed);

        public void SetRotation(Vector3 eulerAngle, bool isImmediately = false);
        public void AddRotation(Quaternion rotation);

        public void UpdateRotation(float deltaTime);
    }

    public interface IRotationStateMachine
    {
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

        public float TurnSpeed { get; }
    }
    public interface IRotationSystemEvent
    {
        public event ChangedLookTargetHandler OnChangedLookTarget;
        public event ChangedRotationStateHandler OnChangedRotationState;
    }



    [AddComponentMenu("StudioScor/RotationSystem/Rotation System Component", order: 0)]
    public class RotationSystemComponent : BaseMonoBehaviour, IRotationSystem, IRotationSystemEvent, IRotationSystemGetter, IRotationStateMachine
    {
        [Header(" [ Rotation System ] ")]
        [SerializeField] private float turnSpeed = 720f;

        [Header(" [ Rotation State Machine ] ")]
        [SerializeField] private FiniteStateMachineSystem<RotationState> stateMachine;
        [SerializeField] private RotationState inputRotationState;
        [SerializeField] private RotationState lookRotationState;
        [SerializeField] private RotationState lookTargetRotationState;

        protected Transform lookTarget;
        protected Vector3 lookDirection;
        protected Vector3 inputDirection;
        protected Vector3 eulerRotation;

        public Transform LookTarget => lookTarget;
        public Vector3 LookDirection => lookDirection;
        public Vector3 InputDirection => inputDirection;
        public Vector3 EulerRotation => eulerRotation;
        public float TurnSpeed => turnSpeed;

        public event ChangedLookTargetHandler OnChangedLookTarget;
        public event ChangedRotationStateHandler OnChangedRotationState;

        private void Awake()
        {
            Setup();
        }

        protected void Setup()
        {
            stateMachine.Setup();
            stateMachine.OnChangedState += StateMachine_OnChangedState;

            OnSetup();
        }


        protected virtual void OnSetup() { }
        
        public bool TrySetDefaultState()
        {
            return stateMachine.TrySetDefaultState();
        }
        public void ForceSetDefaultState()
        {
            stateMachine.ForceSetDefaultState();
        }
        public bool TrySetState(RotationState rotationState)
        {
            return stateMachine.TrySetState(rotationState);
        }
        public void ForceSetState(RotationState rotationState)
        {
            stateMachine.ForceSetState(rotationState);
        }
        public void SetInputDirection(Vector3 direction)
        {
            inputDirection = direction;
        }
        public void SetLookDirection(Vector3 direction)
        {
            lookDirection = direction;
        }

        public void SetLookTarget(Transform transform = null)
        {
            if (lookTarget == transform)
                return;

            var prevTarget = lookTarget;

            lookTarget = transform;

            Callback_OnChangedLookTarget(prevTarget);
        }

        public void UpdateRotation(float deltaTime)
        {
            stateMachine.CurrentState.ProcessUpdate(deltaTime);

            OnRotation();
        }
        public void SetTurnSpeed(float newTurnSpeed)
        {
            turnSpeed = newTurnSpeed;
        }

        public void SetRotation(Vector3 eulerAngle, bool isImmediately = false)
        {
            eulerRotation = eulerAngle;

            if(isImmediately)
            {
                transform.eulerAngles = EulerRotation;
            }
        }
        public void AddRotation(Quaternion rotation)
        {
            transform.rotation *= rotation;
        }

        public void OnRotationToInput()
        {
            TrySetState(inputRotationState);
        }
        public void OnRotationToLook()
        {
            TrySetState(lookRotationState);
        }
        public void OnRotationToTarget()
        {
            TrySetState(lookTargetRotationState);
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
            Log($"On Changed Rotation State - [ Current : {stateMachine.CurrentState} | Prev : {prevState}");

            OnChangedRotationState?.Invoke(this, stateMachine.CurrentState, prevState);
        }


        #endregion
    }
}