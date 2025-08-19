using Partlyx.Core;

namespace Partlyx.Tests
{
    public class RecipeTest
    {
        [Fact]
        public void QuantifyAndMerge_Square_Get4Segments()
        {
            // Arrange
            var segment = new Resource("Segment");

            var triangle = new Resource("Triangle");
            triangle.CreateRecipe().CreateComponent(segment, 3);

            var square = new Resource("Square");
            var squareRecipe = square.CreateRecipe();
            squareRecipe.CreateComponent(triangle, 2);
            squareRecipe.CreateComponent(segment, -2);

            // Act
            squareRecipe.MakeQuantified();
            squareRecipe.MergeDuplicateComponents();

            // Assert
            var component = squareRecipe.Components.Single();

            Assert.Equal("Segment", component.ComponentResource.Name);

            double tolerance = 0.001;
            Assert.Equal(4, component.Quantity, tolerance);
        }

        [Fact]
        public void CloneDetached_TriangleRecipe_CreatesIndependentCopyWithSameComponent()
        {
            // Arrange
            var segment = new Resource("Segment");

            var triangle = new Resource("Triangle");
            var recipe = triangle.CreateRecipe();
            recipe.CreateComponent(segment, 3);

            // Act
            var recipeClone = recipe.CloneDetached();

            // Assert
            Assert.Single(recipe.Components);
            Assert.Single(recipeClone.Components);

            var originalComponent = recipe.Components.Single();
            var clonedComponent = recipeClone.Components.Single();

            Assert.Equal(originalComponent.Quantity, clonedComponent.Quantity);
            Assert.Equal(originalComponent.ComponentResource, clonedComponent.ComponentResource);

            Assert.NotSame(originalComponent, clonedComponent);
        }
    }
}