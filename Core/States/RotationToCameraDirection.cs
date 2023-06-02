using UnityEngine;

namespace StudioScor.RotationSystem
{
    [AddComponentMenu("StudioScor/RotationSystem/States/Rotation To Camera Direction State", order: 20)]
    public class RotationToCameraDirection : RotationState
    {
        private Transform cameraTransform;

        protected override void OnSetup()
        {
            base.OnSetup();

            cameraTransform = Camera.main.transform;
        }
        public override void ProcessUpdate(float deltaTime)
        {
            Vector3 rotation = RotationSystem.transform.eulerAngles;

            Vector3 direction = cameraTransform.transform.forward;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Vector3 lookDirection = lookRotation.eulerAngles;

            rotation.y = Mathf.MoveTowardsAngle(rotation.y, lookDirection.y, deltaTime * RotationSystem.TurnSpeed);

            RotationSystem.SetRotation(rotation);
        }
    }
}