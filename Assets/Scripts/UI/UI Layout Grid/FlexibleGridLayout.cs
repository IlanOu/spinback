using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            FixedColumns,
            FixedRowsAndColumns // Nouveau type pour fixer à la fois les lignes et colonnes
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
        public FitType fitType = FitType.Uniform;
        public int rows;
        public int columns;
        public Vector2 cellSize;
        public Vector2 spacing;

        [Header("CELL SETTINGS")]
        public bool fitX;
        public bool fitY;
        public bool keepCellsSquare;
        
        [Header("FIXED GRID SETTINGS")]
        public bool useFixedCellSize = false; // Nouvelle option pour utiliser une taille fixe
        public Vector2 fixedCellSize = new Vector2(100, 100); // Taille fixe des cellules

        [Header("SORTING SETTINGS")]
        public SortEnum fillFirst = SortEnum.Rows;
        public SortVerticalyEnum sortVertically = SortVerticalyEnum.TopToBottom;
        public SortHorizontalyEnum sortHorizontally = SortHorizontalyEnum.LeftToRight;

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            // Si on utilise une taille fixe, on ignore les calculs de taille adaptative
            if (useFixedCellSize)
            {
                cellSize = fixedCellSize;
                
                // Si on a choisi FixedRowsAndColumns, on utilise les valeurs définies
                if (fitType == FitType.FixedRowsAndColumns)
                {
                    // Garder les valeurs de rows et columns telles quelles
                }
                else
                {
                    // Calculer le nombre de lignes et colonnes en fonction du type de fit
                    CalculateRowsAndColumns();
                }
            }
            else
            {
                // Calculer le nombre de lignes et colonnes en fonction du type de fit
                CalculateRowsAndColumns();
                
                // Calculer la taille des cellules en fonction de l'espace disponible
                CalculateCellSize();
            }

            // Sort and position children based on the selected sort type
            SortAndPositionChildren();
        }
        
        private void CalculateRowsAndColumns()
        {
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
            
            // Pour FixedRowsAndColumns, on garde les valeurs définies dans l'inspecteur
        }
        
        private void CalculateCellSize()
        {
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
        public override void CalculateLayoutInputVertical() { }
        public override void SetLayoutHorizontal() { }
        public override void SetLayoutVertical() { }
    }
}
