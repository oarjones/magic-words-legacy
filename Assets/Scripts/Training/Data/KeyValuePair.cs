using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [Serializable]
    public class SoundsKeyValuePair
    {
        public SoundTypes key;
        public AudioClip val;
    }
}
