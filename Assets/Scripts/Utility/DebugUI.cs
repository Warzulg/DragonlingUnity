using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine;

namespace Dragonling.Utility {

    public class DebugUI : MonoBehaviour {
        public Text debug_animblock;
        public Text debug_idlestart;
        public Text debug_idlestop;

        void Start() {
            debug_animblock = GetComponentsInChildren<Text>().Where((t) => t.text == "ANIM BLOCK").First();
            debug_idlestart = GetComponentsInChildren<Text>().Where((t) => t.text == "IDLE START").First();
            debug_idlestop = GetComponentsInChildren<Text>().Where((t) => t.text == "IDLE STOP").First();
        }

        void Update() {
            debug_animblock.color = Color.white;
            debug_idlestart.color = Color.white;
            debug_idlestop.color = Color.white;
        }

        public void Flash_AnimBlock() {
            debug_animblock.color = Color.red;
        }
        public void Flash_IdleStart() {
            debug_idlestart.color = Color.red;
        }
        public void Flash_IdleStop() {
            debug_idlestop.color = Color.red;
        }
    }

}