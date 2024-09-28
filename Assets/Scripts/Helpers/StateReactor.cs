using System;
using System.Collections.Generic;
using System.ComponentModel;
using UniRx;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Game.Helpers
{
    public abstract class StateReactor<T> : MonoBehaviour where T : Enum
    {
        [Tooltip("List of States")]
        [SerializeField]
        private List<T> visibleInStates;


        protected abstract StateModel<T> Model { get; }

        private void Start()
        {
            Model.State.Subscribe(state => SetVisibility(IsVisible(state))).AddTo(this);
        }

        protected abstract void SetVisibility(bool visible);

        private bool IsVisible(T state)
        {
            if(state == null)
            {
                return false;
            }

            return visibleInStates.Contains(state);
        }
    }
}