using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fluid
{
    public class CameraFollower : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                transform.Translate(-1,0,0);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                transform.Translate(1, 0, 0);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                transform.Translate(0, 1, 0);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                transform.Translate(0, -1, 0);
            }
        }
    }
}
