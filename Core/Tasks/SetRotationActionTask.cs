using StudioScor.Utilities;
using System;
using UnityEngine;

namespace StudioScor.RotationSystem
{

    [Serializable]
    public class ReachTurnRotateActionTask : Task, ISubTask
    {
        [Header( " [ Reach Turn Rotate Action Task ] " )]
#if SCOR_ENABLE_SERIALIZEREFERENCE
        [SerializeReference, SerializeReferenceDropdown]
#endif
        private IDirectionVariable _direction = new LocalDirectionVariable(Vector3.forward);

        [SerializeField][Range(0f, 1f)] private float _turnStartTime = 0f;
        [SerializeField][Range(0f, 1f)] private float _turnEndTime = 1f;
        [SerializeField] private bool _updatableDirection = false;

        private IRotationSystem _rotationSystem;
        private ReachTurnRotateActionTask _original;

        private Vector3 _turnDirection;
        private float _turnStart;
        private float _turnEnd;
        private bool _updatable;

        protected override void SetupTask()
        {
            base.SetupTask();

            _rotationSystem = Owner.GetRotationSystem();

            _direction.Setup(Owner);
        }
        public override ITask Clone()
        {
            var clone = new ReachTurnRotateActionTask();

            clone._original = this;
            clone._direction = _direction.Clone();

            return clone;
        }

        protected override void EnterTask()
        {
            base.EnterTask();

            bool isOriginal = _original is null;

            _updatable = isOriginal ? _updatableDirection : _original._updatableDirection;
            _turnStart = isOriginal ? _turnStartTime : _original._turnStartTime;
            _turnEnd = isOriginal ? _turnEndTime : _original._turnEndTime;
            _turnDirection = _direction.GetValue();
        }

        public void FixedUpdateSubTask(float deltaTime, float normalizedTime)
        {
            return;
        }

        public void UpdateSubTask(float deltaTime, float normalizedTime)
        {
            if (!normalizedTime.InRange(_turnStart, _turnEnd))
            {
                return;
            }

            float lerp = Mathf.InverseLerp(_turnStart, _turnEnd, normalizedTime);

            if(_updatable)
            {
                _turnDirection = _direction.GetValue();
            }

            var rotation = Quaternion.LookRotation(_turnDirection, Owner.transform.up);

            rotation = Quaternion.Lerp(Owner.transform.rotation, rotation, lerp);

            _rotationSystem.SetRotation(rotation);
        }
    }
    [Serializable]
    public class SetRotationActionTask : Task, ISubTask
    {
        [Header(" [ Set Rotation Action Task ] ")]
#if SCOR_ENABLE_SERIALIZEREFERENCE
        [SerializeReference, SerializeReferenceDropdown]
#endif
        private IDirectionVariable _direction = new LocalDirectionVariable(Vector3.forward);

        [SerializeField] private bool _isImmediately = false;

        private IRotationSystem _rotationSystem;

        private SetRotationActionTask _original;

        protected override void SetupTask()
        {
            base.SetupTask();

            _rotationSystem = Owner.GetRotationSystem();

            _direction.Setup(Owner);
        }

        public override ITask Clone()
        {
            var clone = new SetRotationActionTask();

            clone._original = this;
            clone._direction = _direction.Clone();

            return clone;
        }

        protected override void EnterTask()
        {
            base.EnterTask();

            var isImmediately = _original is null ? _isImmediately : _original._isImmediately;
            
            if (isImmediately)
            {
                Vector3 direction = _direction.GetValue();

                Quaternion rotation = Quaternion.LookRotation(direction, Owner.transform.up);

                _rotationSystem.SetRotation(rotation);
            }
            else
            {
                Vector3 direction = _direction.GetValue();

                _rotationSystem.SetLookDirection(direction);
            }
        }

        public void FixedUpdateSubTask(float deltaTime, float normalizedTime)
        {
            return;
        }
        public void UpdateSubTask(float deltaTime, float normalizedTime)
        {
        }
    }
}
