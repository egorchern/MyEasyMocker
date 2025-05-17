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
    }
} 