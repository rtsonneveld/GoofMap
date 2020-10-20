using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GoofMap.Behaviour {
    public class SpeenAnimBehaviour : MonoBehaviour{

        public void Update()
        {
            transform.rotation = transform.rotation * Quaternion.Euler(0, -Time.deltaTime * 120.0f, 0);
        }
    }
}
