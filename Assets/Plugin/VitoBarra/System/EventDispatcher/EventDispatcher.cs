using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VitoBarra.System.Interaction
{
    public class EventDispatcher : MonoBehaviour
    {
        private Dictionary<Type[], Delegate> Delegates = new Dictionary<Type[], Delegate>(new TypeArrayComparer());

        public void RegisterDelegate(Action<object[]> action)
        {
            var parameterTypes = action.Method.GetParameters().Select(p => p.ParameterType).ToArray();

            if (!Delegates.ContainsKey(parameterTypes))
            {
                Delegates[parameterTypes] = action;
            }
            else
            {
                Delegates[parameterTypes] = Delegate.Combine(Delegates[parameterTypes], action);
            }
        }

        public void Invoke(params object[] parameters)
        {
            var parameterTypes = parameters.Select(p => p?.GetType()).ToArray();

            if (Delegates.TryGetValue(parameterTypes, out var action))
            {
                action?.DynamicInvoke(parameters);
            }
        }

        private class TypeArrayComparer : IEqualityComparer<Type[]>
        {
            public bool Equals(Type[] x, Type[] y)
            {
                if (x == null || y == null) return x == null && y == null;
                if (x.Length != y.Length) return false;

                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i]) return false;
                }

                return true;
            }

            public int GetHashCode(Type[] obj)
            {
                return obj.Aggregate(17, (current, type) => current * 31 + type.GetHashCode());
            }
        }
    }
}