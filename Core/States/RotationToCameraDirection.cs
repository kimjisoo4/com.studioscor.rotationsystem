using UnityEngine;

namespace StudioScor.RotationSystem
{
    [AddComponentMenu("StudioScor/RotationSystem/States/Rotation To Camera Direction State", order: 20)]
    public class RotationToCameraDirection : RotationState
    {
        [Header(" [ Rotation To Look Target ]")]
        [SerializeField, Min(0f)] private float _TurnSpeed = 720f;

        private Transform _CameraTransform;

        protected override void OnSetup()
        {
            base.OnSetup();

            _CameraTransform = Camera.main.transform;
        }

        public void SetTurnSpeed(float newSpeed)
        {
            _TurnSpeed = newSpeed;
        }

        public override void ProcessUpdate(float deltaTime)
        {
            Vector3 rotation = RotationSystem.transform.eulerAngles;

            Vector3 direction = _CameraTransform.transform.forward;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Vector3 lookDirection = lookRotation.eulerAngles;

            rotation.y = Mathf.MoveTowardsAngle(rotation.y, lookDirection.y, deltaTime * _TurnSpeed);

            RotationSystem.SetRotation(rotation);
        }
    }
}