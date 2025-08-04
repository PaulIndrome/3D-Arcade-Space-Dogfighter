using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions {

    [Category("Soulspace")]
	[Description("Returns true if any game object is in the physics overlap sphere at the position of the agent and saves that agent. CAUTION!: Can be the agent if it's on the queried layer.")]
	public class GetFirstOverlapSphereObject : ConditionTask<Transform> {

		public LayerMask layerMask = -1;
        public BBParameter<float> radius = 2;
        [BlackboardOnly]
        public BBParameter<Collider> collider;

		protected override bool OnCheck() {
			var hitColliders = Physics.OverlapSphere(agent.position, radius.value, layerMask);
			if(hitColliders.Length < 1) {
				collider.value = null;
				return false;
			}

			collider.value = hitColliders[0];
			
            return true;
		}

        public override void OnDrawGizmosSelected() {
            if ( agent != null ) {
                Gizmos.color = new Color(1, 1, 1, 0.2f);
                Gizmos.DrawSphere(agent.position, radius.value);
            }
        }
	}
}