using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GoofMap.Behaviour {
    public class CoinBehaviour : MonoBehaviour{

        public void Update()
        {
            transform.rotation = Quaternion.Euler(0, Time.realtimeSinceStartup * 120.0f, 0);
        }
    }
}
