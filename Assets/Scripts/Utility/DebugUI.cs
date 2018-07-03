using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine;

namespace Dragonling.Utility {

    public class DebugUI : MonoBehaviour {
        private static Color WHITE = new Color(1, 1, 1);
        private static Color RED = new Color(1, 0, 0);

        public Text debug_animblock;
        public Text debug_idlestart;
        public Text debug_idlestop;

        void Start() {
            debug_animblock = GetComponentsInChildren<Text>().Where((t) => t.text == "ANIM BLOCK").First();
            debug_idlestart = GetComponentsInChildren<Text>().Where((t) => t.text == "IDLE START").First();
            debug_idlestop = GetComponentsInChildren<Text>().Where((t) => t.text == "IDLE STOP").First();
        }

        void Update() {
            debug_animblock.color = WHITE;
            debug_idlestart.color = WHITE;
            debug_idlestop.color = WHITE;
        }

        public void Flash_AnimBlock() {
            debug_animblock.color = RED;
        }
        public void Flash_IdleStart() {
            debug_idlestart.color = RED;
        }
        public void Flash_IdleStop() {
            debug_idlestop.color = RED;
        }
    }

}