using Partlyx.Core;

namespace Partlyx.Tests
{
    public class RecipeTest
    {
        [Fact]
        public void Quantify_Square_Get4Segments()
        {
            // Arrange
            var segment = new Resource() {Name = "Segment"};

            var triangle = new Resource() { Name = "Triangle" };
            triangle.CreateRecipe().AddComponent(segment, 3);

            var square = new Resource() { Name = "Square" };
            var squareRecipe = square.CreateRecipe();
            squareRecipe.AddComponent(triangle, 2);
            squareRecipe.AddComponent(segment, -2);

            // Act
            var quantifiedRecype = squareRecipe.Quantify();
            Services.RecipeDebugger.DumpRecipeToConsole(quantifiedRecype);

            // Assert
            //...
        }
    }
}