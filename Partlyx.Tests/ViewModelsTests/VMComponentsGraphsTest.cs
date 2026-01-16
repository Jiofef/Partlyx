using Moq;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.Graph.PartsGraph;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Collections.Generic;
using Xunit;

namespace Partlyx.Tests.ViewModelsTests
{
    public class VMComponentsGraphsTest
    {
        [Fact]
        public void Quantify_CalculatesCorrectly()
        {
            // Arrange - create mock components with quantities
            var mockResource1 = new Mock<ResourceViewModel>();
            var mockResource2 = new Mock<ResourceViewModel>();

            var mockComp1 = new Mock<RecipeComponentViewModel>();
            mockComp1.Setup(c => c.Quantity).Returns(1.0);
            mockComp1.Setup(c => c.IsOutput).Returns(false);
            mockComp1.Setup(c => c.Resource).Returns(mockResource1.Object);

            var mockComp2 = new Mock<RecipeComponentViewModel>();
            mockComp2.Setup(c => c.Quantity).Returns(2.0);
            mockComp2.Setup(c => c.IsOutput).Returns(true);
            mockComp2.Setup(c => c.Resource).Returns(mockResource2.Object);

            var components = new List<RecipeComponentViewModel> { mockComp1.Object, mockComp2.Object };
            var path = RecipeComponentPath.FromList(components);

            // Act
            var result = path.Quantify(10);

            // Assert - check that result has correct amounts
            // For input: -10 (scaled input)
            // For output: +20 (10 * 2)
            Assert.True(result.ContainsKey(mockResource1.Object));
            Assert.True(result.ContainsKey(mockResource2.Object));
            Assert.Equal(-10, result[mockResource1.Object]);
            Assert.Equal(20, result[mockResource2.Object]);
        }
    }
}
