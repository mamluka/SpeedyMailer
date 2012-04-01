using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SpeedyMailer.Core.Container
{
    public static class InterfaceWrapper
    {

        private static readonly IDictionary<string, ModuleBuilder> _builders = new Dictionary<string, ModuleBuilder>();
        private static readonly IDictionary<Type, Type> _types = new Dictionary<Type, Type>();
        private static readonly object _lockObject = new object();



        /// <summary>
        /// Creates an interface that matches the interface defined by <typeparamref name="T"/>
        /// </summary>
        public static T CreateInterface<T>(Func<T> getter, Type wrapperType)
        {
            return CreateInterfaceInstance(getter, wrapperType);
        }

        // Note that calling this method will cause any further
        // attempts to generate an interface to fail
        public static void Save()
        {
            foreach (ModuleBuilder builder in _builders.Select(b => b.Value))
            {
                var ass = (AssemblyBuilder) builder.Assembly;
                try
                {
                    ass.Save(ass.GetName().Name + ".dll");
                }
                catch
                {
                }
            }
        }



        private static T CreateInterfaceInstance<T>(Func<T> getter, Type wrapperType)
        {
            Type destType = GenerateInterfaceType(getter, wrapperType);

            return (T) Activator.CreateInstance(destType);
        }

        private static Type GenerateInterfaceType<T>(Func<T> getter, Type wrapperType)
        {

            Type sourceType = typeof (T);

            Type newType;
            if (_types.TryGetValue(sourceType, out newType))
                return newType;

            // Make sure the same interface isn't implemented twice
            lock (_lockObject)
            {
                if (_types.TryGetValue(sourceType, out newType))
                    return newType;



                if (!sourceType.IsInterface)
                    throw new ArgumentException("Type T is not an interface", "T");

                if (!wrapperType.GetInterfaces().Contains(typeof (IDisposable)))
                    throw new ArgumentException("Type must implement IDisposable.", "wrapperType");

                ConstructorInfo wrapperTypeConstructor = wrapperType.GetConstructor(new[] {typeof (object)});
                if (wrapperTypeConstructor == null)
                    throw new ArgumentException(
                        "Type must have a single constructor that takes a single object parameter.", "wrapperType");

                MethodInfo getterMethod = getter.Method;
                if ((getterMethod.Attributes & MethodAttributes.Public) != MethodAttributes.Public)
                    throw new ArgumentException("Method must be public.", "getter");



                string orginalAssemblyName = sourceType.Assembly.GetName().Name;

                ModuleBuilder moduleBuilder;
                if (!_builders.TryGetValue(orginalAssemblyName, out moduleBuilder))
                {
                    var newAssemblyName = new AssemblyName(Guid.NewGuid() + "." + orginalAssemblyName);

                    AssemblyBuilder dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
                        newAssemblyName,
                        AssemblyBuilderAccess.RunAndSave);

                    moduleBuilder = dynamicAssembly.DefineDynamicModule(
                        newAssemblyName.Name,
                        newAssemblyName + ".dll");

                    _builders.Add(orginalAssemblyName, moduleBuilder);
                }

                AssemblyName assemblyName = moduleBuilder.Assembly.GetName();



                TypeBuilder typeBuilder = moduleBuilder.DefineType(
                    sourceType.FullName,
                    TypeAttributes.Public | TypeAttributes.Class,
                    typeof (object),
                    new[] {sourceType});



                var interfaces = new List<Type>();
                IEnumerable<Type> subList;

                subList = new[] {sourceType};

                while (subList.Count() != 0)
                {
                    interfaces.AddRange(subList);
                    subList = subList.SelectMany(i => i.GetInterfaces());
                }

                interfaces = interfaces.Distinct().ToList();



                foreach (MethodInfo method in interfaces.SelectMany(i => i.GetMethods()))
                {
                    // Define the method based on the interfaces definition
                    MethodBuilder newMethod = typeBuilder.DefineMethod(
                        method.Name,
                        method.Attributes ^ MethodAttributes.Abstract,
                        method.CallingConvention,
                        method.ReturnType,
                        method.ReturnParameter.GetRequiredCustomModifiers(),
                        method.ReturnParameter.GetOptionalCustomModifiers(),
                        method.GetParameters().Select(p => p.ParameterType).ToArray(),
                        method.GetParameters().Select(p => p.GetRequiredCustomModifiers()).ToArray(),
                        method.GetParameters().Select(p => p.GetOptionalCustomModifiers()).ToArray()
                        );

                    // Check to see if we have a return type
                    bool hasReturnValue = method.ReturnType != typeof (void);

                    ILGenerator methodBody = newMethod.GetILGenerator();

                    // sourceType var0;
                    // wrapperType var1;
                    methodBody.DeclareLocal(sourceType);
                    methodBody.DeclareLocal(wrapperType);

                    // returnType var2;
                    if (hasReturnValue)
                        methodBody.DeclareLocal(method.ReturnType);

                    // var0 = getter();
                    methodBody.Emit(OpCodes.Call, getterMethod);
                    methodBody.Emit(OpCodes.Stloc_0);

                    // var1 = new wrapperType(var0);
                    methodBody.Emit(OpCodes.Ldloc_0);
                    methodBody.Emit(OpCodes.Newobj, wrapperTypeConstructor);
                    methodBody.Emit(OpCodes.Stloc_1);

                    // using (var1) {
                    methodBody.BeginExceptionBlock();

                    // (load the object to call the method on)
                    methodBody.Emit(OpCodes.Ldloc_0);

                    // (load any parameters)
                    for (int i = 1; i <= method.GetParameters().Length; ++i)
                        methodBody.Emit(OpCodes.Ldarg, i);

                    // var2 = var0.method(...);
                    methodBody.Emit(OpCodes.Callvirt, method);

                    if (hasReturnValue)
                        methodBody.Emit(OpCodes.Stloc_2);

                    // } (end using)
                    methodBody.BeginFinallyBlock();

                    methodBody.Emit(OpCodes.Ldloc_1);
                    methodBody.Emit(OpCodes.Callvirt, typeof (IDisposable).GetMethod("Dispose"));

                    methodBody.EndExceptionBlock();

                    // return var2;
                    if (hasReturnValue)
                        methodBody.Emit(OpCodes.Ldloc_2);

                    // return;
                    methodBody.Emit(OpCodes.Ret);
                }



                newType = typeBuilder.CreateType();

                _types.Add(sourceType, newType);

                return newType;
            }

        }

    }
}