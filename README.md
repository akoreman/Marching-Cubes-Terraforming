# Terraforming Prototype
WIP. Terraforming mechanism prototype based on my multi-threaded implementation of the marching cubes algorithm (https://github.com/akoreman/Marching-Cubes-Unity-Job-System). Inspired by the gameplay of Astroneer.

**Currently Implemented**
- Using the marching cubes algorithm to visualise arbitrary scalar field functions.
- Spawning and de-spawning chunks to generate infinitely large marching cubes worlds.
- View frustum culling by calculating the camera draw volume and de-spawning chunks not in camera view.
- Change the geomtry during runtime by changing the underlying scalar field.
- First person mouse controls.

**To Do**
- Implement height map initialisation to start in a more interesting enviroment.
- Implement brush teraforming to manipulate multiple points at once.



# Screenshots

<img src="https://raw.github.com/akoreman/Terraforming-Game-Prototype/main/Images/one.gif" width="400">  


<img src="https://raw.github.com/akoreman/Terraforming-Game-Prototype/main/Images/two.gif" width="400">  
