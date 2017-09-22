==========================================================================
  Copyright (C), 2017-2018, Mogoson Tech. Co., Ltd.
  Name: AssetFilter
  Author: Mogoson   Version: 0.1.0   Date: 8/30/2017
==========================================================================
  [Summary]
    Unity plugin for asset name specification filter.
--------------------------------------------------------------------------
  [Demand]
    Define the specification of asset name.
	
    Check the name of assets under the target directory, filter and
    display the assets those name is mismatch the define specification.
--------------------------------------------------------------------------
  [Environment]
    Unity 5.0 or above.
    .Net Framework 3.0 or above.
--------------------------------------------------------------------------
  [Achieve]
    AssetPatternSettings : Config the asset name specification.

    AssetFilterEditor : Draw the extend editor window and select the
    target directory and specification config, Filter/Browse/Focus the
    assets those name is mismatch the define specification.
--------------------------------------------------------------------------
  [Usage]
    Find the menu item "Tool/Asset Filter" in Unity editor menu bar and
    click it or press key combination Alt+F to open the "Asset Filter"
    editor window.

    Click the "Browse" button to select the "Target Directory".

    Click the "New" button to create the "Pattern Settings", select and
	config it in Unity Inspector.

    Click the "Check" button to start check the target assets names.

    The filter assets will be display in the "Mismatch Assets" area, you
    can click a asset to focus it in the Unity Project.
--------------------------------------------------------------------------
  [Pattern]
    Use regular expressions to define the AssetPattern.
--------------------------------------------------------------------------
  [Resource]
    https://github.com/mogoson/AssetFilter.
--------------------------------------------------------------------------
  [Contact]
    If you have any questions, feel free to contact me at mogoson@qq.com.
--------------------------------------------------------------------------