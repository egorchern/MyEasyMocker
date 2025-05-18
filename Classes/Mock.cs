using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MyEasyMocker
{
    public class Mock<T>
    {
        private readonly T originalObject;
        private readonly Dictionary<string, MockMethodConfiguration> methodNameToConfiguration;
        private readonly Dictionary<string, int> methodNameToCallCount;

        public Mock(T mockedObject)
        {
            originalObject = mockedObject;
            methodNameToConfiguration = new Dictionary<string, MockMethodConfiguration>();
            methodNameToCallCount = new Dictionary<string, int>();
        }
        public void Setup(string methodName, object returnValue, MockParameter<object>[] parameters = null)
        {
            if (parameters == null)
            {
                parameters = new MockParameter<object>[] {};
            }
            
            if (string.IsNullOrEmpty(methodName))
            {
                throw new Exception("Method name cannot be null or empty");
            }

            // is method name in interface of T
            var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var method = methods.FirstOrDefault(method => method.Name == methodName);
            
            if (method == null)
            {
                throw new Exception($"Method {methodName} not accessible");
            }

            // verify return type
            Type expectedReturnType = method.ReturnType;

            if (expectedReturnType.Name != "void" && !expectedReturnType.IsAssignableFrom(returnValue.GetType()))
            {
                throw new Exception($"Return type {expectedReturnType.Name} is not assignable from {returnValue.GetType().Name}");
            }

            // verify parameters
            ParameterInfo[] methodParameters = method.GetParameters();

            if (methodParameters.Length > 0)
            {
                if (parameters == null || parameters.Length != methodParameters.Length)
                {
                    throw new Exception($"Parameter count mismatch");
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo expectedParameter = methodParameters[i];
                    MockParameter<object> actualParameter = parameters[i];

                    if (!expectedParameter.ParameterType.IsAssignableFrom(actualParameter.ParameterType))
                    {
                        throw new Exception($"Parameter (name: {expectedParameter.Name}, position: {i}) is not assignable");
                    }
                }
            }
            
            methodNameToConfiguration[methodName] = new MockMethodConfiguration(returnValue, parameters, method);
            methodNameToCallCount[methodName] = 0;
        }

        public object Call(string methodName, object[] parameters = null)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new Exception("Method name cannot be null or empty");
            }

            MethodInfo[] methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            MethodInfo? method = methods.FirstOrDefault(method => method.Name == methodName);

            if (method == null)
            {
                throw new Exception($"Method {methodName} not accessible");
            }

            if (!methodNameToConfiguration.TryGetValue(methodName, out MockMethodConfiguration? configuration))
            {
                throw new Exception($"Method {methodName} not setup");
            }

            if (parameters == null)
            {
                parameters = new object[] {};
            }

            if (!configuration.CallMatches(parameters))
            {
                throw new Exception($"Parameter mismatch");
            }

            methodNameToCallCount[methodName]++;

            return configuration.ReturnValue;
        }

        public int GetCallCount(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new Exception("Method name cannot be null or empty");
            }

            if (!methodNameToCallCount.TryGetValue(methodName, out int callCount))
            {
                throw new Exception($"Method {methodName} not setup");
            }

            return callCount;
        }
    }
}
