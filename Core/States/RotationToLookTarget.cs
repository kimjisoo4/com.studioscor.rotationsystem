using UnityEngine;
using StudioScor.Utilities;

namespace StudioScor.RotationSystem
{
    [AddComponentMenu("StudioScor/RotationSystem/States/Rotation To LookTarget State", order: 40)]
    public class RotationToLookTarget : RotationState
    {
        public override bool CanEnterState()
        {
            if (!base.CanEnterState())
                return false;

            if (!RotationSystem.LookTarget)
                return false;

            return true;
        }


        public override void ProcessUpdate(float deltaTime)
        {
            if(!RotationSystem.LookTarget)
            {
                RotationSystem.SetRotation(RotationSystem.transform.eulerAngles);

                return;
            }

            Vector3 rotation = RotationSystem.transform.eulerAngles;

            Vector3 direction = RotationSystem.transform.Direction(RotationSystem.LookTarget);
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Vector3 lookDirection = lookRotation.eulerAngles;

            rotation.y = Mathf.MoveTowardsAngle(rotation.y, lookDirection.y, deltaTime * RotationSystem.TurnSpeed);

            RotationSystem.SetRotation(rotation);
        }
    }
}