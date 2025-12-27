using Partlyx.Core.Partlyx;

namespace Partlyx.Tests.CoreTests
{
    public class ResourceTest
    {
        [Fact]
        public void SetDefaultRecipe_SendUncountedRecipe_GetAnException()
        {
            // Arrange
            Resource testResource = new Resource() { Name = "TestResource" };

            Resource supportResource = new Resource() { Name = "SupportResource" };
            Recipe recipe = Recipe.Create();
            recipe.CreateComponent(supportResource, 1); // input, not output

            // Act & assert
            Assert.Throws<ArgumentException>(() => testResource.SetDefaultRecipe(recipe));
        }

        [Fact]
        public void SetDefaultRecipe_SendValidRecipe_SuccessSetting()
        {
            // Arrange
            Resource testResource = new Resource() { Name = "TestResource" };
            Recipe recipe = Recipe.Create();
            recipe.CreateOutput(testResource, 1); // valid output

            // Act
            testResource.SetDefaultRecipe(recipe);

            // Assert
            Assert.Equal(recipe, testResource.DefaultRecipe);
        }
    }
}
