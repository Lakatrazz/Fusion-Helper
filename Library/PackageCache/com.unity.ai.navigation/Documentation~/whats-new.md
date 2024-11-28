# What's new in AI Navigation version 2.0.0

The main updates in this release include:

## Added

* New option to specify the end points of a NavMeshLink through Transform references.
* New `NavMeshLink.activated` property that gets or sets whether agents can use the link.

## Updated

* The `NavMeshLink.costModifier` property is now a float.
* The `OffMeshLink` component has been deprecated. You can no longer add it to GameObjects from the **Add Component** menu. Instead, you can now use a `NavMeshLink` component just as you would have used an `OffMeshLink` component in previous versions.  

For a full list of changes and updates in this version, see the AI Navigation package changelog.
