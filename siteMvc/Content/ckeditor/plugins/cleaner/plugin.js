CKEDITOR.plugins.add('cleaner',
{
	init: function (editor) {
		var pluginName = 'cleaner';

		// Register the dialog.
		CKEDITOR.dialog.add(pluginName, this.path + 'dialogs/cleaner.js');

		// Register the command.
		editor.addCommand(pluginName, new CKEDITOR.dialogCommand(pluginName));

		// Register the toolbar button.
		editor.ui.addButton('Cleaner',
			{
				label: editor.lang.cleaner.buttonLabel,
				command: pluginName,
				icon: CKEDITOR.plugins.getPath('cleaner') + "/images/cleaner.gif"
			});

		editor.on('mode', function () {
			if(editor.readOnly == false)
				editor.getCommand(pluginName).enable();
		});
	}
});