using UnityEngine;

namespace StudioScor.RotationSystem
{
    [AddComponentMenu("StudioScor/RotationSystem/States/Rotation To LookDirection State", order: 30)]
    public class RotationToLookDirection : RotationState
    {
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

            rotation.y = Mathf.MoveTowardsAngle(rotation.y, lookDirection.y, deltaTime * RotationSystem.TurnSpeed);

            RotationSystem.SetRotation(rotation);
        }
    }
}