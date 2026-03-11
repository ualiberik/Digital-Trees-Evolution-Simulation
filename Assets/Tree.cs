using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tree
{
    public float energy;
    public int age;
    public int maxAge;
    public bool isTreeSeed;
    public Simulation simulation;
    public List<Cell> cells = new List<Cell>();
    public int[,] genome = new int[16, 6];
    public Color leafColor;

    public void Execute()
    {
        if (!isTreeSeed)
        {
            energy -= 8 * cells.Count;
            age++;
            bool hasStemCells = false;
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].cellType == 0)
                {
                    hasStemCells = true;
                    break;
                }
            }
            if (!hasStemCells)
            {
                for (int i = cells.Count - 1; i >= 0; i--) cells[i].Die();
                simulation.trees.Remove(this);
            }
        }
        else
            energy -= 2 * cells.Count;

        if (energy <= 0 || cells.Count <= 0)
        {
            for (int i = cells.Count - 1; i >= 0; i--)
            {
                cells[i].Die();
            }
            simulation.trees.Remove(this);
        }
        if(age > maxAge)
        {
            List<Cell> cellsToDestroy = new List<Cell>();
            List<Cell> seeds = new List<Cell>();

            for(int i = 0; i < cells.Count; i++)
            {
                if (cells[i].cellType != 0)
                    cellsToDestroy.Add(cells[i]);
                if (cells[i].cellType == 0)
                    seeds.Add(cells[i]);
            }

            for(int i = seeds.Count - 1; i >= 0; i--)
            {
                Tree newTree = new Tree();
                newTree.energy = energy / seeds.Count;
                if (newTree.energy > 1000) newTree.energy /= 10;
                newTree.maxAge = 45 + Random.Range(-4, 4);
                newTree.isTreeSeed = true;
                newTree.simulation = simulation;

                for (int x = 0; x < genome.GetLength(0); x++)
                {
                    for (int y = 0; y < genome.GetLength(1); y++)
                    {
                        newTree.genome[x, y] = genome[x, y];
                    }
                }
                int randomGenomeXPosition = Random.Range(0, 16);
                int randomGenomeYPosition = Random.Range(0, 6);
                newTree.genome[randomGenomeXPosition, randomGenomeYPosition] = Random.Range(0, 40); Random.Range(0, 40);


                float colorR = Mathf.Clamp(leafColor.r + Random.Range(-0.1f, 0.1f), 0f, 1f);
                float colorG = Mathf.Clamp(leafColor.g + Random.Range(-0.1f, 0.1f), 0f, 1f);
                float colorB = Mathf.Clamp(leafColor.b + Random.Range(-0.1f, 0.1f), 0f, 1f);
                newTree.leafColor = new Color(colorR, colorG, colorB, 1);

                seeds[i].cellType = 2;
                seeds[i].tree = newTree;
                seeds[i].activeGene = 0;
                seeds[i].size = new Vector3(0.6f, 0.6f, 0.6f);
                seeds[i].color = Color.yellow;

                Vector2 column = new Vector2(seeds[i].position.x, seeds[i].position.z);
                if (!simulation.columns.ContainsKey(column))
                    simulation.columns[column] = new SortedSet<float>();
                simulation.columns[column].Add(seeds[i].position.y);
                simulation.cellsMap[seeds[i].position] = seeds[i];
                newTree.cells.Add(seeds[i]);
                simulation.trees.Add(newTree);

                cells.Remove(seeds[i]);
            }
            for(int i = cellsToDestroy.Count - 1; i >= 0; i--)
            {
                cellsToDestroy[i].Die();
            }
            simulation.trees.Remove(this);
        }
    }
}
