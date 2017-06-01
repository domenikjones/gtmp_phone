let addcontactListMenuItem = null;
let contactListMenuItem = null;

let contactListMenu = null;
let contactMenu = null;

// misc
let addName = null;
let addNumber = null;

API.onResourceStart.connect(function (sender, e) {
	// menu decleration
	contactListMenu = API.createMenu("SMARTPHONE", "Telefonbuch", 0, 0, 6);
	contactMenu =  API.createMenu("SMARTPHONE", "FOO BAR", 0, 0, 6);

	// menu item: add contact
	addcontactListMenuItem = API.createMenuItem("Kontakt hinzufügen", "");
	addcontactListMenuItem.Activated.connect(function(menu, item) {
		addContactToList();
	});
	resource.phone.phoneMenu.AddItem(addcontactListMenuItem);
	
	// menu item: contact list
	contactListMenuItem = API.createMenuItem("Telefonbuch", "");
	// trigger server event to get contact list
	contactListMenuItem.Activated.connect(function(menu, item) {
		API.triggerServerEvent("CONTACT_LIST_REQUEST");
	});
	resource.phone.phoneMenu.AddItem(contactListMenuItem);

	// close menu
	contactListMenuCloseItem = API.createMenuItem("Schliessen", "");
	contactListMenuCloseItem.Activated.connect(function(menu, item) {
		resource.phone.phoneMenu.Visible = false;
	});
	resource.phone.phoneMenu.AddItem(contactListMenuCloseItem);

	// event listener on conacts
	contactListMenu.OnItemSelect.connect(function(sender, item, index) {
		if (index == 0)
			return;
		contactListMenu.Visible = false;
		API.sendChatMessage("You selected: ~g~" + item.Text + " ~w~(" +  item.RightLabel + ")");
		createContactMenu(item.Text, item.RightLabel);
	});

	// bind contacts menu to main menu
	resource.phone.phoneMenu.BindMenuToItem(contactListMenu, contactListMenuItem);
	
	// register contacts menu in menuPool
	resource.phone.menuPool.Add(contactListMenu);
	resource.phone.menuPool.Add(contactMenu);
	contactListMenu.RefreshIndex();
});

//
// events triggered by server
//
API.onServerEventTrigger.connect(function(eventName, args) {
	
	switch(eventName) {
		
		case 'CONTACT_LIST_RESPONSE':
			// rebuild the contact list with the recieved json in text format
			createContactListMenu(args[0]);
			break;

		case 'ADD_CONTACT_RESPONSE_ERROR':
			// name error, mostly because a contact with this name already exists
			if (args[0] == 'error_name')
			{
				API.sendNotification("Kontakt '" + args[1] + "' existiert bereits.");
				return;
			}

			// number error, mostly because a contact with this number already exists
			if (args[0] == 'error_number')
			{
				API.sendNotification('Ein Kontakt mit der Nummer ' + args[1] + " existiert bereits.");
				return;
			}

			API.sendNotification('Unbekannter Status erhalten ' + args[0]);
			break;

		case 'ADD_CONTACT_RESPONSE':
			// contact has been created, rebuild contact list
			createContactListMenu();
			API.sendNotification('Kontakt hinzugefügt: ' + args[0] + " - " + args[1]);
			break;

		case 'DELETE_CONTACT_RESPONSE_ERROR':
			// an error occoured while deleting the contact from the database
			API.sendNotification('Kontakt konnte nicht gelöscht werden: ' + args[1]);
			break;

		case 'DELETE_CONTACT_RESPONSE':
			// contact has succesfully been deleted
			API.sendNotification('Kontakt gelöscht: ' + args[1]);
			break;
	}
});

// toggle menus onKeyDown of keys F3 and ESC, we assume they always have to be closed,
// since they are submenu and are not triggered by F3 and ESC
API.onKeyDown.connect(function(sender, keyEventArgs) {
	if (keyEventArgs.KeyCode == Keys.ESC || keyEventArgs.KeyCode == Keys.F3) {
		contactListMenu.Visible = false;
		contactMenu.Visible = false;
	}
});

//
// helper functions
//
function createContactMenu(name, number) {
	// rebuild contact menu
	contactMenu.Clear();

	// contact number information
	contactMenuNumberItem = API.createMenuItem("Nummer: ~g~" + number, "");
	contactMenu.AddItem(contactMenuNumberItem);

	// call number menu item
	contactMenuCallItem = API.createMenuItem("Anrufen", "");
	contactMenuCallItem.Activated.connect(function(menu, item) {
		API.sendNotification('Anrufen');
		contactMenu.Visible = false;
	});
	contactMenu.AddItem(contactMenuCallItem);

	// delete contact menu item
	contactMenuDeleteItem = API.createMenuItem("~r~Löschen", "");
	contactMenuDeleteItem.Activated.connect(function(menu, item) {
		API.sendNotification('Löschen');
		deleteContact(name);
	});
	contactMenu.AddItem(contactMenuDeleteItem);
	
	// close menu
	contactMenuCloseItem = API.createMenuItem("Schliessen", "");
	contactMenuCloseItem.Activated.connect(function(menu, item) {
		contactMenu.Visible = false;
	});
	contactMenu.AddItem(contactMenuCloseItem);

	contactMenu.Visible = true;

	// refresh contactMenu index
	contactMenu.RefreshIndex();
}

function createContactListMenu(contacts_list) {
	// rebuild contacts list
	contactListMenu.Clear();

	// close menu item
	contactListCloseMenuItem = API.createMenuItem("Schliessen", "");
	contactListCloseMenuItem.Activated.connect(function(menu, item) {
		contactListMenu.Visible = false;
	});
	contactListMenu.AddItem(contactListCloseMenuItem);

	// empty contacts list
	if (contacts_list == null) {
		API.sendNotification('Contacts list empty');
		return;
	}

	var list = JSON.parse(contacts_list);

	// add menu item to contactListMenu foreach contact from server
	list.forEach(function(element, index, array) {
		// a single contact
		var contactMenuItem = API.createMenuItem(element.name, "");
		contactMenuItem.SetRightLabel(element.number);
		contactListMenu.AddItem(contactMenuItem);
	});

	// refresh contacts menu index
	contactListMenu.RefreshIndex();
}

function addContactToList() {
	// prompt for name
	API.sendNotification('Name des Kontaktes eingeben.');
	addName = API.getUserInput("", 15);

	// prompt for name
	API.sendNotification('Nummer des Kontaktes eingeben.');
	addNumber = API.getUserInput("", 10);

	// verify number prompt is a number
	if (isNaN(addNumber)) {
		API.sendNotification('~r~Keine gültige Nummer.');
		return;
	}

	// send contact data to onServerEventTrigger
	API.triggerServerEvent("ADD_CONTACT_REQUEST", addName, addNumber);
	return;
}

function deleteContact(name)
{
	// confirm the contact should be deleted by promting for "ja" or "Ja"
	API.sendNotification('Ja um den Kontakt zu löschen.');
	var confirm = API.getUserInput("", 4);
	if (confirm == "ja" || confirm == "Ja")
	{
		// close menu and send request to server
		contactMenu.Visible = false;
		API.triggerServerEvent("DELETE_CONTACT_REQUEST", name);
	}
}