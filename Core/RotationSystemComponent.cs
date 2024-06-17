using UnityEngine;
using StudioScor.Utilities;

namespace StudioScor.RotationSystem
{
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

    public enum ERotationType
    {
        Target,
        Position,
        Direction,
        Custom,
    }

    public interface IRotationSystem
    {
        public delegate void ChangedLookTargetHandler(IRotationSystem rotationSystem, Transform currentTarget, Transform prevTarget);
        public delegate void ChangedRotationTypeHandler(IRotationSystem rotationSystem, ERotationType currentType, ERotationType prevType);

        public Transform transform { get; }
        public GameObject gameObject { get; }

        public void SetLookDirection(Vector3 direction);
        public void SetLookPosition(Vector3 position);
        public void ClearLookPosition();
        public void SetLookTarget(Transform target);
        public void ClearLookTarget();

        public void SetTurnSpeed(float turnSpeed);
        public void SetAutoTransition(bool newAutoTransition);

        public void SetRotation(Quaternion newRotation);
        public void AddRotation(Quaternion additiveRotation);

        public void UpdateRotation(float deltaTime);

        public Transform LookTarget { get; }
        public Vector3 LookDirection { get; }
        public Quaternion TurnRotation { get; }

        public ERotationType RotationType { get; }
        public float TurnSpeed { get; }
        public bool AutoTransition { get; }

        public event ChangedLookTargetHandler OnChangedLookTarget;
        public event ChangedRotationTypeHandler OnChangedRotationType;
    }

    [AddComponentMenu("StudioScor/RotationSystem/Rotation System Component", order: 0)]
    public class RotationSystemComponent : BaseMonoBehaviour, IRotationSystem
    {
        [Header(" [ Rotation System ] ")]
        [SerializeField] private float _turnSpeed = 720f;
        [SerializeField] private ERotationType _rotationType = ERotationType.Direction;
        [SerializeField] private bool _autoTransition = true;

        [Header(" Rotation Target ")]
        [SerializeField] protected Transform _lookTarget;
        [SerializeField] protected Vector3 _lookDirection;
        [SerializeField] protected Vector3 _lookPosition;

        private bool _wasClearPosition;
        protected Quaternion _turnRotation;

        protected Quaternion _overrideRotation = Quaternion.identity;
        protected Quaternion _additiveRotation = Quaternion.identity;

        private bool _wasOverrideRotation;
        private bool _wasAdditiveRotation;

        public Transform LookTarget => _lookTarget;
        public ERotationType RotationType => _rotationType;
        public Vector3 LookDirection => _lookDirection;
        public Quaternion TurnRotation => _turnRotation;
        public float TurnSpeed => _turnSpeed;
        public bool AutoTransition => _autoTransition;

        public event IRotationSystem.ChangedLookTargetHandler OnChangedLookTarget;
        public event IRotationSystem.ChangedRotationTypeHandler OnChangedRotationType;

        private void Awake()
        {
            Setup();
        }

        public void Setup()
        {
            OnSetup();
        }

        protected virtual void OnSetup() { }
        

        public void SetLookDirection(Vector3 direction)
        {
            _lookDirection = direction;   
        }
        public void SetLookPosition(Vector3 position)
        {
            _lookPosition = position;

            if(AutoTransition)
            {
                TransitionRotationType(ERotationType.Position);
            }
        }
        public void ClearLookPosition()
        {
            _lookPosition = default;
            _wasClearPosition = true;

            if (AutoTransition)
            {
                TransitionRotationType(LookTarget ? ERotationType.Target : ERotationType.Direction);
            }
        }
        public void SetLookTarget(Transform target)
        {
            if (!target)
                return;

            if (_lookTarget == transform)
                return;

            var prevTarget = _lookTarget;
            _lookTarget = target;

            Invoke_OnChangedLookTarget(prevTarget);

            if (AutoTransition)
            {
                TransitionRotationType(ERotationType.Position);
            }
        }
        public void ClearLookTarget()
        {
            _lookTarget = null;

            if(AutoTransition)
            {
                TransitionRotationType(_wasClearPosition ? ERotationType.Direction : ERotationType.Position);
            }
        }

        public void TransitionRotationType(ERotationType newRotationType)
        {
            if (_rotationType == newRotationType)
                return;

            var prevType = _rotationType;
            _rotationType = newRotationType;

            Invoke_OnChangedRotationType(prevType);
        }

        public void UpdateRotation(float deltaTime)
        {
            ProcessRotation(deltaTime);

            OnRotation();
        }

        public void SetTurnSpeed(float newTurnSpeed)
        {
            Log($"Changed Turn Speed :: Current - {newTurnSpeed} || Prev - {_turnSpeed}" );

            _turnSpeed = newTurnSpeed;
        }
        public void SetAutoTransition(bool newAutoTransition)
        {
            Log($"Changed Auto Transition State :: {newAutoTransition}");

            _autoTransition = newAutoTransition;
        }

        public void SetRotation(Quaternion setRotation)
        {
            _overrideRotation = setRotation;

            _wasOverrideRotation = true;
        }
        public void AddRotation(Quaternion additiveRotation)
        {
            _additiveRotation *= additiveRotation;

            _wasAdditiveRotation = true;
        }

        protected virtual void ProcessRotation(float deltaTime)
        {
            Quaternion newRotation = Quaternion.identity;

            if (!_wasOverrideRotation)
            {
                switch (_rotationType)
                {
                    case ERotationType.Target:
                        if (LookTarget)
                        {
                            newRotation = CalcTargetToRotation();
                        }
                        else
                        {
                            if (AutoTransition)
                            {
                                TransitionRotationType(_wasClearPosition ? ERotationType.Direction : ERotationType.Position);

                                if (_wasClearPosition)
                                {
                                    newRotation = CalcDirectionToRotation();
                                }
                                else
                                {
                                    newRotation = CalcPositionToRotation();
                                }
                            }
                            else
                            {
                                newRotation = transform.rotation;
                            }
                        }

                        break;
                    case ERotationType.Position:
                        if (!_wasClearPosition)
                        {
                            newRotation = CalcPositionToRotation();
                        }
                        else
                        {
                            if (AutoTransition)
                            {
                                if (LookTarget)
                                {
                                    TransitionRotationType(ERotationType.Target);

                                    newRotation = CalcTargetToRotation();
                                }
                                else
                                {
                                    TransitionRotationType(ERotationType.Direction);

                                    newRotation = CalcDirectionToRotation();
                                }
                            }
                            else
                            {
                                newRotation = transform.rotation;
                            }
                        }
                        break;
                    case ERotationType.Direction:
                        if (_lookDirection == Vector3.zero)
                        {
                            newRotation = transform.rotation;
                        }
                        else
                        {
                            newRotation = CalcDirectionToRotation();
                        }
                        break;
                    case ERotationType.Custom:
                        newRotation = CalcCustomRotation();
                        break;
                    default:
                        break;
                }

                Vector3 rotation = transform.eulerAngles;
                Vector3 lookDirection = newRotation.eulerAngles;

                rotation.y = Mathf.MoveTowardsAngle(rotation.y, lookDirection.y, deltaTime * TurnSpeed);

                _turnRotation = Quaternion.Euler(rotation);
            }
            else
            {
                ApplyOverrideRotation();
            }

            ApplyAdditiveRotation();
        }

        protected void ApplyOverrideRotation()
        {
            if (!_wasOverrideRotation)
                return;

            _wasOverrideRotation = false;

            _turnRotation = _overrideRotation;

            _overrideRotation = Quaternion.identity;

        }
        protected void ApplyAdditiveRotation()
        {
            if (!_wasAdditiveRotation)
                return;

            _wasAdditiveRotation = false;

            _turnRotation *= _additiveRotation;

            _additiveRotation = Quaternion.identity;
        }

        protected Quaternion CalcTargetToRotation()
        {
            Vector3 targetDirection = transform.Direction(_lookTarget);

            return Quaternion.LookRotation(targetDirection, transform.up);
        }
        protected Quaternion CalcPositionToRotation()
        {
            Vector3 direction = transform.Direction(_lookPosition);

            return Quaternion.LookRotation(direction, transform.up);
        }
        protected Quaternion CalcDirectionToRotation()
        {
            return Quaternion.LookRotation(_lookDirection, transform.up);
        }
        protected virtual Quaternion CalcCustomRotation()
        {
            return Quaternion.identity;
        }

        protected virtual void OnRotation()
        {
            transform.eulerAngles = new Vector3(0, TurnRotation.eulerAngles.y, 0);
            //transform.rotatio = TurnRotation;
        }


        #region Callback

        protected virtual void Invoke_OnChangedRotationType(ERotationType prevRotationType)
        {
            Log("On Changed Rotation Type - Current : " + _rotationType + " | Prev : " + prevRotationType);

            OnChangedRotationType?.Invoke(this, _rotationType, prevRotationType);
        }
        protected virtual void Invoke_OnChangedLookTarget(Transform prevTarget)
        {
            Log("On Changed Look Target - Current : " + LookTarget + " | Prev : " + prevTarget);

            OnChangedLookTarget?.Invoke(this, LookTarget, prevTarget);
        }
        #endregion
    }
}