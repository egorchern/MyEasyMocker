using System.Reflection;

namespace MyEasyMocker
{
    public class MockMethodConfiguration
    {
        private readonly object _returnValue;
        public object ReturnValue => _returnValue;
        private readonly MockParameter<object>[] _parameters;
        private readonly MethodInfo _methodInfo;

        public MockMethodConfiguration(object returnValue, MockParameter<object>[] parameters, MethodInfo methodInfo)
        {
            _returnValue = returnValue;
            _parameters = parameters;
            _methodInfo = methodInfo;
        }
        
        public bool CallMatches(object[] parameters)
        {
            if (parameters.Length != _parameters.Length)
            {
                return false;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (!_parameters[i].Matches(parameters[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
