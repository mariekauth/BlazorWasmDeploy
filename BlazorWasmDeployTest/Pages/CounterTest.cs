using Bunit;
using BlazorWasmDeploy.Pages;

namespace BlazorWasmDeployTest.Pages
{
    [TestClass]
    public class CounterTest : TestContextWrapper
    {
        [TestInitialize]
        public void Setup() => TestContext = new Bunit.TestContext();

        [TestCleanup]
        public void TearDown() => TestContext?.Dispose();

        [TestMethod]
        public void CounterRendersCurrentCount()
        {
            // Setup
            var cut = RenderComponent<Counter>();

            // Act
            var ele = cut.Find("p");

            // Assert
            ele?.MarkupMatches("<p role=\"status\">Current count: 0</p>");
        }

        [TestMethod]
        public void CounterRendersCurrentCountWhenClickedThreeTimes()
        {
            // Setup
            var cut = RenderComponent<Counter>();
            var button = cut.Find("button");

            // Act
            var ele = cut.Find("p");

            for (var i = 0; i < 3; i++)
            {
                button.Click();
            }

            // Assert
            ele?.MarkupMatches("<p role=\"status\">Current count: 3</p>");
        }

        [TestMethod]
        public void CounterRendersCurrentCountWhenActionIsIncrementByAndAmountIs5AndClickedThreeTimes()
        {
            // Setup
            var cut = RenderComponent<Counter>(
                parameters => parameters
                .Add(p => p.Action, "IncrementBy")
                .Add(p => p.Value, "5"));
            var button = cut.Find("button");

            // Act
            var ele = cut.Find("p");

            for (var i = 0; i < 3; i++)
            {
                button.Click();
            }

            // Assert
            ele?.MarkupMatches("<p role=\"status\">Current count: 15</p>");
        }

        [TestMethod]
        public void CounterRendersCurrentCountWhenActionIsFibonacciAndClickedThreeTimes()
        {
            // Setup
            var cut = RenderComponent<Counter>(
                parameters => parameters
                .Add(p => p.Action, "Fibonacci")
                .Add(p => p.Value, "1"));
            var button = cut.Find("button");

            // Act
            var ele = cut.Find("p");

            for (var i = 0; i < 3; i++)
            {
                button.Click();
            }

            // Assert
            ele?.MarkupMatches("<p role=\"status\">Current count: 2</p>");
        }
    }
}
