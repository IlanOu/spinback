using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.UIElements;


namespace UserInterfaceGridLayout
{
    public class FlexibleGridLayout : LayoutGroup
    {

        #region ENUM CLASSES
        // Enum for the different types of fitting
        public enum FitType
        {
            Uniform,
            Width,
            Height,
            FixedRows,
            FixedColumns
        }

        // Enum to define what shall be filled first
        public enum SortEnum
        {
            Rows,
            Columns
        }

        // Enum for vertical sorting
        public enum SortVerticalyEnum
        {
            TopToBottom,
            BottomToTop
        }

        // Enum for horizontal sorting
        public enum SortHorizontalyEnum
        {
            LeftToRight,
            RightToLeft
        }
        #endregion
        

        // Serialized fields for the Inspector
        [Header("GRID SETTING")]
        public FitType fitType = FitType.Uniform;               // The type of fitting
        public int rows;                                        // The number of rows
        public int columns;                                     // The number of columns
        public Vector2 cellSize;                                // The size of each cell
        public Vector2 spacing;                                 // The spacing between cells

        [Header("CELL SETTINGS")]
        public bool fitX;                   // Booleans to determine if the grid should fit in the X and Y directions
        public bool fitY;
        public bool keepCellsSquare;                            // Boolean to determine if cells should be kept square

        [Header("SORTING SETTINGS")]
        public SortEnum fillFirst = SortEnum.Rows;                                  // Choose what shall be filled first
        public SortVerticalyEnum sortVertically = SortVerticalyEnum.TopToBottom;     // Vertical sorting of cells
        public SortHorizontalyEnum sortHorizontally = SortHorizontalyEnum.LeftToRight;   // Horizontal sorting of cells


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            // Calculate the number of rows and columns based on the fit type
            if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
            {
                float squareRoot = Mathf.Sqrt(transform.childCount);
                rows = columns = Mathf.CeilToInt(squareRoot);
            }

            if (fitType == FitType.Width || fitType == FitType.FixedColumns)
            {
                rows = Mathf.CeilToInt(transform.childCount / (float)columns);
            }

            if (fitType == FitType.Height || fitType == FitType.FixedRows)
            {
                columns = Mathf.CeilToInt(transform.childCount / (float)rows);
            }

            // Calculate the parent's width and height, subtracting the padding
            float parentWidth = rectTransform.rect.width - padding.left - padding.right;
            float parentHeight = rectTransform.rect.height - padding.top - padding.bottom;

            // Calculate the cell's width and height
            float cellWidth = parentWidth / (float)columns - ((spacing.x / (float)columns) * (columns - 1));
            float cellHeight = parentHeight / (float)rows - ((spacing.y / (float)rows) * (rows - 1));

            // If fitX or fitY is true, set the cell's width or height to the calculated width or height
            cellSize.x = fitX ? cellWidth : cellSize.x;
            cellSize.y = fitY ? cellHeight : cellSize.y;

            // If keepCellsSquare is true, set the cell's height to its width
            if (keepCellsSquare)
            {
                cellSize.y = cellSize.x;
            }

            // Sort and position children based on the selected sort type
            SortAndPositionChildren();
        }

        // Method to sort the children based on the selected sort type and position them in the grid
        private void SortAndPositionChildren()
        {
            // Create a list to hold the sorted children
            List<RectTransform> sortedChildren = new List<RectTransform>(rectChildren.Count);

            if (fillFirst == SortEnum.Rows)
            {
                // Sort by rows
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < columns; col++)
                    {
                        int rowIndex = sortVertically == SortVerticalyEnum.TopToBottom ? row : rows - 1 - row;
                        int colIndex = sortHorizontally == SortHorizontalyEnum.LeftToRight ? col : columns - 1 - col;
                        int index = rowIndex * columns + colIndex;

                        if (index < rectChildren.Count)
                        {
                            sortedChildren.Add(rectChildren[index]);
                        }
                    }
                }
            }
            else // fillFirst == SortEnum.Columns
            {
                // Sort by columns
                for (int col = 0; col < columns; col++)
                {
                    for (int row = 0; row < rows; row++)
                    {
                        int colIndex = sortHorizontally == SortHorizontalyEnum.LeftToRight ? col : columns - 1 - col;
                        int rowIndex = sortVertically == SortVerticalyEnum.TopToBottom ? row : rows - 1 - row;
                        int index = rowIndex + colIndex * rows;

                        if (index < rectChildren.Count)
                        {
                            sortedChildren.Add(rectChildren[index]);
                        }
                    }
                }
            }

            // Now, position the sorted children
            for (int i = 0; i < sortedChildren.Count; i++)
            {
                int rowCount, columnCount;

                if (fillFirst == SortEnum.Rows)
                {
                    rowCount = i / columns;
                    columnCount = i % columns;
                }
                else // fillFirst == SortEnum.Columns
                {
                    columnCount = i / rows;
                    rowCount = i % rows;
                }

                var item = sortedChildren[i];

                // Correct padding and position calculations
                var xPos = padding.left + (cellSize.x * columnCount) + (spacing.x * columnCount);
                var yPos = padding.top + (cellSize.y * rowCount) + (spacing.y * rowCount);

                // Adjust position based on Child Alignment, respecting padding
                if (childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.MiddleLeft || childAlignment == TextAnchor.MiddleRight)
                {
                    // Vertical centering adjustment with padding respected
                    yPos = padding.top + (rectTransform.rect.height - padding.top - padding.bottom - (rows * cellSize.y + (rows - 1) * spacing.y)) / 2
                            + (cellSize.y + spacing.y) * rowCount;
                }
                else if (childAlignment == TextAnchor.LowerCenter || childAlignment == TextAnchor.LowerLeft || childAlignment == TextAnchor.LowerRight)
                {
                    // Bottom alignment adjustment with padding respected
                    yPos = rectTransform.rect.height - padding.bottom - (rows * cellSize.y + (rows - 1) * spacing.y)
                            + (cellSize.y + spacing.y) * rowCount;
                }

                if (childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.UpperCenter || childAlignment == TextAnchor.LowerCenter)
                {
                    // Horizontal centering adjustment with padding respected
                    xPos = padding.left + (rectTransform.rect.width - padding.left - padding.right - (columns * cellSize.x + (columns - 1) * spacing.x)) / 2
                            + (cellSize.x + spacing.x) * columnCount;
                }
                else if (childAlignment == TextAnchor.MiddleRight || childAlignment == TextAnchor.UpperRight || childAlignment == TextAnchor.LowerRight)
                {
                    // Right alignment adjustment with padding respected
                    xPos = rectTransform.rect.width - padding.right - (columns * cellSize.x + (columns - 1) * spacing.x)
                            + (cellSize.x + spacing.x) * columnCount;
                }

                SetChildAlongAxis(item, 0, xPos, cellSize.x);
                SetChildAlongAxis(item, 1, yPos, cellSize.y);
            }

        }

        // These methods are required by the LayoutGroup base class, but are not used in this script
        public override void CalculateLayoutInputVertical()
        {
        }

        public override void SetLayoutHorizontal()
        {
        }

        public override void SetLayoutVertical()
        {
        }
    }
}

