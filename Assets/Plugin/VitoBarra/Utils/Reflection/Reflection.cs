using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace VitoBarra.Utils.Reflection
{
    public static class Reflection
    {
        public static List<Type> GetSubclasses(this Type parentClass)
        {
            List<Type> subclasses = new List<Type>();

            // Get all loaded assemblies
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Iterate through each assembly
            foreach (Assembly assembly in assemblies)
            {
                // Get all types in the assembly
                Type[] types = assembly.GetTypes();

                // Iterate through each type
                foreach (Type type in types)
                {
                    // Check if the type is a subclass of the parent class
                    if (type.IsSubclassOf(parentClass))
                    {
                        subclasses.Add(type);
                    }
                }
            }

            return subclasses;
        }

        public static T CreateInstance<T>(Type type) where T : class
        {
            if (!typeof(T).IsAssignableFrom(type))
            {
                throw new ArgumentException("Type does not inherit from or implement " + typeof(T).Name, nameof(type));
            }

            return Activator.CreateInstance(type) as T;
        }
        
        // var methodInfo = typeof(ChipPackageSpawner).GetMethod(nameof(ChipPackageSpawner.GenerateBuiltInPackageAndChip));
        //
        // foreach (var genericMethod in typeof(BuiltinChip).GetSubclasses().Select(subclass => methodInfo.MakeGenericMethod(subclass)))
        // {
        //     genericMethod.Invoke(ChipPackageSpawner.i, null);
        // }
    }
}