using UnityEngine;

namespace StudioScor.RotationSystem
{
    [AddComponentMenu("StudioScor/RotationSystem/States/Rotation Stoping State", order: 0)]
    public class RotationStoping : RotationState
    {
        public override void ProcessUpdate(float deltaTime)
        {
            RotationSystem.SetRotation(RotationSystem.transform.eulerAngles);
        }
    }
}