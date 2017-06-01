var menuPool = API.getMenuPool();
var phoneMenu = API.createMenu("SMARTPHONE", "Telefon", 0, 0, 6);

menuPool.Add(phoneMenu);

API.onKeyDown.connect(function(sender, keyEventArgs) {
	if (keyEventArgs.KeyCode == Keys.F3) {
		phoneMenu.Visible = !phoneMenu.Visible;
	}
	if (keyEventArgs.KeyCode == Keys.ESC) {
		phoneMenu.Visible = false;
	}
});

API.onUpdate.connect(function(sender, events) {
	menuPool.ProcessMenus();
});