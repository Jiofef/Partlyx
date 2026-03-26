using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Partlyx.ViewModels.Graph.PartsGraph
{
    public enum CalculationArgumentType
    {
        Input,  // Amount is the input quantity
        Output  // Amount is the desired output quantity
    }

    public record CalculationRequest(
        double Amount,
        CalculationArgumentType ArgumentType
    );

    public record PathCalculationResult(
        Dictionary<RecipeComponentViewModel, double> StepCosts,
        Dictionary<ResourceViewModel, double> ResourceTotals
    );

    public class RecipeComponentPath : Path<RecipeComponentViewModel>
    {
        public RecipeComponentPath(LinkedList<RecipeComponentViewModel> nodes) : base(nodes)
        {
        }

        public int GetComponentsAmount() => Steps.Count;
        public int GetRecipesAmount() => Steps.Count / 2;

        public static RecipeComponentPath FromList(List<RecipeComponentViewModel> path)
        {
            var linkedList = new LinkedList<RecipeComponentViewModel>(path);
            return new RecipeComponentPath(linkedList);
        }

        public Dictionary<RecipeComponentViewModel, double> CalculateMultipliers()
        {
            var multipliers = new Dictionary<RecipeComponentViewModel, double>();
            double currentMultiplier = 1.0;

            var current = GetFirst();
            while (current != null)
            {
                multipliers[current.Value] = currentMultiplier;
                currentMultiplier *= current.Value.Quantity;
                current = GetNext(current);
            }

            return multipliers;
        }

        /// <summary>
        /// Core calculation method that returns both step costs and resource totals.
        /// Supports both Input and Output as argument types.
        /// </summary>
        public PathCalculationResult CalculatePath(
            CalculationRequest request,
            bool adjustToArgument = true)
        {
            // Determine input amount based on request type
            double inputAmount;
            double? targetOutputAmount = null;

            if (request.ArgumentType == CalculationArgumentType.Input)
            {
                inputAmount = request.Amount;
            }
            else
            {
                // Calculate required input for desired output
                inputAmount = CalculateRequiredInput(request.Amount);
                targetOutputAmount = request.Amount;
            }

            var stepCosts = new Dictionary<RecipeComponentViewModel, double>();
            var totals = new Dictionary<ResourceViewModel, double>();
            double currentFlow = inputAmount;
            var firstNode = Steps.First;
            var firstNodeResource = firstNode?.Value.Resource;

            if (firstNode == null)
                return new PathCalculationResult(stepCosts, totals);

            var currentNode = firstNode;

            while (currentNode != null && currentNode.Next != null)
            {
                var pathInputComp = currentNode.Value;
                var pathOutputComp = currentNode.Next.Value;
                var recipe = pathInputComp.ParentRecipe;

                if (recipe == null) break;

                bool isReverseStep = recipe.Outputs.Any(o => o.Uid == pathInputComp.Uid);

                double crafts = recipe.GetCraftsCount(pathInputComp.Uid, currentFlow, isReverseStep);
                double directionMultiplier = isReverseStep ? -1.0 : 1.0;

                // Store step costs for both path components
                stepCosts[pathInputComp] = pathInputComp.Quantity * crafts;
                stepCosts[pathOutputComp] = pathOutputComp.Quantity * crafts;

                // Store step costs for side components (inputs and outputs of the recipe)
                foreach (var inComp in recipe.Inputs)
                {
                    if (inComp.Resource != null)
                    {
                        stepCosts[inComp] = inComp.Quantity * crafts;
                        AddDelta(totals, inComp.Resource, -inComp.Quantity * crafts * directionMultiplier);
                    }
                }
                foreach (var outComp in recipe.Outputs)
                {
                    if (outComp.Resource != null)
                    {
                        stepCosts[outComp] = outComp.Quantity * crafts;
                        AddDelta(totals, outComp.Resource, outComp.Quantity * crafts * directionMultiplier);
                    }
                }

                currentFlow = pathOutputComp.Quantity * crafts;
                currentNode = currentNode.Next.Next;
            }

            // Cleanup small values
            const double epsilon = 1e-10;
            foreach (var key in totals.Keys.ToList())
                if (Math.Abs(totals[key]) < epsilon) totals.Remove(key);

            // Adjust to argument amount if needed
            if (adjustToArgument)
            {
                if (request.ArgumentType == CalculationArgumentType.Input && firstNodeResource != null && totals.ContainsKey(firstNodeResource))
                {
                    // Adjust to input amount
                    var resultInput = totals[firstNodeResource];
                    if (Math.Abs(inputAmount - resultInput) > epsilon && resultInput != 0)
                    {
                        var adjustCoeff = inputAmount / -resultInput;
                        foreach (var key in totals.Keys.ToList())
                            totals[key] *= adjustCoeff;
                        foreach (var key in stepCosts.Keys.ToList())
                            stepCosts[key] *= adjustCoeff;
                    }
                }
                else if (request.ArgumentType == CalculationArgumentType.Output && targetOutputAmount.HasValue)
                {
                    // Adjust to output amount
                    var lastNode = Steps.Last?.Value;
                    var lastNodeResource = lastNode?.Resource;
                    if (lastNodeResource != null && totals.ContainsKey(lastNodeResource))
                    {
                        var resultOutput = totals[lastNodeResource];
                        if (Math.Abs(targetOutputAmount.Value - resultOutput) > epsilon && resultOutput != 0)
                        {
                            var adjustCoeff = targetOutputAmount.Value / resultOutput;
                            foreach (var key in totals.Keys.ToList())
                                totals[key] *= adjustCoeff;
                            foreach (var key in stepCosts.Keys.ToList())
                                stepCosts[key] *= adjustCoeff;
                        }
                    }
                }
            }

            return new PathCalculationResult(stepCosts, totals);
        }

        public record PathQuantitifationOptions(bool AdjustResultToInputAmount = true);
        public Dictionary<ResourceViewModel, double> Quantify(double inputAmount, PathQuantitifationOptions? options = null)
        {
            var qOptions = options ?? new PathQuantitifationOptions();
            var request = new CalculationRequest(inputAmount, CalculationArgumentType.Input);
            
            var result = CalculatePath(request, adjustToArgument: qOptions.AdjustResultToInputAmount);
            return result.ResourceTotals;
        }

        public Dictionary<ResourceViewModel, double> QuantifyFromOutputAmount(double targetOutputAmount)
        {
            var request = new CalculationRequest(targetOutputAmount, CalculationArgumentType.Output);
            var result = CalculatePath(request);
            return result.ResourceTotals;
        }

        public double CalculateRequiredInput(double targetOutputAmount)
        {
            double neededFlow = targetOutputAmount;
            var currentNode = Steps.Last;

            while (currentNode != null && currentNode.Previous != null)
            {
                var pathOutputComp = currentNode.Value;
                var pathInputComp = currentNode.Previous.Value;
                var recipe = pathOutputComp.ParentRecipe;

                if (recipe != null)
                {
                    // Detect direction for the backward pass
                    bool isReverseStep = recipe.Outputs.Any(o => o.Uid == pathInputComp.Uid);

                    // Logic for needed flow also needs to be direction-aware
                    // If step is reverse (Out -> In), then pathOutputComp is an Input. 
                    // So we calculate crafts needed to get that Input amount.
                    double crafts = recipe.GetCraftsCount(pathOutputComp.Uid, neededFlow, !isReverseStep);
                    neededFlow = pathInputComp.Quantity * crafts;
                }

                currentNode = currentNode.Previous.Previous;
            }

            return neededFlow;
        }

        private void AddDelta(Dictionary<ResourceViewModel, double> dict, ResourceViewModel res, double delta)
        {
            if (dict.ContainsKey(res)) dict[res] += delta;
            else dict[res] = delta;
        }

        public IEnumerable<ResourceAmountPairViewModel> QuantifyToValuePairs(double inputAmount)
        {
            var quantified = Quantify(inputAmount);
            return quantified.Select(kvp => new ResourceAmountPairViewModel(kvp.Key, kvp.Value));
        }

        public IEnumerable<ResourceAmountPairViewModel> QuantifyFromOutputAmountToValuePairs(double outputAmount)
        {
            var quantified = QuantifyFromOutputAmount(outputAmount);
            return quantified.Select(kvp => new ResourceAmountPairViewModel(kvp.Key, kvp.Value));
        }
    }
}