using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Spine;
using UnityEngine.UI;
using Spine.Unity;

namespace Dragonling.Concepts {

    public class AnimatedEntity : MonoBehaviour {
        public Spine.AnimationState AnimationState;
        public SkeletonAnimation SkeletonAnimation;

        protected TrackEntry CurrectTrackEntry(AnimTrack track) {
            return AnimationState.GetCurrent((int)track);
        }
        protected Spine.Animation GetCurrentAnim(AnimTrack track) {
            if (CurrectTrackEntry(track) == null)
                return null;
            return CurrectTrackEntry(track).Animation;
        }
        protected TrackEntry SetAnim(AnimTrack track, string name, bool loop) {
            return AnimationState.SetAnimation((int)track, name, loop);
        }
        protected TrackEntry SetAnimEmpty(AnimTrack track, float mixDuration) {
            return AnimationState.SetEmptyAnimation((int)track, mixDuration);
        }
        protected TrackEntry AddAnim(AnimTrack track, string name, bool loop, float delay = -1) {
            return AnimationState.AddAnimation((int)track, name, loop, delay != -1 ? delay : (CurrectTrackEntry(track) != null ? CurrectTrackEntry(track).MixDuration : 0F));
        }
        protected TrackEntry AddAnimEmpty(AnimTrack track, float mixDuration, float delay = -1) {
            return AnimationState.AddEmptyAnimation((int)track, mixDuration, delay != -1 ? delay : (CurrectTrackEntry(track) != null ? CurrectTrackEntry(track).MixDuration : 0F));
        }
        protected void ClearAnim(AnimTrack track) {
            AnimationState.ClearTrack((int)track);
        }

        protected enum AnimTrack {
            Movement, Attack, Special
        }
        abstract protected class Anim { };
    }

}