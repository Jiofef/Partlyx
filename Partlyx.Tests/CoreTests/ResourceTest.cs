using Partlyx.Core;

namespace Partlyx.Tests.CoreTests
{
    public class ResourceTest
    {
        [Fact]
        public void SetDefaultRecipe_SendUncountedRecipe_GetAnException()
        {
            // Arrange
            Resource testResource = new Resource() { Name = "TestResource" };

            Resource supportResource = new Resource() { Name = "SupportResource" }; // The recipe cannot exist without a resource, so an auxiliary is required
            Recipe recipe = supportResource.CreateRecipe();

            // Act & assert
            Assert.Throws<ArgumentException>(() => testResource.SetDefaultRecipe(recipe));
        }

        [Fact]
        public void SetDefaultRecipe_SendValidRecipe_SuccessSetting()
        {
            // Arrange
            Resource testResource = new Resource() { Name = "TestResource" };
            Recipe recipe = testResource.CreateRecipe();

            // Act
            testResource.SetDefaultRecipe(recipe);

            // Assert
            Assert.Equal(recipe, testResource.ComponentDefaultRecipe);
        }
    }
}