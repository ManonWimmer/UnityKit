Package : localization System

-----------------------------------------------------------------------------

Setup :

	--------------------------------------

generate a localizationDataSO :
	-> Open the localizationDataSO Editor with the "Tool" menu in the Unity toolbar then "CSV to scriptable object"
	-> insert your CSV to convert
	-> create a SO localizationDataSO in Assets/Create/localization/localizationData
	-> Insert your localizationDataSO to fill
	-> Click convert to SO

	--------------------------------------

Always :
	-> put a Localizationmanager in your scene
	-> Set it a localization data

	--------------------------------------

To localize constant text :
	-> Add localizationInfoBox to the object you want to translate
	-> set the target TMP
	-> set the associated id mathcing your localizationDataSO
	-> you are ready to go !

	--------------------------------------

To localize Dialogue (With DialogSystem Package) :
	-> Just toggle useDynamicLocalizationDisplay bool in the DialogDisplayer prefab
	-> You are ready to go ! (ID's need to match dialogue graph ID's)

-----------------------------------------------------------------------------

Crédits : Corentin REMOT

Salut francko !