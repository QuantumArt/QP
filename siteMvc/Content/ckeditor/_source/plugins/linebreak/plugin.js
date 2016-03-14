/*
Copyright (c) 2003-2010, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

/**
 * @file Break plugin.
 */

(function()
{
	var linebreakCmd =
	{
		canUndo : false,    // The undo snapshot will be handled by 'insertElement'.
		exec : function( editor )
		{
			editor.insertElement( editor.document.createElement( 'br' ) );
		}
	};

	var pluginName = 'linebreak';

	// Register a plugin named "linebreak".
	CKEDITOR.plugins.add( pluginName,
	{
		init : function( editor )
		{
			editor.addCommand(pluginName, linebreakCmd);
			editor.ui.addButton( 'LineBreak',
				{
					label : editor.lang.linebreak,
					command : pluginName,
					icon: CKEDITOR.plugins.getPath('linebreak') + "/images/line_break.gif"
				});
		}
	});
})();
