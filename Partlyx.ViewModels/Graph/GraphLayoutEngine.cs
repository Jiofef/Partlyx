using System.Collections.Generic;
using System.Linq;

namespace Partlyx.ViewModels.Graph
{
    public class GraphLayoutEngine
    {
        private const float StandardNodeDistanceX = 24;
        private const float StandardNodeDistanceY = 48;
        private const float StandardBranchDistanceX = StandardNodeDistanceX * 2.5f;

        public void LayoutAsTree(GraphTreeNodeViewModel rootNode)
        {
            if (rootNode == null) return;

            BuildBranchAndGetItsWidth(rootNode);
            UpdateGlobalPositionOfTree(rootNode);
        }

        public void LayoutAsRecipeGraph(GraphTreeNodeViewModel recipeNode, List<GraphTreeNodeViewModel> outputNodes, List<GraphTreeNodeViewModel> inputNodes)
        {
            if (recipeNode == null) return;

            // Position outputs above the recipe
            float currentY = recipeNode.YLocal - StandardNodeDistanceY - recipeNode.Height;
            float totalOutputsWidth = outputNodes.Sum(n => n.Width) + (outputNodes.Count - 1) * StandardNodeDistanceX;
            float outputsStartX = recipeNode.XLocal - totalOutputsWidth / 2 + recipeNode.Width / 2;

            for (int i = 0; i < outputNodes.Count; i++)
            {
                var node = outputNodes[i];
                node.SetXLocalSilent(outputsStartX + i * (node.Width + StandardNodeDistanceX));
                node.SetYLocalSilent(currentY);
            }

            // Recipe is already positioned
            // Position inputs below the recipe
            currentY = recipeNode.YLocal + recipeNode.Height + StandardNodeDistanceY;
            float totalInputsWidth = inputNodes.Sum(n => n.Width) + (inputNodes.Count - 1) * StandardNodeDistanceX;
            float inputsStartX = recipeNode.XLocal - totalInputsWidth / 2 + recipeNode.Width / 2;

            for (int i = 0; i < inputNodes.Count; i++)
            {
                var node = inputNodes[i];
                node.SetXLocalSilent(inputsStartX + i * (node.Width + StandardNodeDistanceX));
                node.SetYLocalSilent(currentY);

                // If input has children, layout them recursively
                if (node.Children.Count > 0)
                {
                    LayoutAsTree(node);
                }
            }

            UpdateGlobalPositionOfTree(recipeNode);
        }

        // This method works almost without errors, but requires rework and removal of workarounds (for example adding Width to branchOffset)
        private float BuildBranchAndGetItsWidth(GraphTreeNodeViewModel node)
        {
            if (node.Children.Count == 0)
                return node.Width;

            // Finding branch width
            float branchWidth = 0;
            float[] childrenBranchWidths = new float[node.Children.Count];

            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = (GraphTreeNodeViewModel)node.Children[i];
                float childBranchWidth = BuildBranchAndGetItsWidth(child);
                childrenBranchWidths[i] = childBranchWidth;
                branchWidth += childBranchWidth;

                if (i < node.Children.Count - 1)
                    branchWidth += child.Children.Count == 0 ? StandardNodeDistanceX : StandardBranchDistanceX;
            }

            // Setting children positions
            float branchOffset = -branchWidth / 2;

            float nextChildOffsetX = branchOffset;
            for (int i = 0; i < node.Children.Count; i++)
            {
                float currentBranchWidth = childrenBranchWidths[i];
                var child = (GraphTreeNodeViewModel)node.Children[i];
                child.SetYLocalSilent(node.Height + StandardNodeDistanceY);
                child.SetXLocalSilent(nextChildOffsetX + currentBranchWidth / 2);

                float dist = child.Children.Count == 0 ? StandardNodeDistanceX : StandardBranchDistanceX;
                nextChildOffsetX += currentBranchWidth + dist;
            }

            if (node.Children.Count == 1)
                ((GraphTreeNodeViewModel)node.Children[0]).SetXLocalSilent(0);

            return branchWidth;
        }

        private void UpdateGlobalPositionOfTree(GraphTreeNodeViewModel root)
        {
            // Since we now have multiple parents, we need to update all roots
            var roots = root.GetRoots();
            foreach (var rootNode in roots)
            {
                if (rootNode is GraphTreeNodeViewModel gtNode)
                {
                    gtNode.UpdateGlobalPositionOfTree();
                }
            }
        }
    }
}
