using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ParadoxNotion.Services;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions {

	[Category("Soulspace/Steering")]
	public class SteerTowardsTarget : ActionTask<Rigidbody> {

        public BBParameter<float> seekStrength;
        public BBParameter<Transform> target;

        [RequiredField, BlackboardOnly]
		public BBParameter<Vector3> steering;

        private Vector3 targetVector;

        protected override void OnExecute()
        {
            if(target != null){
                targetVector = target.value.position - agent.position;
                steering.value += targetVector.normalized * seekStrength.value;
                steering.value.Normalize();
                EndAction(true);
            } else {
                targetVector = Vector3.zero;
                EndAction(false);
            }
        }

		public override void OnDrawGizmosSelected() {
            Gizmos.color = new Color(0, 1, 0, 0.66f);
            Gizmos.DrawRay(agent.position, targetVector.normalized * seekStrength.value);
        }

	}
}