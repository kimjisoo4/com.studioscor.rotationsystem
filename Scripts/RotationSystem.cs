using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KimScor.RotationSystem
{

    public abstract class RotationSystem : MonoBehaviour
    {
        [Header(" [ Setting ] ")]
        [SerializeField] protected ERotationType _RotationType;
        [SerializeField] protected float _TurnSpeed = 360;
        public float TurnSpeed => _TurnSpeed;

        [SerializeField] protected Transform _TargetTransform;
        [SerializeField] protected Transform _CameraTransform;
        [SerializeField] protected bool _UseRotation = false;

        [SerializeField] public bool UseRotation => _UseRotation;
        [SerializeField] public Transform TargetTransform => _TargetTransform;
        [SerializeField] public Transform CameraTransform => _CameraTransform;


        [Header("[Debug Mode]")]
        [SerializeField] protected bool DebugMode = false;

        protected Vector3 _TurnEulerAngles = Vector3.zero;
        protected Vector3 _RotationDirection = Vector3.zero;
        public Vector3 TurnEulerAngles => _TurnEulerAngles;
        public Vector3 RotationDirection => _RotationDirection;


        #region Setter
        public void SetRotationType(ERotationType newRotationType)
        {
            _RotationType = newRotationType;
        }
        public void SetTurnSpeed(float newTurnSpeed)
        {
            _TurnSpeed = newTurnSpeed;
        }
        public void SetUseRotation(bool useRotation)
        {
            _UseRotation = useRotation;
        }
        public void SetRotationDirection(Vector3 Direction)
        {
            _RotationDirection = Direction;
        }

        public void SetRotationTarget(Transform target)
        {
            _TargetTransform = target;
        }
        #endregion

        private void Awake()
        {
            _CameraTransform = Camera.main.transform;
        }

        public virtual void OnRotation(float deltaTime)
        {
            if (!_UseRotation)
            {
                _TurnEulerAngles = transform.eulerAngles;

                UpdateRotation(deltaTime);

                return;
            }

            switch (_RotationType)
            {
                case ERotationType.Direction:
                    OnRotationToDirection();
                    break;
                case ERotationType.Target:
                    OnRotationToTarget();
                    break;
                case ERotationType.TargetOrDirection:
                    OnRotationToTargetOrDirection();
                    break;
                case ERotationType.Camera:
                    OnRotationToCamera();
                    break;
                default:
                    break;
            }

            UpdateRotation(deltaTime);
        }

        protected abstract void UpdateRotation(float deltaTime);

        public virtual void OnRotationToDirection()
        {
            if (RotationDirection == Vector3.zero)
            {
                _TurnEulerAngles = transform.eulerAngles;

                return;
            }

            Quaternion newRotation = Quaternion.LookRotation(RotationDirection);

            _TurnEulerAngles = newRotation.eulerAngles;
        }

        public virtual void OnRotationToTarget()
        {
            if (TargetTransform == null)
            {
                _TurnEulerAngles = transform.eulerAngles;

                return;
            }

            Vector3 direction = TargetTransform.position - transform.position;

            direction.Normalize();

            Quaternion newRotation = Quaternion.LookRotation(direction);

            _TurnEulerAngles = newRotation.eulerAngles;
        }

        public virtual void OnRotationToCamera()
        {
            if (CameraTransform == null)
            {
                _TurnEulerAngles = transform.eulerAngles;

                return;
            }

            Vector3 direction = CameraTransform.forward;

            Quaternion newRotation = Quaternion.LookRotation(direction);

            _TurnEulerAngles = newRotation.eulerAngles;

        }

        public virtual void OnRotationToTargetOrDirection()
        {
            if (TargetTransform != null)
            {
                OnRotationToTarget();
            }
            else
            {
                OnRotationToDirection();
            }
        }
    }
}