Package : Dialog System

This packages allows you to create a dialog graph and then use it in your game.

What this package do :
	-> Dialog graph creation
	-> Graph converter to readable dialog data
	-> Dialog data read by a DialogController into DialogText
	-> Dialog text Displayed (optional) with DialogDisplayer

You will find several prefab and demo scenes to ease the setup process.

-----------------------------------------------------------------------------------

There are 10 small setup steps to see your Dialog comme to life in your scene.

Setup :
	-> Open the graph Editor with the "Graph" menu in the Unity toolbar or by doucle-clicking a DialogueGraphSO
	-> Configure your graph
		-> Each Dialog and choice id you will enter will refers to a specific sentence
		-> You can add several conditions to your choices. The item key you will enter will refers to the key you will enter in the DialogItemGiver component
	-> When it's ready, save it in your project's files
	-> Place a prefab Controller_Dialogue in the scene
	-> Set the DialogueGraphSO to your new DialogGraphSo
		-> Auto init will automatically play the dialog to the first sentence
		-> Unity Events are here to ease the process of adding effects, they do not return any value
	-> Then place a prefab Displayer_Dialogue in your scene
		-> You can also create your own with the DialogueDisplayer component
	-> Set the DialogueController in your DialogueDisplayer

	----------------------------------------------

	-> if YOU ARE NOT using the loaclization system
		-> You'll need to setup your dialog and choices idToDialogue converter
		-> to do that : Unity toolbar : Assets/Create/dialogue/IdToDialogueConverter
		-> Enter the id's you set in your graph and the associated text
		-> Repeat the process for the choices
		-> Set the converter variables in the DialogueDisplayer

	----------------------------------------------

	-> if YOU ARE using the loaclization system
		-> You won't need the Dialogue and Choices converter
		-> Add a LocalizationDynamicDialogue component to the DialogueDisplayer
		-> Make sure you have your LocalizationManager setup ready in your scene
		-> That's all (the converter will auomatically be handled in the Localizationmanager)


	-> Choose your DisplayParameters preferences

	-> You now have your Dialogue Ready to go !

	----------------------------------------------

	-> if you wan't to use Function calls trough graph
		-> you'll need the prefab ManagerPersistant in your scene
		-> then click "assign GUID to All Objects""
			-> now every gameObjects in the scene have a Persistant GUID attached
			-> !!!!! WARNING !!!!! if you click "remove all guids" your references in the graph will break
			-> references only works in the scene you linked you function graph in (or in duplications of the scene if the Persistant GUID remain the same)
		-> if you wan't to add a Persistant GUID only for new objects that don't have one, don't worry, the button "assign GUID to All Objects" doesn't modify existing Persistant GUID

		-> You are now ready to use Function Calls in the graph !

-----------------------------------------------------------------------------------

Customization :
	-> There is a lot of methods which can be called through Unity event in the Controller/Displayer
	-> See an example in the second DemoScene to see how to customize the Dialogue to your needs

-----------------------------------------------------------------------------------

Crédits : Corentin REMOT

Salut franck'o !