using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ParadoxNotion.Services;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions {

	[Category("Soulspace/Steering")]
	public class AvoidLayersCondition : ConditionTask<Rigidbody> {

		public BBParameter<float> lookaheadDistance;
		public BBParameter<float> avoidanceStrength;
		public BBParameter<float> spherecastRadius;
		public BBParameter<LayerMask> avoidLayers;
		[RequiredField, BlackboardOnly]
		public BBParameter<Vector3> steering;
        public bool scaleLookaheadWithVelocity;

        public bool isAvoiding = false;
        private RaycastHit raycastHit;
        private Vector3 turnawayVector;

        protected override bool OnCheck()
        {
            isAvoiding = Physics.SphereCast(agent.position, spherecastRadius.value, agent.velocity, out raycastHit, lookaheadDistance.value * (scaleLookaheadWithVelocity ? agent.velocity.magnitude : 1), avoidLayers.value);
            if(isAvoiding){
                Debug.Log($"Is avoiding {raycastHit.collider.name}");
                // we start with direct opposite
                turnawayVector = agent.position - raycastHit.point;
                
                steering.value += turnawayVector.normalized * avoidanceStrength.value;
                steering.value.Normalize();
                return true;
            } else {
                turnawayVector = Vector3.zero;
                return false;
            }
        }

		public override void OnDrawGizmosSelected() {
            Gizmos.color = new Color(1, 0, 0, 0.66f);
            Gizmos.DrawRay(agent.position, agent.transform.forward * (lookaheadDistance.value * (scaleLookaheadWithVelocity ? agent.velocity.magnitude : 1)));
            Gizmos.color = new Color(1.0f, 0.64f, 0.0f, 0.33f);
            Gizmos.DrawRay(agent.position, agent.transform.forward * lookaheadDistance.value);
            Gizmos.color = new Color(1.0f, 0.64f, 0.0f, 0.05f);
            Gizmos.DrawSphere(agent.position, spherecastRadius.value);
        }

	}
}