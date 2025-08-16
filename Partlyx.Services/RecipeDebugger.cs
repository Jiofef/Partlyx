namespace Partlyx.Services
{
    public class RecipeDebugger
    {
        public static void DumpRecipeToConsole(Core.Recipe recipe)
        {
            string toPrint = $"Here is {recipe.ParentResource.Name} recipe components:";

            foreach (var component in recipe.Components)
            {
                toPrint += $"{component.ComponentResource.Name} - {component.Quantity}";
            }
            Console.WriteLine(toPrint);
        }
    }
}
