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
        private readonly Dictionary<string, object> methodNameToReturnValue;
        private readonly Dictionary<string, int> methodNameToCallCount;

        public Mock(T mockedObject)
        {
            originalObject = mockedObject;
            methodNameToReturnValue = new Dictionary<string, object>();
            methodNameToCallCount = new Dictionary<string, int>();
        }

        public void Setup(string methodName, object returnValue)
        {
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

            methodNameToReturnValue[methodName] = returnValue;
            methodNameToCallCount[methodName] = 0;
        }

        public object Call(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new Exception("Method name cannot be null or empty");
            }

            var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var method = methods.FirstOrDefault(method => method.Name == methodName);

            if (method == null)
            {
                throw new Exception($"Method {methodName} not accessible");
            }

            if (!methodNameToReturnValue.TryGetValue(methodName, out object returnValue))
            {
                throw new Exception($"Method {methodName} not setup");
            }

            methodNameToCallCount[methodName]++;

            return returnValue;
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
