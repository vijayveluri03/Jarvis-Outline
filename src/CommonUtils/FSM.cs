using System;
using System.Collections;
using System.Collections.Generic;

namespace QUtils {
    public abstract class FSMState {
        public abstract void OnContext(System.Object context);
        public abstract void OnEnter();
        public abstract void OnExit();
        public abstract void Update();
    }

    public class FSMBaseState : FSMState {
        public override void OnContext(System.Object context) { }
        public override void Update() { }
        public override void OnEnter() { }
        public override void OnExit() { }
    }

    public class FSM {
        public int StateCount { get { return states.Count; } }

        public bool IsThereAnyStateToUpdate() {
            return states.Count > 0;
        }

        public void PushInNextFrame(FSMState state, System.Object context) {
            nextState = state;
            nextStateContext = context;
        }
        public void Pop() {
            popRequest = true;
        }

        public void Update() {
            if (nextState != null) {
                states.Push(nextState);
                nextState.OnContext(nextStateContext);
                nextState.OnEnter();

                nextState = null;
                nextStateContext = null;
            }

            if (popRequest) {
                states.Peek().OnExit();
                states.Pop();
                popRequest = false;
            }

            if (IsThereAnyStateToUpdate())
                states.Peek().Update();
        }

        // Private

        private Stack<FSMState> states = new Stack<FSMState>();
        private FSMState nextState = null;
        private System.Object nextStateContext = null;
        private bool popRequest = false;
    }
}

