using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ParadoxNotion.Services;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions {

	[Category("Soulspace/Steering")]
	public class MoveRigidbodyBySteering : ActionTask<Rigidbody> {

        public BBParameter<float> steeringStrength;
        
        [BlackboardOnly]
        public BBParameter<float> maxVelocity;

        [RequiredField, BlackboardOnly]
		public BBParameter<Vector3> steering;

        protected override void OnExecute() {
			MonoManager.current.onFixedUpdate += OnFixedUpdate;
		}

        private void OnFixedUpdate()
        {
            Vector3 steeredVelocity = Vector3.Lerp(agent.velocity, steering.value.normalized * maxVelocity.value, steeringStrength.value * Time.fixedDeltaTime);
            agent.velocity = Vector3.ClampMagnitude(steeredVelocity, maxVelocity.value);
            agent.rotation = Quaternion.LookRotation(agent.velocity, agent.transform.up);
            EndAction(true);
        }

        protected override void OnStop()
        {
			MonoManager.current.onFixedUpdate -= OnFixedUpdate;
        }

		public override void OnDrawGizmosSelected() {
            Gizmos.color = new Color(1, 1, 1, 0.66f);
            Gizmos.DrawRay(agent.position, agent.velocity);
        }

	}
}