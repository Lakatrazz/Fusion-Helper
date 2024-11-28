# NavMesh Link component reference

Use the NavMesh Link component to connect different NavMeshes built for the same [agent type](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavigationWindow.html#agents-tab). The link can be a line from one point to another (no width), or a span (with width). If the link is a span (with width), the Agent uses the nearest location along the entry edge to cross the link. For example, you can use the **NavMesh Link** component to connect a NavMesh that represents a building’s interior to a NavMesh that represents the building’s exterior. You can't overlap separate NavMeshes to create a link between them. 

To use the NavMesh Link you can either add it to your scene as a GameObject or add it to an existing GameObject as a component.

To add a NavMesh Link to your scene as a GameObject, do the following:
- From the main menu go to **GameObject** > **AI** > **NavMesh Link**.<br/> The **NavMesh Link** component is displayed in the **Inspector** window.

To add the NavMesh Link component to an existing GameObject, do the following: 
1. Select the GameObject you want to add the component to.
1. In the Inspector select **Add Component**, then select **Navigation** &gt; **NavMesh Link**. <br/> The **NavMesh Link** component is displayed in the **Inspector** window.


The following table describes the properties available in the NavMesh Link component.
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
      <td colspan="2">Specify which Agent type can use the link.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Start Transform</strong></td>
      <td colspan="2">Select the GameObject that represents the start location of the link. This object is tracked by the middle of the link's start edge.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Start Point</strong></td>
      <td colspan="2">Specify the start point of the link, relative to the GameObject's world-space position and orientation. The three values define the point's X, Y, and Z coordinates. Neither transform scale nor shear affect this point. </br>The link uses this start position only when <strong>Start Transform</strong> does not reference any object.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>End Transform</strong></td>
      <td colspan="2">Select the GameObject that represents the end location of the link. This GameObject is tracked by the middle of the link's end edge.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>End Point</strong></td>
      <td colspan="2">Specify the end point of the link, relative to the GameObject's world-space position and orientation. The three values define the point's X, Y, and Z coordinates. Neither transform scale nor shear affect this point. </br>The link uses this end position only when <strong>End Transform</strong> does not reference any object.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Swap</strong></td>
      <td colspan="2">Swap the start and end points and swap the start and end transforms.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Re-Center Origin</strong></td>
      <td colspan="2">Move the GameObject to the center point of the link and align the transform’s forward axis with the end point. </td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Width</strong></td>
      <td colspan="2">Specify the width of the link. You can also drag the handles at the side of the link to adjust the width. <br/><strong>Note</strong>: The GameObject's scale does not affect the width of the link.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Cost Override</strong></td>
      <td colspan="2">Choose how to assign the cost to move across the link. <br/> Select <strong>Cost Override</strong> to set the cost value directly in the adjacent number field.<br/> Deselect <strong>Cost Override</strong> for the cost of the Area type to become the cost of moving over the NavMesh link. In this case the adjacent number field is disabled and not used. <br/> Path finding uses the cost in conjunction with the distance between the start and end positions in world space. Refer to [Areas and costs](./AreasAndCosts.md#pathfinding-cost) for more information.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Auto Update Positions</strong></td>
      <td colspan="2">Update the positions of the link's ends automatically when any of the GameObject transform, the start transform or the end transform change position.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Bidirectional</strong></td>
      <td colspan="2">Control the direction NavMesh Agents move across the link. When you select this checkbox, NavMesh Agents can move across the link in both directions (from the start point to the end point, and from the end point to the start point).<br/>When you clear this checkbox, NavMesh Agents can only move across the link in one direction (from the start point to the end point).</td>
    </tr>
    <tr>
      <td rowspan="5"><strong>Area Type</strong></td>
      <td colspan="2">The area type of the NavMesh Link. The area type allows you to apply a common traversal cost to similar area types and prevent certain characters from accessing the NavMesh Link based on the agent’s Area Mask. For more information about area types and traversal costs, refer to [Areas and Costs](./AreasAndCosts.md)</td>
    </tr>
    <tr>
      <td><strong>Walkable</strong></td>
      <td>Make the link walkable for the affected agent types. This is the default option.</td>
    </tr>
    <tr>
      <td><strong>Not Walkable</strong></td>
      <td>Prevent the affected agent types from crossing the link. Links with a <strong>Not Walkable</strong> area type do not connect to any NavMesh.</td>
    </tr>
    <tr>
      <td><strong>Jump</strong></td>
      <td>Change the area type of the link to <strong>Jump</strong>. This is the type that is assigned to all auto-generated NavMesh links.</td>
    </tr>
    <tr>
      <td><strong>Open Area Settings </strong></td>
      <td>Open the Areas tab of the [Navigation window](NavigationWindow.md) to define new area types or modify existing ones.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Activated</strong></td>
      <td colspan="2">Allow agents and queries to use the link in pathfinding. <br/>Select <strong>Activated</strong> to display the link's gizmo in the Scene view with black lines. <br/>Deselect <strong>Activated</strong> to display the link's gizmo in the Scene view with red lines.</td>
    </tr>
  </tbody>
</table>

To adjust the ends of the link directly from the scene view you can drag the yellow handler gizmos at each end. A yellow cube represents the **Point** position for that end. In the opposite arrangement where the link references an object, a yellow sphere represents the **Transform** position of the referenced object. If you move the yellow sphere, the referenced object moves along to the same position.  
To adjust the width of the link you can drag the orange dot handler gizmos placed on the sides of the link, at one third of the distance from the start to the end.  
To display the handles, enable the NavMesh Link gizmo and select the GameObject. For more information on gizmos, refer to [Gizmos menu](https://docs.unity3d.com/Manual/GizmosMenu.html).

## Additional resources

- [About Agents](./NavigationWindow.md#agents-tab)
- [Areas and costs](./AreasAndCosts.md)
- [OffMesh Link component (deprecated) reference](./OffMeshLink.md)
