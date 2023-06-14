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
        public Transform transform { get; }
        public GameObject gameObject { get; }

        public void SetInputDirection(Vector3 direction);
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
    public class RotationSystemComponent : BaseMonoBehaviour, IRotationSystem, IRotationSystemEvent, IRotationSystemGetter
    {
        [Header(" [ Rotation System ] ")]
        [SerializeField] private float turnSpeed = 720f;

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
            OnSetup();
        }


        protected virtual void OnSetup() { }
        
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
            ProcessRotation(deltaTime);

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

        protected virtual void ProcessRotation(float deltaTime)
        {
            if (InputDirection == Vector3.zero)
            {
                SetRotation(transform.eulerAngles);

                return;
            }

            Vector3 rotation = transform.eulerAngles;

            Quaternion lookRotation = Quaternion.LookRotation(InputDirection);
            Vector3 lookDirection = lookRotation.eulerAngles;

            rotation.y = Mathf.MoveTowardsAngle(rotation.y, lookDirection.y, deltaTime * TurnSpeed);

            SetRotation(rotation);
        }

        protected virtual void OnRotation()
        {
            transform.eulerAngles = EulerRotation;
        }


        #region Callback
        protected void Callback_OnChangedLookTarget(Transform prevTarget)
        {
            Log("On Changed Look Target - Current : " + LookTarget + " | Prev : " + prevTarget);

            OnChangedLookTarget?.Invoke(this, LookTarget, prevTarget);
        }
        #endregion
    }
}