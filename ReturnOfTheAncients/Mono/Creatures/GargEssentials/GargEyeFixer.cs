﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RotA.Mono.Creatures.GargEssentials
{
    public class GargEyeFixer : MonoBehaviour
    {
        private Vector3 overrideScale = new Vector3(0.95f, 0.95f, 0.95f);

        void LateUpdate()
        {
            transform.localScale = overrideScale;
        }
    }
}
