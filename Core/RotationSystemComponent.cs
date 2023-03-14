using UnityEngine;
using StudioScor.Utilities;

namespace StudioScor.RotationSystem
{
    public interface IRotationSystem
    {
        public void SetInputDirection(Vector3 direction);
    }

    [AddComponentMenu("StudioScor/RotationSystem/Rotation System Component", order: 0)]
    public class RotationSystemComponent : BaseMonoBehaviour, IRotationSystem
    {
        #region Events
        public delegate void ChangedLookTargetHandler(RotationSystemComponent rotationSystem, Transform currentTarget, Transform prevTarget);
        #endregion
        [Header(" [ Rotation System ] ")]
        [SerializeField] private Transform _Transform;
        [SerializeField] private FiniteStateMachineSystem<RotationState> _StateMachine;

        protected Transform _LookTarget;
        protected Vector3 _LookDirection;
        protected Vector3 _InputDirection;
        protected Vector3 _Rotation;

        public Transform LookTarget => _LookTarget;
        public Vector3 LookDirection => _LookDirection;
        public Vector3 InputDirection => _InputDirection;
        public Vector3 Rotation => _Rotation;

        public event ChangedLookTargetHandler OnChangedLookTarget;

        private void Awake()
        {
            Setup();
        }

        protected void Setup()
        {
            _StateMachine.Setup();

            if (!_Transform)
                _Transform = transform;

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

            OnRotation(deltaTime);
        }

        public void SetRotation(Vector3 eulerAngle)
        {
            _Rotation = eulerAngle;
        }

        protected virtual void OnRotation(float deltaTime)
        {
            _Transform.eulerAngles = Rotation;
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