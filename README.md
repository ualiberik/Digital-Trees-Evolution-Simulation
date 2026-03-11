# Digital Trees Evolution Simulation
## Introduction
**This project was inspired by foo52's (Simulife Hub — his english chanell) [video](https://youtu.be/IL1HogOu5B0) and takes .**
"Digital Trees Evolution Simulation" project is the 3D simulation of natural selection and evolution that shows how organisms can develop and become more complex through time. Simulation can visualize how multicellular "trees", fighting for a place in the sun, can find the most effective solutions.
Trees pass on the genome by inheritance. However, it can slightly change randomly, laucnhing changes in population and natural selection.
## How does simulation work?
All simulation's active objects are divided into two levels: trees and cells. One tree consists of large amount of cells and cells make up the tree. The cells occupy clear positions in space and fill the 3D grid.
### Trees
Tree — group of cells working together. Trees have parameters such as energy and age. Losing all energy or exceeding the life span, tree dies.
### Cells
Cells are functional elementary units of a tree. Each cell consumes energy and performs a function for a tree.
#### Stem cells
Stem cells perform commands of genome. Each stem cell stores a number of active gene that has to be performed. Gene's command is spawning new stem cells in certain positions around performing stem cell and storing active gene to perform in each new cell. Thus, tree can grow and genome can controll how it will do it. After performing gene's command, stem cell becomes leaf cell. If tree dies of old age, stem cells become seeds.
#### Leaf cells
Leaf cells function — produce energy for tree "by photosynthesis". How much energy does leaf cell produce is determined by how much light does it get, which is determined by "laws of lighting".
#### Seeds
All stem cells become seeds when tree dies of old age. Seeds store genetic information and each seed slightly changes it in start of its life. Seeds fall constantly and become stem cells when touch the ground.
### Genome
Genome is two-dimensional array, where 16 rows are genes and 6 columns are genes' parameters. Each gene store 6 integer values — active genes of new stem cells (because the cube has 6 faces and there are 6 sides to spawn new stem cell). If value is larger than 16, new stem cell doesn't spawn in the corresponding side.
### "Laws of lightning"
The higher (by y position) the leaf cell is, the more light and energy it receives. The amount of light received is also affected by the number of other cells above the considered leaf cell.
Thus, received energy is determined by formula:
``` tree.energy += (rank * (position.y + 3)) * simulation.lightAmount;
simulation.lightAmount — general coefficient of "light amount" and 3 is smoothing coefficient.

