using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class UIevents:MonoBehaviour
    {
        public void OnResolveWord()
        {
            GameEvents.FireValidateWord(GameActor.Player);
        }

        
    }
}
