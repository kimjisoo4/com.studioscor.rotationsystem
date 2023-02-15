using UnityEngine;

namespace StudioScor.RotationSystem
{
    [AddComponentMenu("StudioScor/RotationSystem/States/Rotation To LookDirection State", order: 30)]
    public class RotationToLookDirection : RotationState
    {
        [Header(" [ Rotation To Look Direction ] ")]
        [SerializeField, Min(0f)] private float _TurnSpeed = 720f;

        public override bool CanEnterState()
        {
            if (!base.CanEnterState())
                return false;

            if (RotationSystem.LookDirection == default)
                return false;

            return true;
        }
        public void SetTurnSpeed(float newSpeed)
        {
            _TurnSpeed = newSpeed;
        }

        public override void ProcessUpdate(float deltaTime)
        {
            if (RotationSystem.LookDirection == Vector3.zero)
            {
                RotationSystem.SetRotation(RotationSystem.transform.eulerAngles);

                return;
            }

            Vector3 rotation = RotationSystem.transform.eulerAngles;

            Quaternion lookRotation = Quaternion.LookRotation(RotationSystem.LookDirection);
            Vector3 lookDirection = lookRotation.eulerAngles;

            rotation.y = Mathf.MoveTowardsAngle(rotation.y, lookDirection.y, deltaTime * _TurnSpeed);

            RotationSystem.SetRotation(rotation);
        }
    }
}