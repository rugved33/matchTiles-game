using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Unity.VisualScripting;

namespace Game.Helpers
{
    public abstract class StateModel<T> : DisposableEntity where T : Enum
    {
        public ReactiveProperty<T> State { get;  private set; }

        public StateModel(T state)
        {
            State = new ReactiveProperty<T>(state);
        }
    }
}