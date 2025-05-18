namespace MyEasyMocker
{
    public static class It
    {
        public static MockParameter<object> Any<T>()
        {
            return new MockParameter<object>(_ => true).AsObject();
        }

        public static MockParameter<object> Is<T>(Func<T, bool> predicate)
        {
            return new MockParameter<T>(predicate).AsObject();
        }
    }

    public class MockParameter<T>
    {
        private readonly Func<T, bool> _matchPredicate;
        public Type ParameterType { get; private set; }

        public MockParameter(Func<T, bool> matchPredicate)
        {
            _matchPredicate = matchPredicate;
            ParameterType = typeof(T);
        }

        public bool Matches(object value)
        {
            if (!ParameterType.IsAssignableFrom(value.GetType()))
            {
                return false;
            }

            return _matchPredicate((T)value);
        }

        public MockParameter<object> AsObject()
        {
            MockParameter<object> objectMockParameter = new MockParameter<object>(value => _matchPredicate((T)value));
            objectMockParameter.ParameterType = ParameterType;
            return objectMockParameter;
        }
    }
}
