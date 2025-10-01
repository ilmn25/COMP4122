using System.Collections;
using UnityEngine;

namespace Resources.Scripts.Utility
{
    // https://discussions.unity.com/t/a-more-flexible-coroutine-interface/448437
    // A CoroutineTask object represents a coroutine.  CoroutineTasks can be started, paused, and stopped.
    // It is an error to attempt to start a task that has been stopped or which has
    // naturally terminated.
    public class CoroutineTask
    {
        // Returns true if and only if the coroutine is running.  Paused tasks
        // are considered to be running.
        public bool Running {
            get {
                return coroutineState.Running;
            }
        }
    
        // Returns true if and only if the coroutine is currently paused.
        public bool Paused {
            get {
                return coroutineState.Paused;
            }
        }
    
        // Delegate for termination subscribers.  manual is true if and only if
        // the coroutine was stopped with an explicit call to Stop().
        public delegate void FinishedHandler(bool manual);
    
        // Termination event.  Triggered when the coroutine completes execution.
        public event FinishedHandler Finished;

        // Creates a new CoroutineTask object for the given coroutine.
        //
        // If autoStart is true (default) the task is automatically started
        // upon construction.
        public CoroutineTask(IEnumerator c, bool autoStart = true)
        {
            coroutineState = CoroutineManager.CreateCoroutine(c);
            coroutineState.Finished += CoroutineFinished;
            if(autoStart)
                Start();
        }
    
        // Begins execution of the coroutine
        public void Start()
        {
            coroutineState.Start();
        }

        // Discontinues execution of the coroutine at its next yield.
        public void Stop()
        {
            coroutineState.Stop();
        }
    
        public void Pause()
        {
            coroutineState.Pause();
        }
    
        public void Unpause()
        {
            coroutineState.Unpause();
        }
    
        void CoroutineFinished(bool manual)
        {
            FinishedHandler handler = Finished;
            if(handler != null)
                handler(manual);
        }
    
        CoroutineManager.CoroutineState coroutineState;
    }

    class CoroutineManager : MonoBehaviour
    {
        public class CoroutineState
        {
            public bool Running {
                get {
                    return running;
                }
            }

            public bool Paused  {
                get {
                    return paused;
                }
            }

            public delegate void FinishedHandler(bool manual);
            public event FinishedHandler Finished;

            IEnumerator coroutine;
            bool running;
            bool paused;
            bool stopped;
        
            public CoroutineState(IEnumerator c)
            {
                coroutine = c;
            }
        
            public void Pause()
            {
                paused = true;
            }
        
            public void Unpause()
            {
                paused = false;
            }
        
            public void Start()
            {
                running = true;
                singleton.StartCoroutine(CallWrapper());
            }
        
            public void Stop()
            {
                stopped = true;
                running = false;
            }
        
            IEnumerator CallWrapper()
            {
                yield return null;
                IEnumerator e = coroutine;
                while(running) {
                    if(paused)
                        yield return null;
                    else {
                        if(e != null && e.MoveNext()) {
                            yield return e.Current;
                        }
                        else {
                            running = false;
                        }
                    }
                }
            
                FinishedHandler handler = Finished;
                if(handler != null)
                    handler(stopped);
            }
        }

        static CoroutineManager singleton;

        public static CoroutineState CreateCoroutine(IEnumerator coroutine)
        {
            if(!singleton) {
                GameObject go = new GameObject("CoroutineManager");
                singleton = go.AddComponent<CoroutineManager>();
            }
            return new CoroutineState(coroutine);
        }
    }
}