# OffMesh Link component reference

> [!Important]
> The OffMesh Link component is deprecated and no longer supported. Use the [NavMesh Link component](./NavMeshLink.md) instead.

Use OffMesh Link components to incorporate navigation shortcuts, which can't be represented using a walkable surface, into your scene. For example, with OffMesh links, an agent can jump over a ditch or a fence, or open a door then walk through it. 

OffMesh Links only apply to the Humanoid agent type, so if the NavMeshes in your scene use a different agent type, the OffMesh Link won't create a link between them.

The following table describes the properties available in the OffMesh Link component:

<table>
  <thead>
    <tr>
      <th colspan="1"><strong>Property</strong></th>
      <th colspan="2"><strong>Description</strong></th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td rowspan="1"><strong>Start</strong></td>
      <td colspan="2">Select the GameObject that represents the start location of the link.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>End</strong></td>
      <td colspan="2">Select the GameObject that represents the end location of the link.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Cost Override</strong></td>
      <td colspan="2">Override the cost to move across the link. <br/>If the Cost Override value is negative, the cost of the Navigation Area type is used. If the Cost Override value is non-negative, the cost of moving over the link is equal to the Cost Override value multiplied by the length of the link. The length of the link is the distance between the start and end points of the link.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Bidirectional</strong></td>
      <td colspan="2">Control the direction NavMesh Agents move across the link. When you select this checkbox, NavMesh Agents can move across the link in both directions (from the start to the end, and from the end to the start).<br/>When you clear this checkbox, NavMesh Agents can only move across the link in one direction (from the start to the end).</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Activated</strong></td>
      <td colspan="2">Allow the link to be used in pathfinding.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Auto Update Positions</strong></td>
      <td colspan="2">Reconnect the OffMesh link to the NavMesh if you move the end points. If disabled, the link stays at its start location even if the end points move.</td>
    </tr>
    <tr>
      <td rowspan="4"><strong>Navigation Area</strong></td>
      <td colspan="2">Specify the area type of the OffMesh Link. Use the area type to apply a common traversal cost to similar area types and prevent certain characters from crossing the link based on the agent’s Area Mask.</td>
    </tr>
    <tr>
      <td><strong>Walkable</strong></td>
      <td>Make the link walkable for the affected agent types. This is the default option.</td>
    </tr>
    <tr>
      <td><strong>Not Walkable</strong></td>
      <td>Prevent the affected agent types from crossing the link.</td>
    </tr>
    <tr>
      <td><strong>Jump</strong></td>
      <td>Change the area type of the link to Jump.</td>
    </tr>
  </tbody>
</table>


## Additional resources

- [NavMesh Link component reference](./NavMeshLink.md)
- [Create OffMesh Links](./CreateOffMeshLink.md)
- [OffMesh Link scripting reference](https://docs.unity3d.com/ScriptReference/AI.OffMeshLink.html) 
