using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace Dragonling {
    class StateManager {
        private Spine.AnimationState animationState;
        private List<State> registeredStates;
        private string idleAnimation;

        public StateManager(Spine.AnimationState animationState) {
            this.animationState = animationState;
            registeredStates = new List<State>();
        }

        public void SetIdleAnimation(string animationName) {
            idleAnimation = animationName;
        }

        public void RegisterState(
            string animationName,
            string cancelAnimation,
            int animationTrack,
            string[] cancelingAnimations,
            KeyCode key,
            KeyMode keyMode,
            AnimationMode animationMode
            ) {
            registeredStates.Add(
                new State(
                    animationName,
                    cancelAnimation,
                    animationTrack,
                    cancelingAnimations.ToList(),
                    key,
                    keyMode,
                    animationMode
                    )
                );
        }

        public void ProcessStates() {
            registeredStates.ForEach(state => {
                state.Check(registeredStates.Where(others => others != state).ToList());
                var current = animationState.GetCurrent(state.AnimationTrack);
                var currentlyPlaying = current != null && current.Animation.Name != state.AnimationName;

                if (state.IsDirty) {
                    if (state.IsActive && !currentlyPlaying) {
                        animationState.SetAnimation(
                            state.AnimationTrack,
                            state.AnimationName,
                            state.AnimationMode == AnimationMode.Looping
                            );
                        Debug.Log("<state> " + state.AnimationName + " active");
                    } else {
                        animationState.ClearTrack(state.AnimationTrack);
                        if (state.CancelAnimation != "") {
                            animationState.SetAnimation(
                                state.AnimationTrack,
                                state.CancelAnimation,
                                false
                                );
                        }
                        Debug.Log("<state> " + state.AnimationName + " cancelled");
                    }
                    state.IsDirty = false;
                }
            });

            if (registeredStates.Where(state => state.IsActive).Count() == 0) {
                animationState.SetAnimation(0, idleAnimation, true);
            }
        }
    }

    class State {
        public string AnimationName;
        public string CancelAnimation;
        public int AnimationTrack;
        public List<string> CancelingAnimations;
        public KeyCode Key;
        public KeyMode KeyMode;
        public AnimationMode AnimationMode;

        public bool IsActive;
        public bool IsDirty;

        public State(
            string animationName,
            string cancelAnimation,
            int animationTrack,
            List<string> cancelingAnimations,
            KeyCode key,
            KeyMode keyMode,
            AnimationMode animationMode
            ) {
            AnimationName = animationName;
            CancelAnimation = cancelAnimation;
            AnimationTrack = animationTrack;
            CancelingAnimations = cancelingAnimations;
            Key = key;
            KeyMode = keyMode;
            AnimationMode = animationMode;

            IsActive = false;
            IsDirty = true;
        }

        public void Check(List<State> others) {
            var willCancel = others.Where(other =>
            CancelingAnimations.Count > 0 &&
            CancelingAnimations.Where(x => x == other.AnimationName).Count() > 0
            ).Count() > 0;

            if (IsActive && willCancel) {
                IsActive = false;
                IsDirty = true;
            }
            if (!willCancel && Input.GetKeyDown(Key)) {
                IsActive = true;
                IsDirty = true;
            }
        }
    }

    public enum AnimationMode {
        Once, Looping
    }

    public enum KeyMode {
        OneShot, Hold
    }
}
