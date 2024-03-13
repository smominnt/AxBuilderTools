# AxBuilderTools
 Autocross and other cone based driving course map creator for Assetto Corsa.

## Downloading the Application
To Download click the link on the repository [homepage](https://github.com/smominnt/AxBuilderTools) marked `Releases`.

You will be navigated to a new page. Scroll to the latest release and click on `Assets` so that the files are visible. 

Click on `release.zip`, the file will automatically download.

![image](https://github.com/smominnt/AxBuilderTools/assets/47583239/c601ef7a-1a8c-44d9-9f6a-3b5da84ddc7f)


Extract the release.zip file to the directory of your choosing. 

Navigate into the `net8.0-windows` folder.

Open `AxBuilder` to access the UI. 

![image](https://github.com/smominnt/AxBuilderTools/assets/47583239/7719f034-7679-49c7-97f6-2a93b898ee3d)


## Help

### Getting Started

#### UI Navigation

![help_ui1](https://github.com/smominnt/AxBuilderTools/assets/47583239/c290e7af-5bc2-41fc-b62f-4b1d6d26bef2)

#### Tools

![image](https://github.com/smominnt/AxBuilderTools/assets/47583239/0ee94af7-7de1-42db-ba68-18f992199cfa)

#### Tool Use and Element Manipulation

![image](https://github.com/smominnt/AxBuilderTools/assets/47583239/fc4b96fa-b021-4e30-b113-6c7b35eadbc8)

![image](https://github.com/smominnt/AxBuilderTools/assets/47583239/ef05c6a6-b192-4209-86a7-00c51509a853)

### Building Track

To Build the track, you must `Save` the current map file, then click the `Build Track` button and choose the directory you want to save your track to. 
When building, this Window will appear:

![image](https://github.com/smominnt/AxBuilderTools/assets/47583239/62955d26-60b6-43d8-bcdc-e21656650eec)

After the track is finished building, you can copy the track directory to your Assetto Corsa directory:

`C:\Program Files (x86)\Steam\steamapps\common\assettocorsa\content\tracks`

Then you will be able to load the game normally or by Content Manager and select your created track.

### Questions and Tips 

Why is it taking so long after I clicked the `Build Track` button?
- The time it takes to build depends on how many cones you decide to add as well as the processing power of your computer. In most cases, it should complete well within one minute.

Why is the course so small in Assetto Corsa?
- Make sure you have correctly measured distance and have used the correct unit of measure.

Can I keep track of penalties when hitting a cone?
- Unfortunately this is not supported. 

Can I use this to build Autotest/Autoslalom or Gymkhana courses instead of American Autocross? 
- You can be creative and possibly use this to build such tracks as well.

The course I drive on at real events isn't an empty open lot, how do I recreate it in AX Builder?
- Unfortunately this tool is only able to build with an empty lot. If you drive on a track and the size is within 1 mile by 1 mile (1.6 km by 1.6 km), you can create barriers of cones to indicate where the edge of a track is like so:

![image](https://github.com/smominnt/AxBuilderTools/assets/47583239/21ff8ecf-544d-44ce-a139-346f52f841da)


## Development

To develop this tool in your own environment, the application requires the following external packages:

- Autodesk FBX SDK: [https://aps.autodesk.com/developer/overview/fbx-sdk](https://aps.autodesk.com/developer/overview/fbx-sdk)
- RapidJSON: [https://github.com/Tencent/rapidjson/](https://github.com/Tencent/rapidjson/)
