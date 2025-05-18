using NUnit.Framework;
using MyEasyMocker;
using System;

namespace Tests
{
    public interface ITestInterface
    {
        string GetValue();
        int GetNumber();
        void DoSomething();
        string GetWithParam(string param);
        int Add(int a, int b);
        string GetWithNullableParam(string? param);
    }

    public class TestImplementation : ITestInterface
    {
        public void DoSomething()
        {
            // Implementation doesn't matter for mocking
        }

        public int GetNumber()
        {
            return 42; // Implementation doesn't matter for mocking
        }

        public string GetValue()
        {
            return "original"; // Implementation doesn't matter for mocking
        }
        
        public string GetWithParam(string param)
        {
            return param; // Implementation doesn't matter for mocking
        }
        
        public int Add(int a, int b)
        {
            return a + b; // Implementation doesn't matter for mocking
        }
        
        public string GetWithNullableParam(string? param)
        {
            return param ?? "null"; // Implementation doesn't matter for mocking
        }
    }

    [TestFixture]
    public class MockTests
    {
        private Mock<ITestInterface> mock;
        private ITestInterface implementation;

        [SetUp]
        public void Setup()
        {
            implementation = new TestImplementation();
            mock = new Mock<ITestInterface>(implementation);
        }

        [Test]
        public void Setup_ValidMethodAndReturnValue_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => mock.Setup("GetValue", "mocked value"));
        }

        [Test]
        public void Setup_NullMethodName_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => mock.Setup(null, "value"));
            Assert.That(ex.Message, Is.EqualTo("Method name cannot be null or empty"));
        }

        [Test]
        public void Setup_EmptyMethodName_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => mock.Setup("", "value"));
            Assert.That(ex.Message, Is.EqualTo("Method name cannot be null or empty"));
        }

        [Test]
        public void Setup_InvalidMethodName_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => mock.Setup("NonExistingMethod", "value"));
            Assert.That(ex.Message, Is.EqualTo("Method NonExistingMethod not accessible"));
        }

        [Test]
        public void Setup_InvalidReturnType_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => mock.Setup("GetValue", 123));
            Assert.That(ex.Message, Is.EqualTo("Return type String is not assignable from Int32"));
        }

        [Test]
        public void Setup_VoidMethod_ShouldNotThrow()
        {
            // The return value doesn't matter for void methods
            Assert.DoesNotThrow(() => mock.Setup("DoSomething", null));
        }

        [Test]
        public void Call_SetupMethod_ShouldReturnMockedValue()
        {
            // Arrange
            string expectedValue = "mocked value";
            mock.Setup("GetValue", expectedValue);

            // Act
            var result = mock.Call("GetValue");

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void Call_NotSetupMethod_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => mock.Call("GetValue"));
            Assert.That(ex.Message, Is.EqualTo("Method GetValue not setup"));
        }

        [Test]
        public void Call_NotDefinedMethod_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => mock.Call("NonExistingMethod"));
            Assert.That(ex.Message, Is.EqualTo("Method NonExistingMethod not accessible"));
        }

        [Test]
        public void Call_NullMethodName_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => mock.Call(null));
            Assert.That(ex.Message, Is.EqualTo("Method name cannot be null or empty"));
        }

        [Test]
        public void Call_EmptyMethodName_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => mock.Call(""));
            Assert.That(ex.Message, Is.EqualTo("Method name cannot be null or empty"));
        }

        [Test]
        public void Call_SetupMethodMultipleTimes_ShouldIncrementCallCount()
        {
            // Arrange
            mock.Setup("GetValue", "mocked value");
            mock.Setup("GetNumber", 123);

            // Act
            mock.Call("GetValue");
            mock.Call("GetValue");
            mock.Call("GetNumber");

            // Assert
            Assert.That(mock.GetCallCount("GetValue"), Is.EqualTo(2));
            Assert.That(mock.GetCallCount("GetNumber"), Is.EqualTo(1));
        }

        [Test]
        public void Call_VoidMethod_ShouldWork()
        {
            // Arrange
            mock.Setup("DoSomething", null);

            // Act & Assert
            Assert.DoesNotThrow(() => mock.Call("DoSomething"));
            Assert.That(mock.GetCallCount("DoSomething"), Is.EqualTo(1));
        }
        
        [Test]
        public void Setup_WithParameters_ShouldNotThrow()
        {
            // Create a parameter of the right type
            var stringParam = It.Is<string>(value => value is string);
            
            // Act & Assert
            Assert.DoesNotThrow(() => mock.Setup("GetWithParam", "mocked response", new MockParameter<object>[] {stringParam.AsObject()}));
        }
        
        [Test]
        public void Setup_WithWrongParameterCount_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => mock.Setup("GetWithParam", "mocked response"));
            Assert.That(ex.Message, Is.EqualTo("Parameter count mismatch"));
        }
        
        [Test]
        public void Setup_WithWrongParameterType_ShouldThrowException()
        {
            // Arrange a parameter of the wrong type (int instead of string)
            var intParam = new MockParameter<object>(value => value is int);
            
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => 
                mock.Setup("GetWithParam", "mocked response", new MockParameter<object>[] { intParam }));
            
            Assert.That(ex.Message, Contains.Substring("Parameter (name: param, position: 0) is not assignable"));
        }
        
        [Test]
        public void Call_WithMatchingParameter_ShouldReturnMockedValue()
        {
            // Arrange
            string expectedValue = "mocked response";
            var stringParam = It.Is<string>(value => value is string);
            mock.Setup("GetWithParam", expectedValue, new MockParameter<object>[] { stringParam.AsObject() });

            // Act
            var result = mock.Call("GetWithParam", new object[] { "any string" });

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }
        
        [Test]
        public void Call_WithNonMatchingParameter_ShouldThrowException()
        {
            // Arrange - Setup with a specific parameter matcher
            mock.Setup("Add", 42, new MockParameter<object>[] { 
                It.Is<int>(x => x == 1),
                It.Is<int>(x => x == 2)
            });

            // Act & Assert - Call with non-matching parameters
            var ex = Assert.Throws<Exception>(() => mock.Call("Add", new object[] { 3, 4 }));
            Assert.That(ex.Message, Is.EqualTo("Parameter mismatch"));
        }

        [Test]
        public void GetCallCount_MethodNotSetup_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => mock.GetCallCount("GetValue"));
            Assert.That(ex.Message, Is.EqualTo("Method GetValue not setup"));
        }

        [Test]
        public void GetCallCount_NullMethodName_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => mock.GetCallCount(null));
            Assert.That(ex.Message, Is.EqualTo("Method name cannot be null or empty"));
        }

        [Test]
        public void GetCallCount_EmptyMethodName_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => mock.GetCallCount(""));
            Assert.That(ex.Message, Is.EqualTo("Method name cannot be null or empty"));
        }

        [Test]
        public void Call_WithNullParameter_ShouldHandleCorrectly()
        {
            // Arrange
            string expectedValue = "null handled";
            mock.Setup("GetWithNullableParam", expectedValue, new MockParameter<object>[] { 
                It.Is<string>(x => x == null) 
            });

            // Act
            var result = mock.Call("GetWithNullableParam", new object[] { null });

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }
    }

    [TestFixture]
    public class MockParameterTests
    {
        [Test]
        public void Matches_MatchingValue_ShouldReturnTrue()
        {
            // Arrange
            var parameter = new MockParameter<int>(x => x > 5);
            
            // Act & Assert
            Assert.That(parameter.Matches(10), Is.True);
            Assert.That(parameter.Matches(3), Is.False);
        }
        
        [Test]
        public void Matches_NonAssignableType_ShouldReturnFalse()
        {
            // Arrange
            var parameter = new MockParameter<int>(x => x > 5);
            
            // Act & Assert
            Assert.That(parameter.Matches("not an int"), Is.False);
        }
        
        [Test]
        public void AsObject_ShouldPreserveParameterType()
        {
            // Arrange
            var parameter = new MockParameter<string>(x => x.StartsWith("test"));
            
            // Act
            var objectParameter = parameter.AsObject();
            
            // Assert
            Assert.That(objectParameter.ParameterType, Is.EqualTo(typeof(string)));
        }
    }
    
    [TestFixture]
    public class ItTests
    {
        [Test]
        public void Any_ShouldMatchAnyValue()
        {
            // Arrange
            var parameter = It.Any<string>();
            
            // Act & Assert
            Assert.That(parameter.Matches("any string"), Is.True);
            Assert.That(parameter.Matches(string.Empty), Is.True);
        }
        
        [Test]
        public void Is_ShouldApplyPredicate()
        {
            // Arrange
            var parameter = It.Is<int>(x => x % 2 == 0);
            
            // Act & Assert
            Assert.That(parameter.Matches(2), Is.True);
            Assert.That(parameter.Matches(4), Is.True);
            Assert.That(parameter.Matches(3), Is.False);
        }
    }
    
    [TestFixture]
    public class MockMethodConfigurationTests
    {
        [Test]
        public void CallMatches_MatchingParameters_ShouldReturnTrue()
        {
            // Arrange
            var method = typeof(ITestInterface).GetMethod("Add");
            var config = new MockMethodConfiguration(
                3, 
                new MockParameter<object>[] {
                    It.Is<int>(x => x == 1),
                    It.Is<int>(x => x == 2)
                },
                method
            );
            
            // Act & Assert
            Assert.That(config.CallMatches(new object[] { 1, 2 }), Is.True);
            Assert.That(config.CallMatches(new object[] { 3, 4 }), Is.False);
        }
        
        [Test]
        public void CallMatches_DifferentParameterCount_ShouldReturnFalse()
        {
            // Arrange
            var method = typeof(ITestInterface).GetMethod("Add");
            var config = new MockMethodConfiguration(
                3, 
                new MockParameter<object>[] {
                    It.Is<int>(x => x == 1),
                    It.Is<int>(x => x == 2)
                },
                method
            );
            
            // Act & Assert
            Assert.That(config.CallMatches(new object[] { 1 }), Is.False);
            Assert.That(config.CallMatches(new object[] { 1, 2, 3 }), Is.False);
        }
        
        [Test]
        public void ReturnValue_ShouldBeAccessible()
        {
            // Arrange
            var method = typeof(ITestInterface).GetMethod("GetValue");
            var expectedValue = "test";
            var config = new MockMethodConfiguration(expectedValue, new MockParameter<object>[] {}, method);
            
            // Act & Assert
            Assert.That(config.ReturnValue, Is.EqualTo(expectedValue));
        }
    }
} 