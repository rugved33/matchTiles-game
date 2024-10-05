using UnityEngine;
using System;
using System.Collections.Generic;

namespace Scripts.Core
{
    public class RuntimeInjector : MonoBehaviour
    {
        private Injector injector;

        private void Awake()
        {
            injector = FindObjectOfType<Injector>();
            if (injector == null)
            {
                Debug.LogError("RuntimeInjector: Injector not found in the scene.");
                enabled = false;
            }
            else
            {
                // Inject dependencies into all MonoBehaviours attached to this GameObject and its children
                InjectDependenciesRecursively(transform);
            }
        }

        private void InjectDependenciesRecursively(Transform target)
        {
            foreach (var monoBehaviour in target.GetComponents<MonoBehaviour>())
            {
                // Inject dependencies into the MonoBehaviour
                InjectDependencies(monoBehaviour);
            }

            // Recursively iterate through children
            foreach (Transform child in target)
            {
                InjectDependenciesRecursively(child);
            }
        }

        /// <summary>
        /// Injects dependencies into the specified MonoBehaviour instance.
        /// </summary>
        public void InjectDependencies(MonoBehaviour monoBehaviour)
        {
            if (injector == null)
            {
                Debug.LogError("RuntimeInjector: Injector not found.");
                return;
            }

            injector.Inject(monoBehaviour);
        }

    }
}
