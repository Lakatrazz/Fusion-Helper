# Upgrade projects for use with AI Navigation package

Navigation and Pathfinding in Unity is handled by the AI Navigation package as of Unity 2022.2.

If you have projects that were created with the Navigation feature in previous versions of Unity, the AI Navigation package is automatically installed and added to your project. You can then do one of the following:

- Continue to use your projects as they are
- Convert your projects to use the new package

## Remove old component scripts

If your project uses the NavMesh Surface, NavMesh Modifier, NavMesh Modifier Volume or NavMesh Link components defined by scripts downloaded from [Unity’s NavMeshComponents GitHub repository](https://github.com/Unity-Technologies/NavMeshComponents), then remove those scripts and any associated files before you add the AI Navigation package to your project. If you don’t remove these scripts, you might get conflicts and errors related to these components in the Console. The new components mirror the same behavior as the old components do in your project except when using the following components:

- The NavMesh Surface component now includes an option to use only the objects that have a NavMesh Modifier in the baking process.
- You can now specify whether or not to apply the NavMesh Modifier component to child objects in the hierarchy.

## Convert your project

If you want to use the new package you need to convert your project(s). As part of the conversion process, the NavMesh Updater makes the following changes:

- Any NavMesh that was previously baked and embedded in the scene is now referenced from a NavMeshSurface component created on a new GameObject
 called Navigation.
- Any object that was marked with Navigation Static now has a NavMeshModifier component with the appropriate settings.

To convert your project do the following:

1. In the main menu go to **Window** > **AI** > **NavMesh Updater**.
2. In the **NavMesh Updater** window, select which kind of data to convert.
3. Click **Initialize Converters** to detect and display the types of data you selected.
4. Select the data you want to convert.
5. Click **Convert Assets** to complete the conversion. 

## Create new agent types 

If the NavMeshes in different scenes are baked with different agent settings then you need to create new agent types to match those settings. 

To create the agent types do the following:

1. In the main menu go to **Window** > **AI** > **Navigation**.
2. Select **Agents**.
3. Create new entries and specify the relevant settings.

### Assign new agent types
When you have created the new agent types you then need to assign them as follows: 

- Assign the newly created agent types to their respective NavMeshSurfaces in the Navigation created for that scene.
- Assign the agent types to the NavMeshAgents intended to use that NavMesh.

To find the settings that were used for each existing NavMesh, select the NavMesh `.asset` file in the **Project** window. The NavMesh settings will be displayed in the **Inspector**.

## Create NavMesh Links instead of Off-Mesh Links

The **OffMesh Link** component was originally designed to work with only the **Humanoid** agent type. Now it has been deprecated. Your project can still use this component but you can no longer add it from the editor. You are encouraged to use the **NavMesh Link** component instead. It has the same properties as **OffMesh Link**, and a few additional ones: agent type, width, and two positions that can define the ends.

To replace an **OffMesh Link** component with a **NavMesh Link** component do the following:

1. Select the GameObject that has the **OffMesh Link** component. The GameObject can be in a scene or a prefab.
2. Add a **NavMesh Link** component.
3. Assign to the **NavMesh Link** the same properties as the **OffMesh Link**.
4. Remove the **OffMesh Link** from the GameObject.
5. Save the scene or prefab as needed.

### Replace OffMeshLink with NavMeshLink in scripts

In your scripts you can replace any occurence of the `OffMeshLink` class with the `NavMeshLink` class. The scripts will continue to work as before, as long as the `NavMeshLink` component exists on the affected GameObjects. The `OffMeshLink` properties `autoUpdatePositions`, `biDirectional`, `costOverride` and the method `UpdatePositions()` have equivalents in the `NavMeshLink` component. You can substitute those class members in places where you use them in scripts, or you can accept the suggestion from the _Script Updating Consent_ tool to do the same thing. This tool runs when the editor reloads the scripts in the project.
