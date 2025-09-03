using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor.Utils
{
    [InitializeOnLoad]
    public static class StaticCoroutine
    {
        static StaticCoroutine()
        {
            EditorApplication.update += Update;
        }
        
        private class CoroutineHost : MonoBehaviour {}
        
        private static CoroutineHost instance;
        private static List<IEnumerator> runningRoutines;

        internal static MonoBehaviour Host
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject("Static Coroutine Host").AddComponent<CoroutineHost>();
                    instance.hideFlags = HideFlags.HideAndDontSave;
                }

                return instance;
            }
        }

        internal static Coroutine StartCoroutine(IEnumerator routine)
        {
            runningRoutines ??= new List<IEnumerator>();
            Coroutine rout = Host.StartCoroutine(routine);
            runningRoutines.Add(routine);
            return rout;
        }
        
        internal static void StopCoroutine(Coroutine routine)
        {
            if (routine != null)
            {
                Host.StopCoroutine(routine);
            }
        }

        private static void Update()
        {
            if(runningRoutines == null || runningRoutines.Count == 0)
                return;
            
            List<IEnumerator> persistingRoutines = new List<IEnumerator>();
            for (int i = 0; i < runningRoutines.Count; i++)
            {
                if (runningRoutines[i] != null)
                {
                    if(runningRoutines[i].MoveNext())
                        persistingRoutines.Add(runningRoutines[i]);
                }
            }
            runningRoutines.Clear();
            runningRoutines = persistingRoutines;
        }
    }
}