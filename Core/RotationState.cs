using UnityEngine;
using StudioScor.Utilities;

namespace StudioScor.RotationSystem
{
    [AddComponentMenu("StudioScor/RotationSystem/States/Rotation State", order: 0)]
    public abstract class RotationState : BaseStateMono
    {
        [Header(" [ Rotation System ] ")]
        [SerializeField] private RotationSystemComponent _RotationSystem;

        protected RotationSystemComponent RotationSystem => _RotationSystem;

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            gameObject.TryGetComponentInParentOrChildren(out _RotationSystem);
        }
#endif

        private void Awake()
        {
            if (!_RotationSystem)
            {
                if (!gameObject.TryGetComponentInParentOrChildren(out _RotationSystem))
                {
                    Log("Rotation System is NULL!", true);
                }
            }

            OnSetup();
        }

        protected virtual void OnSetup() { }

        public abstract void ProcessUpdate(float deltaTime);
    }
}