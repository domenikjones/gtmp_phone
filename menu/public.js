var mainWindow = null;

API.onResourceStart.connect(function (sender, e) {
	/*
	mainWindow = API.createMenu("OEFFENTLICH", 0, 0, 6);

	// declare main item
	var publicMenu = API.createMenuItem("Öffentlich", "");
	resource.phone.mainWindow.AddItem(publicMenu);
	resource.phone.mainWindow.BindMenuToItem(mainWindow, publicMenu);

	// close main menu
	var closeItem = API.createMenuItem("Schliessen", "");
	closeItem.Activated.connect(function(menu, item) {
		resource.phone.mainWindow.Visible = false;
	});

	mainWindow.AddItem(closeItem);
	
	// register sub menu in main menu
	resource.phone.menuPool.Add(mainWindow);
	
	mainWindow.RefreshIndex();
	*/
});

API.onUpdate.connect(function (s, e) {

});