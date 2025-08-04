using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ParadoxNotion.Services;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions {

	[Category("Soulspace")]
	public class SteerAwayFromClosestPointOnCollider : ActionTask<Rigidbody> {
		[RequiredField]
		public BBParameter<Collider> collider;
		[RequiredField]
		public BBParameter<float> safeDistance;
		public BBParameter<float> maxVelocity;
		public BBParameter<float> steeringDampening;
		public BBParameter<float> maxSteering;
		[BlackboardOnly]
		public BBParameter<Vector3> desiredVelocity, steering;

		private Vector3 closestPointOnBounds;
		private Vector3 vectorToClosestPoint;

		protected override void OnExecute() {
			MonoManager.current.onFixedUpdate += OnFixedUpdate;
		}

        private void OnFixedUpdate(){
			if(collider.value == null){
				EndAction(false);
				return;
			}

			closestPointOnBounds = collider.value.ClosestPointOnBounds(agent.transform.position);
			vectorToClosestPoint = closestPointOnBounds - agent.transform.position;

			if(vectorToClosestPoint.sqrMagnitude > safeDistance.value * safeDistance.value){
				EndAction(true);
				return;
			}

			desiredVelocity.value = (-1) * maxVelocity.value * vectorToClosestPoint;

			steering.value = Vector3.ClampMagnitude(desiredVelocity.value - agent.velocity, maxSteering.value);
        	steering.value /= steeringDampening.value;

			agent.velocity = Vector3.ClampMagnitude(agent.velocity + steering.value, maxVelocity.value);
			if(agent.velocity != Vector3.zero){
				agent.rotation = Quaternion.LookRotation(agent.velocity, agent.transform.up);
			}
		}

        protected override void OnStop()
        {
			MonoManager.current.onFixedUpdate -= OnFixedUpdate;
        }

	}
}