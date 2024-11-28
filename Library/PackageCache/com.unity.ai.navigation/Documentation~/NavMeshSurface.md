# NavMesh Surface component reference

Use the NavMesh Surface component to define and build a [NavMesh](Glossary.md#NavMesh) for a specific type of [NavMesh Agent](NavMeshAgent.md) in your scene. Use the available properties to specify the type of NavMesh Agent that can use the NavMesh, the area type to assign to the generated NavMesh, and the geometry to use when you bake the NavMesh.

To use the NavMesh Surface component, apply it to the [GameObject](Glossary.md#gameobject) on which you want to build the NavMesh.

To apply the NavMesh Surface component to a GameObject, do the following:

1. Select the GameObject.
1. In the Inspector, select **Add Component** &gt; **Navigation** &gt; **NavMesh Surface**. <br/> The Inspector window displays the NavMesh Surface component.

 A [Scene](Glossary.md#scenes) can contain multiple NavMesh surfaces. You can add the NavMesh Surface component to any GameObject in your scene. This is useful for when you want to use the GameObject parenting [Hierarchy](https://docs.unity3d.com/Manual/Hierarchy.html) to define which GameObjects contribute to the NavMesh.

The following table describes the properties available in the NavMesh Surface component. Use the main settings for the NavMesh Surface component to filter the input geometry on a broad scale. Use the [NavMesh Modifier](NavMeshModifier.md) component to adjust how Unity treats input geometry on a per-GameObject basis. 

<table>
  <thead>
    <tr>
      <th colspan="1"><strong>Property</strong></th>
      <th colspan="2"><strong>Description</strong></th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td rowspan="1"><strong>Agent Type</strong></td>
     <td colspan="2">Select the type of <a href="NavMeshAgent.html">NavMesh Agent</a> that can use the NavMesh.</td>
    </tr>
    <tr>
      <td rowspan="4"><strong>Default Area</strong></td>
      <td colspan="2">Specify the area type to assign to the generated NavMesh. The area types define how difficult it is for agents to move across the NavMesh. The available options include all of the area types defined on the Areas tab of the Navigation window. There are 29 custom area types and 3 built-in area types: </td>
    </tr>
    <tr>
      <td><strong>Walkable</strong></td>
      <td>Make the NavMesh walkable for the assigned Agent type. (This is the default option.)</td>
    </tr>
    <tr>
      <td><strong>Not Walkable</strong></td>
      <td>Prevent the specified Agent type from crossing this surface unless there is a GameObject that <a href="NavMeshModifier.html">overrides the area type</a>.</td>
    </tr>
    <tr>
      <td><strong>Jump</strong></td>
      <td>This option is used for automatically generated links. <br/> For more details about area types, refer to <a href="AreasAndCosts.html">Navigation Areas and Costs</a>.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Generate Links</strong></td>
      <td colspan="2">Automatically generate links between objects that the NavMesh Surface collects when you bake the NavMesh. If you select <strong>Generate Links</strong>, NavMesh Surface attempts to generate links between any collected GameObjects when you bake the NavMesh. If you do not select <strong>Generate Links</strong>, NavMesh Surface doesn't attempt to generate any links between the collected GameObjects when you bake the NavMesh.<br/> Refer to the <a href="BuildingOffMeshLinksAutomatically.html#links-generation">Links Generation</a> section for more information.</td>
    </tr>
    <tr>
      <td rowspan="3"><strong>Use Geometry</strong></td>
      <td colspan="2">Select which geometry to use when you bake the NavMesh.</td>
    </tr>
    <tr>
      <td><strong>Render Meshes</strong></td>
      <td>Use geometry from Render Meshes and <a href="https://docs.unity3d.com/Manual/terrain-UsingTerrains.html">Terrains</a>.</td>
    </tr>
    <tr>
      <td><strong>Physics Colliders</strong></td>
      <td>Use geometry from Colliders and Terrains. Agents can move closer to the edge of the physical bounds of the environment with this option than they can with the <strong>Render Meshes</strong> option. For more information on Colliders, refer to <a href="https://docs.unity3d.com/Manual/CollidersOverview.html">Introduction to collision</a>.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>NavMesh Data</strong></td>
      <td colspan="2">(Read-only) Locate the asset file where the NavMesh is stored. </td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Clear</strong></td>
      <td colspan="2">Remove the asset file where the NavMesh is stored.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Bake</strong></td>
      <td colspan="2">Bake a NavMesh with the current settings. When you bake the NavMesh, it automatically excludes GameObjects that have a <strong>NavMesh Agent</strong> or <strong>NavMesh Obstacle</strong>. They are dynamic users of the NavMesh and don't contribute to the process.</td>
    </tr>
  </tbody>
</table>



## Object collection

Use the Object Collection settings to define which GameObjects to use when you bake the NavMesh.

<table>
  <thead>
    <tr>
      <th colspan="1"><strong>Property</strong></th>
      <th colspan="2"><strong>Description</strong></th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td rowspan="5"><strong>Collect Objects</strong></td>
      <td colspan="2">Define which GameObjects to use when you bake the NavMesh.</td>
    </tr>
    <tr>
      <td><strong>All Game Objects</strong></td>
      <td>Use all active GameObjects in the scene. (This is the default option.)</td>
    </tr>
    <tr>
      <td><strong>Volume</strong></td>
      <td>Use all active GameObjects that overlap the bounding volume. Geometry that is located outside of the bounding volume but within the agent radius is included when you bake the NavMesh.</td>
    </tr>
    <tr>
      <td><strong>Current Object Hierarchy</strong></td>
      <td>Use the GameObject that the <strong>NavMesh Surface</strong> component is placed on and all active GameObjects which are children of this GameObject. </td>
    </tr>
    <tr>
      <td><strong>NavMeshModifier Component Only</strong></td>
      <td>Use any GameObjects in the scene that have a NavMesh Modifier attached to them and, if their <strong>Apply To Children</strong> option is turned on, use their child objects as well.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Include Layers</strong></td>
      <td colspan="2">Select the layers for which GameObjects are included in the bake process. In addition to <strong>Collect Objects</strong>, this allows for further exclusion of specific GameObjects from the bake process (for example, effects or animated characters).<br/> This is set to <strong>Everything</strong> by default, but you can toggle options on (denoted by a checkmark) or off, individually.</td>
    </tr>
  </tbody>
</table>

Futhermore, you can use the [NavMesh Modifier](./NavMeshModifier.md) component to designate more precisely the objects, and their hierarchies, that the NavMesh Surface can or cannot collect.

## Advanced Settings

Use the Advanced settings section to customize the following additional properties:

| **Property**            | **Description**      |
|:------------------------|:---------------------|
| **Override Voxel Size** | Control how accurately Unity processes the input geometry when you bake the NavMesh. This is a tradeoff between speed and accuracy. <br/> The default size is one third of the Agent [radius](./NavigationWindow.md#agents-tab), which translates into 3 [voxels](NavInnerWorkings.md#about-voxels) per Agent radius. This voxel size allows the capture of narrow passages, such as doors, and maintains a quick baking time. For big open areas, you can use 1 or 2 voxels per radius to speed up baking. Tight indoor spots are better suited to smaller voxels, for example 4 to 6 voxels per radius. More than 8 voxels per radius doesn't usually provide much additional benefit. <br/> To change the default size, select this checkbox. In the **Voxel Size** field, specify the size of the voxels to use when you bake the NavMesh. |
| **Voxel Size**         | Specify the size, in world units, of the voxels to use when you bake the NavMesh. This property is only available if you select the **Override Voxel Size** option.|
| **Override Tile Size** | Change the default Tile Size of the NavMesh. To make the bake process parallel and memory efficient, the Scene is divided into tiles for baking. The white lines visible on the NavMesh are tile boundaries. <br/> The default tile size is 256 voxels, which provides a good tradeoff between memory use and NavMesh fragmentation. <br/> To change this default tile size, select this checkbox. In the **Tile Size** field, specify the number of voxels you want the tile size to be. <br/> The smaller the tiles, the more fragmented the NavMesh is. This can sometimes cause non-optimal paths. NavMesh carving also operates on tiles. If you have a lot of obstacles in your scene, you can often speed up carving by making the tile size smaller (for example around 64 to 128 voxels). For more information, refer to [Carving](AboutObstacles.md#carving). <br/> If you plan to bake the NavMesh at runtime, use a smaller tile size to keep the maximum memory use low.|
| **Tile Size**          | Specify the desired Tile Size in voxels. This property is only available if you select the **Override Tile Size** option. |
| **Minimum Region Area**| Remove small regions that are disconnected from the larger NavMesh. The process that builds the NavMesh doesn't retain the stretches of the mesh that have a surface size smaller than the specified value. <br/> **Note**: Some areas might not get removed despite the **Minimum Region Area** parameter. The NavMesh is built in parallel as a grid of tiles. If an area straddles a tile boundary, the area isn't removed. The reason for this is that the area pruning step takes place at a stage in the build process when the surrounding tiles aren't accessible. |
| **Build Height Mesh** | Generate additional data that specifies the height of the surface at each point on the NavMesh. Select this option to generate HeightMesh data. Clear this option if you do not want to generate HeightMesh data. For more information, refer to [**About HeightMeshes**](AboutHeightMesh.md).  |



## Additional resources

- [About NavMesh agents](AboutAgents.md)
- [About HeightMeshes](AboutHeightMesh.md)
- [Links Generation](BuildingOffMeshLinksAutomatically.md#links-generation)
- [Carving](AboutObstacles.md#carving)
- [Hierarchy](https://docs.unity3d.com/Manual/Hierarchy.html)
- [Create a NavMesh agent](CreateNavMeshAgent.md)
- [Navigation areas and costs](AreasAndCosts.md)
- [NavMesh Modifier component reference](NavMeshModifier.md)
- Physics [Colliders](https://docs.unity3d.com/Manual/CollidersOverview.html)
- [Terrains](https://docs.unity3d.com/Manual/terrain-UsingTerrains.html)
