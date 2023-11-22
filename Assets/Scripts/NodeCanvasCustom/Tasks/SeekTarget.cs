using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ParadoxNotion.Services;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions {

	[Category("Soulspace")]
	public class SeekTarget : ActionTask<Rigidbody> {

		[RequiredField]
		public BBParameter<Transform> seekTarget;
		public BBParameter<float> slowingRadius, slowingRadiusOffset;
		public BBParameter<float> maxVelocity;
		public BBParameter<float> steeringDampening, maxSteering;
		
		[Space, BlackboardOnly]
		public BBParameter<Vector3> desiredVelocity, steering;

		private float distanceToTarget;
		private float arriveFactor;
		private Vector3 vectorToTarget;

        protected override void OnExecute() {
			arriveFactor = float.MaxValue;
			MonoManager.current.onFixedUpdate += OnFixedUpdate;
		}

		private void OnFixedUpdate(){
			if(!seekTarget.value){
				EndAction(false);
				return;
			}

			if(arriveFactor <= Mathf.Epsilon)
			{
				agent.velocity = Vector3.zero;
				EndAction(true);
				return;
			} 

			vectorToTarget = seekTarget.value.position - agent.position;
			distanceToTarget = vectorToTarget.magnitude;

			desiredVelocity.value = vectorToTarget.normalized * maxVelocity.value;
			arriveFactor = Mathf.InverseLerp(slowingRadiusOffset.value, slowingRadius.value, distanceToTarget);
			desiredVelocity.value *= arriveFactor;

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

		public override void OnDrawGizmosSelected() {
            if ( seekTarget != null ) {
                Gizmos.color = new Color(1, 0, 0, 0.33f);
                Gizmos.DrawLine(agent.position, seekTarget.value.position);
            }
        }

	}
}