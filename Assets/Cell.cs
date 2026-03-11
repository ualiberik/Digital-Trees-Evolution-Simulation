using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Cell
{
    public Vector3 position;
    public Vector3 size;
    public Color color;
    public Simulation simulation;
    public Tree tree;
    public int cellType;
    public int activeGene;

    public void Execute()
    {
        if (cellType == 1)
        {
            ProduceEnergy();
            color = tree.leafColor;
        }
        if (cellType == 2)
        {
            if (position.y > 0)
                Fall();
            else
                BecomeStemCell();
        }
        if (cellType == 0)
        {
            if (activeGene < tree.genome.GetLength(0)) Grow();
            cellType = 1;
        }
    }    

    void Grow()
    {
        for(int i = 0; i < tree.genome.GetLength(1); i++)
        {
            Vector3 newCellPosition = Vector3.zero;
            bool isOccupied = false;
            switch(i)
            {
                case 0: newCellPosition = WrapPosition(position + Vector3.up); break;
                case 1: newCellPosition = WrapPosition(position + Vector3.forward); break;
                case 2: newCellPosition = WrapPosition(position + Vector3.right); break;
                case 3: newCellPosition = WrapPosition(position + Vector3.back); break;
                case 4: newCellPosition = WrapPosition(position + Vector3.left); break;
                case 5: newCellPosition = WrapPosition(position + Vector3.down); break;
            }
            if (newCellPosition.y < 0 || simulation.cellsMap.ContainsKey(newCellPosition))
                isOccupied = true;
            if(!isOccupied)
            {
                Cell newCell = new Cell();
                newCell.position = newCellPosition;
                newCell.size = Vector3.one;
                newCell.color = Color.gray;
                newCell.activeGene = tree.genome[activeGene, i];
                newCell.cellType = 0;
                newCell.simulation = simulation;
                newCell.tree = tree;

                Vector2 column = new Vector2(newCellPosition.x, newCellPosition.z);
                if (!simulation.columns.ContainsKey(column))
                    simulation.columns[column] = new SortedSet<float>();
                simulation.columns[column].Add(newCellPosition.y);
                simulation.cellsMap.Add(newCellPosition, newCell);
                tree.cells.Add(newCell);
                tree.energy -= 1;
            }
        }
    }

    void ProduceEnergy()
    {
        Vector2 column = new Vector2(position.x, position.z);

        if (!simulation.columns.ContainsKey(column))
            return;

        var ySet = simulation.columns[column];
        int index = 0;
        foreach (float y in ySet.Reverse())
        {
            if (y == position.y)
                break;
            index++;
        }
        if(index < 3)
        {
            int rank = 3 - index;
            tree.energy += (rank * (position.y + 3)) * simulation.lightAmount;
        }
    }

    void Fall()
    {
        Vector2 column = new Vector2(position.x, position.z);

        if (!simulation.columns.TryGetValue(column, out SortedSet<float> ySet))
            return;

        if (ySet.Contains(position.y - 1)) Die();
        else
        {
            ySet.Remove(position.y);
            simulation.cellsMap.Remove(position);
            position = new Vector3(position.x, position.y - 1, position.z);
            simulation.cellsMap.Add(position, this);
            ySet.Add(position.y);
        }       
    }

    void BecomeStemCell()
    {
        position = new Vector3(position.x, 0, position.z);
        tree.isTreeSeed = false;
        cellType = 0;
        color = Color.gray;
        size = new Vector3(1, 1, 1);
    }

    public void Die()
    {
        Vector2 columnKey = new Vector2(position.x, position.z);
        if (simulation.columns.TryGetValue(columnKey, out var column))
        {
            column.Remove(position.y);

            if (column.Count == 0)
                simulation.columns.Remove(columnKey);
        }
        simulation.cellsMap.Remove(position);
        tree.cells.Remove(this);
    }
    
    Vector3 WrapPosition(Vector3 position)
    {
        float xRange = simulation.xLimit * 2 + 1;
        float zRange = simulation.zLimit * 2 + 1;

        float x = position.x;
        float z = position.z;

        if (x > simulation.xLimit) x -= xRange;
        if (x < -simulation.xLimit) x += xRange;
        if (z > simulation.zLimit) z -= zRange;
        if (z < -simulation.zLimit) z += zRange;

        return new Vector3(x, position.y, z);
    }
}
