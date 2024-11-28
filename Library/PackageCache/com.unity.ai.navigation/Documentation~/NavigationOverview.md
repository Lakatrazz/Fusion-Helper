# Navigation Overview

This section provides details on how to build NavMeshes for your [**scene**][1] or prefabs, and create [**NavMesh**][2] agents, NavMesh obstacles and NavMesh links.

| **Topic**  | **Description**                |
|:-----------|:-------------------------------|
| [Create a NavMesh](CreateNavMesh.md)| Define the area(s) of your scene where a character can navigate intelligently. |
| [Create a NavMesh agent](CreateNavMeshAgent.md)| Create a character to navigate your scene. |
| [Create a NavMesh obstacle](CreateNavMeshObstacle.md) | Create obstacles for the agents to avoid as they navigate your scene. |
| [Create a NavMesh link](CreateNavMeshLink.md) | Create navigation shortcuts that cannot be represented by a walkable surface. |
| [Using NavMesh Agent with other components](MixingComponents.md) | Best practices when using navigation components along with other Unity components.|
| [Advanced navigation how-tos](NavHowTos.md)| Advanced techniques to implement common tasks in navigation. |


[1]: https://docs.unity3d.com/Manual/CreatingScenes.html "A Scene contains the environments and menus of your game. Think of each unique Scene file as a unique level. In each Scene, you place your environments, obstacles, and decorations, essentially designing and building your game in pieces."
[2]: ./Glossary.md#navmesh "A mesh that Unity generates to approximate the walkable areas and obstacles in your environment for path finding and AI-controlled navigation."
