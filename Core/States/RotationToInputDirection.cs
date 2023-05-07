using UnityEngine;

namespace StudioScor.RotationSystem
{
    [AddComponentMenu("StudioScor/RotationSystem/States/Rotation To InputDirection State", order: 10)]
    public class RotationToInputDirection : RotationState
    {
        [Header(" [ Rotation To Look Direction ] ")]
        [SerializeField, Min(0f)] private float _TurnSpeed = 720f;

        public void SetTurnSpeed(float newSpeed)
        {
            _TurnSpeed = newSpeed;
        }

        public override void ProcessUpdate(float deltaTime)
        {
            if (RotationSystem.InputDirection == Vector3.zero)
            {
                RotationSystem.SetRotation(RotationSystem.transform.eulerAngles);

                return;
            }

            Vector3 rotation = RotationSystem.transform.eulerAngles;

            Quaternion lookRotation = Quaternion.LookRotation(RotationSystem.InputDirection);
            Vector3 lookDirection = lookRotation.eulerAngles;

            rotation.y = Mathf.MoveTowardsAngle(rotation.y, lookDirection.y, deltaTime * _TurnSpeed);

            RotationSystem.SetRotation(rotation);
        }
    }
}