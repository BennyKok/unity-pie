# unity-pie

This pacakge has is an experimental pie menu for Unity

### A
## Press A for the Pie

![Imgur](https://i.imgur.com/J2maHmf.gif)

```
UPM install via git url -> https://github.com/BennyKok/unity-pie.git
```

To try out the feel of the pie menu in unity you can try the probuilder pie integration package

```
UPM install via git url -> https://github.com/BennyKok/unity-pie-probuilder.git
```

## API Examples
Just like Unity's MenuItemAttribute, instead you use PieMenu!!!

```
[PieMenu(path = "Selection/Object")]
public static void ObjectMode()
{
    ProBuilderEditor.selectMode = SelectMode.Object;
}

[PieMenu(path = "New PolyShape")]
public static void PolyShape()
{
    EditorApplication.ExecuteMenuItem("Tools/ProBuilder/Editors/New Poly Shape");
}
```

## Explore
Feel free to check me out!! :)

[Twitter](https://twitter.com/BennyKokMusic) | [Website](https://bennykok.com) | [AssetStore](https://assetstore.unity.com/publishers/28510)
