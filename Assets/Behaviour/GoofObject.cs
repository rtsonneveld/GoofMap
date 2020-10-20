using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Behaviour {
    
    public class GoofObject : MonoBehaviour{

        public Bounds bounds;

        public void OnEnable()
        {
            bounds = new Bounds(transform.position, Vector3.one);
        }
    }
}
