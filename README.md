# ChartToJson
Flow chart exportable to Json using WPF and MEF. It works but it's not finished (also it lacks certain dlls of my own tools that I will upload some other time).

I made this because I needed a tree like structure of objects, I like to plan things on flow charts and didn't want to plan it all and then write all the code down. So I made a flow chart that export not only to an image but to a Json file with a tree like structure.

Only entities will be exported to Json, not any visual including text entities and texts boxes. 
Entities are added through MEF plugins following the "StoryChartEntitiesTemplatesPlugin" (the model subfolder are the entities itself) folder as example.
Using MEF UserControls can also be added so the user can input the data of the entities. Following MVVM pattern, the real entites wich will be exported to Json will be the model of that UserControls.

Just for visual appealing, more border shapes of the entities can be added through MEF plugins too, following "ShapesTemplatesPlugin" folder as example.

The idea is just throw the plugin dll file into a subfolder called "plugins" (case insensitive), and run.
