using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Scripts.Core
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class InjectAttribute : PropertyAttribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProvideAttribute : PropertyAttribute { }

    public interface IDependencyProvider { }

    [DefaultExecutionOrder(-1000)]
    public class Injector : MonoBehaviour
    {
        private const BindingFlags k_bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly Dictionary<Type, object> registry = new();
        private readonly Dictionary<Type, MemberInfo[]> injectableMembersCache = new();

        private void Awake()
        {
            var monoBehaviours = FindMonoBehaviours();

            // Find all modules implementing IDependencyProvider and register the dependencies they provide
            var providers = monoBehaviours.OfType<IDependencyProvider>();
            foreach (var provider in providers)
            {
                Register(provider);
            }

            // Find all injectable objects and inject their dependencies
            var injectables = monoBehaviours.Where(IsInjectable);
            foreach (var injectable in injectables)
            {
                Inject(injectable);
            }
        }

        // Register an instance of a type outside of the normal dependency injection process
        public void Register<T>(T instance)
        {
            registry[typeof(T)] = instance;
        }

        public void Inject(object instance)
        {
            var type = instance.GetType();
            var injectableMembers = GetInjectableMembers(type);

            // Inject into fields
            foreach (var member in injectableMembers)
            {
                if (member is FieldInfo field)
                {
                    if (field.GetValue(instance) != null)
                    {
                        Debug.LogWarning($"[Injector] Field '{field.Name}' of class '{type.Name}' is already set.");
                        continue;
                    }
                    var resolvedInstance = Resolve(field.FieldType);
                    if (resolvedInstance == null)
                    {
                        throw new Exception($"Failed to inject dependency into field '{field.Name}' of class '{type.Name}'.");
                    }
                    field.SetValue(instance, resolvedInstance);
                }
                else if (member is MethodInfo method)
                {
                    var resolvedInstances = method.GetParameters().Select(p => Resolve(p.ParameterType)).ToArray();
                    if (resolvedInstances.Any(resolvedInstance => resolvedInstance == null))
                    {
                        throw new Exception($"Failed to inject dependencies into method '{method.Name}' of class '{type.Name}'.");
                    }
                    method.Invoke(instance, resolvedInstances);
                }
                else if (member is PropertyInfo property)
                {
                    var resolvedInstance = Resolve(property.PropertyType);
                    if (resolvedInstance == null)
                    {
                        throw new Exception($"Failed to inject dependency into property '{property.Name}' of class '{type.Name}'.");
                    }
                    property.SetValue(instance, resolvedInstance);
                }
            }
        }

        private void Register(IDependencyProvider provider)
        {
            var methods = provider.GetType().GetMethods(k_bindingFlags);

            foreach (var method in methods)
            {
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;

                var returnType = method.ReturnType;
                var providedInstance = method.Invoke(provider, null);
                if (providedInstance != null)
                {
                    registry.Add(returnType, providedInstance);
                }
                else
                {
                    throw new Exception($"Provider method '{method.Name}' in class '{provider.GetType().Name}' returned null when providing type '{returnType.Name}'.");
                }
            }
        }

        public void ValidateDependencies()
        {
            var monoBehaviours = FindMonoBehaviours();
            var providers = monoBehaviours.OfType<IDependencyProvider>();
            var providedDependencies = GetProvidedDependencies(providers);

            var invalidDependencies = monoBehaviours
                .SelectMany(mb => mb.GetType().GetFields(k_bindingFlags), (mb, field) => new { mb, field })
                .Where(t => Attribute.IsDefined(t.field, typeof(InjectAttribute)))
                .Where(t => !providedDependencies.Contains(t.field.FieldType) && t.field.GetValue(t.mb) == null)
                .Select(t => $"[Validation] {t.mb.GetType().Name} is missing dependency {t.field.FieldType.Name} on GameObject {t.mb.gameObject.name}");

            var invalidDependencyList = invalidDependencies.ToList();

            if (!invalidDependencyList.Any())
            {
                Debug.Log("[Validation] All dependencies are valid.");
            }
            else
            {
                Debug.LogError($"[Validation] {invalidDependencyList.Count} dependencies are invalid:");
                foreach (var invalidDependency in invalidDependencyList)
                {
                    Debug.LogError(invalidDependency);
                }
            }
        }

        private HashSet<Type> GetProvidedDependencies(IEnumerable<IDependencyProvider> providers)
        {
            var providedDependencies = new HashSet<Type>();
            foreach (var provider in providers)
            {
                var methods = provider.GetType().GetMethods(k_bindingFlags);

                foreach (var method in methods)
                {
                    if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;

                    var returnType = method.ReturnType;
                    providedDependencies.Add(returnType);
                }
            }

            return providedDependencies;
        }

        public void ClearDependencies()
        {
            foreach (var monoBehaviour in FindMonoBehaviours())
            {
                var type = monoBehaviour.GetType();
                var injectableFields = type.GetFields(k_bindingFlags)
                    .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

                foreach (var injectableField in injectableFields)
                {
                    injectableField.SetValue(monoBehaviour, null);
                }
            }

            Debug.Log("[Injector] All injectable fields cleared.");
        }

        private object Resolve(Type type)
        {
            registry.TryGetValue(type, out var resolvedInstance);
            return resolvedInstance;
        }

        private static MonoBehaviour[] FindMonoBehaviours()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);
        }

        private static bool IsInjectable(MonoBehaviour obj)
        {
            var members = obj.GetType().GetMembers(k_bindingFlags);
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }

        private MemberInfo[] GetInjectableMembers(Type type)
        {
            if (!injectableMembersCache.TryGetValue(type, out var members))
            {
                members = type.GetMembers(k_bindingFlags)
                            .Where(m => Attribute.IsDefined(m, typeof(InjectAttribute)))
                            .ToArray();
                injectableMembersCache[type] = members;
            }
            return members;
        }

    }
}