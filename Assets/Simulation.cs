using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Simulation : MonoBehaviour
{
    public Dictionary<Vector3, Cell> cellsMap = new Dictionary<Vector3, Cell>();
    public List<Tree> trees = new List<Tree>();
    public Dictionary<Vector2, SortedSet<float>> columns = new Dictionary<Vector2, SortedSet<float>>();
    public Dictionary<int, List<CellToVisualize>> cellsToVisualize = new Dictionary<int, List<CellToVisualize>>();
    public List<GameObject> cellsOnScene = new List<GameObject>();
    public Transform mainCamera;

    [SerializeField] int simulationSpeed = 1;
    [SerializeField] float visualizationStepInterval;
    [SerializeField] float visualizationStepTimer;

    [SerializeField] int seedsNumber;
    [SerializeField] GameObject cubePrefab;
    public int xLimit, zLimit;

    public float lightAmount;

    int step;
    int stepsNumber;
    int visualizationStep;
    bool isSimulationStarted;
    bool isVisualizationStarted;

    [SerializeField] GameObject simGenerationPanel;
    [SerializeField] Text simulationSpeedText;
    [SerializeField] Text stepCountText;
    [SerializeField] Text cellsCountText;
    [SerializeField] Text stepText;
    [SerializeField] Text statusText;
    [SerializeField] GameObject watchButton;
    [SerializeField] InputField stepNumberInput;
    [SerializeField] InputField stepInput;
    [SerializeField] Slider cameraSlider;

    void Start()
    {
        stepNumberInput.onEndEdit.AddListener(OnEndEditStepNumber);
        stepInput.onEndEdit.AddListener(OnEndEditStep);
    }

    void Update()
    {
        mainCamera.rotation = Quaternion.Euler(0, Mathf.Lerp(0, 360, cameraSlider.value), 0);

        if (isSimulationStarted)
        {
            statusText.text = "simulation...";

            List<Cell> currentCells = new List<Cell>(cellsMap.Values);
            foreach (Cell cell in currentCells) cell.Execute();
            List<Tree> currentTrees = new List<Tree>(trees);
            foreach (Tree tree in currentTrees) tree.Execute();

            if (!cellsToVisualize.TryGetValue(step, out var frame))
            {
                frame = new List<CellToVisualize>(currentCells.Count);
                cellsToVisualize[step] = frame;
            }
            else frame.Clear();

            foreach (var cell in currentCells)
            {
                frame.Add(new CellToVisualize
                {
                    position = cell.position,
                    size = cell.size,
                    color = cell.color
                });
            }

            if (cellsMap.Count <= 0)
            {
                statusText.text = "No Life";
                statusText.color = Color.red;
                watchButton.SetActive(true);
                isSimulationStarted = false;
            }

            step++;
            stepText.text = "Step - " + step.ToString();

            if (step >= stepsNumber)
            {
                statusText.text = "Completed succesfully";
                statusText.color = Color.green;
                watchButton.SetActive(true);
                isSimulationStarted = false;
            }
        }
        if(isVisualizationStarted)
        {
            visualizationStepTimer -= Time.deltaTime;
            if (visualizationStepTimer <= 0f)
            {
                if (!cellsToVisualize.TryGetValue(visualizationStep, out var frame))
                {
                    isVisualizationStarted = false;
                    return;
                }

                for (int i = cellsOnScene.Count - 1; i >= 0; i--) Destroy(cellsOnScene[i]);
                cellsOnScene.Clear();

                for (int i = 0; i < frame.Count; i++)
                {
                    var data = frame[i];
                    GameObject newCellObj = Instantiate(cubePrefab, data.position, Quaternion.identity);
                    newCellObj.transform.localScale = data.size;

                    var rend = newCellObj.GetComponent<MeshRenderer>();
                    var block = new MaterialPropertyBlock();
                    rend.GetPropertyBlock(block);
                    block.SetColor("_BaseColor", data.color);
                    rend.SetPropertyBlock(block);

                    cellsOnScene.Add(newCellObj);
                }

                visualizationStep++;
                stepCountText.text = "Step: " + visualizationStep.ToString();
                int numberOfCells = cellsOnScene.Count();
                cellsCountText.text = "Cells number: " + numberOfCells;
                visualizationStepTimer += visualizationStepInterval;
            }
        }
    }

    public void RestartSimulation()
    {
        cellsMap.Clear();
        columns.Clear();
        trees.Clear();
        cellsToVisualize.Clear();

        statusText.color = Color.white;

        for (int i = 0; i < seedsNumber; i++)
        {
            bool isOccupied = false;
            Vector3 newCellPosition = Vector3.zero;
            do
            {
                isOccupied = false;
                newCellPosition = new Vector3(Random.Range(-xLimit, xLimit), 0, Random.Range(-zLimit, zLimit));
                if (cellsMap.ContainsKey(newCellPosition))
                    isOccupied = true;

            } while (isOccupied);

            Tree newTree = new Tree();
            newTree.energy = 500;
            newTree.isTreeSeed = false;
            newTree.maxAge = 45 + Random.Range(-4, 4);
            newTree.simulation = this;

            for (int j = 0; j < newTree.genome.GetLength(0); j++)
            {
                for (int k = 0; k < newTree.genome.GetLength(1); k++)
                {
                    newTree.genome[j, k] = Random.Range(0, 40);
                }
            }
            newTree.leafColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);

            Cell newCell = new Cell();
            newCell.position = newCellPosition;
            newCell.size = Vector3.one;
            newCell.color = Color.gray;
            newCell.tree = newTree;
            newCell.activeGene = 0;
            newCell.cellType = 0;
            newCell.simulation = this;

            Vector2 column = new Vector2(newCellPosition.x, newCellPosition.z);
            if (!columns.ContainsKey(column))
                columns[column] = new SortedSet<float>();
            columns[column].Add(newCellPosition.y);
            cellsMap.Add(newCellPosition, newCell);
            newTree.cells.Add(newCell);

            trees.Add(newTree);
        }
        step = 0;

        isSimulationStarted = true;
        watchButton.SetActive(false);
    }

    void OnEndEditStepNumber(string value)
    {
        int.TryParse(value, out int number);
        stepsNumber = number;
    }
    void OnEndEditStep(string value)
    {
        int.TryParse(value, out int number);
        visualizationStep = number;
    }

    public void VisualizeSimulation()
    {
        simGenerationPanel.SetActive(false);
        visualizationStep = 0;
        visualizationStepTimer = 0f;
        isVisualizationStarted = true;
    }

    public void IncreaseSimulationSpeed()
    {
        if(simulationSpeed < 5)
        {
            simulationSpeed++;
            simulationSpeedText.text = "x" + simulationSpeed;
            visualizationStepInterval = 0.3f / simulationSpeed;
        }
    }

    public void DecreaseSimulationSpeed()
    {
        if (simulationSpeed > 1)
        {
            simulationSpeed--;
            simulationSpeedText.text = "x" + simulationSpeed;
            visualizationStepInterval = 0.3f / simulationSpeed;
        }
    }
}
