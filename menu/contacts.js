let contactsMenuItem = null;
let contactsMenu = null;

let addName = null;
let addNumber = null;

let contacts_list_dummy = [
	{name: "foo", number: "1111"},
	{name: "bar", number: "2222"}
]

API.onResourceStart.connect(function (sender, e) {	
	contactsMenu = API.createMenu("SMARTPHONE", "Kontakte", 0, 0, 6);

	// menu item: add contact
	addContactMenuItem = API.createMenuItem("Kontakt hinzufügen", "");
	addContactMenuItem.Activated.connect(function(menu, item) {
		addContactToList();
	});
	resource.phone.phoneMenu.AddItem(addContactMenuItem);
	
	// menu item: contact list
	contactsMenuItem = API.createMenuItem("Kontakte", "");
	// trigger server event to get contact list
	contactsMenuItem.Activated.connect(function(menu, item) {
		API.triggerServerEvent("CONTACT_LIST_REQUEST");
	});
	resource.phone.phoneMenu.AddItem(contactsMenuItem);

	// event listener on conacts
	contactsMenu.OnItemSelect.connect(function(sender, item, index) {
		API.sendChatMessage("You selected: ~g~" + item.Text + " ~w~(" +  item.RightLabel + ")");
		contactsMenu.Visible = false;
	});

	// bind contacts menu to main menu
	resource.phone.phoneMenu.BindMenuToItem(contactsMenu, contactsMenuItem);
	
	// register contacts menu in menuPool
	resource.phone.menuPool.Add(contactsMenu);
	contactsMenu.RefreshIndex();
});

//
// events triggered by server
//
API.onServerEventTrigger.connect(function(eventName, args) {
	switch(eventName) {
		
		case 'CONTACT_LIST_RESPONSE':
			createContactsList(args[0]);
			break;

		case 'ADD_CONTACT_RESPONSE':
			createContactsList();
			API.sendNotification('Kontakt hinzugefügt: ' + args[0] + " - " + args[1]);
			break;
	}
});


function createContactsList(contacts_list) {
	contactsMenu.Clear();

	// empty contacts list
	if (contacts_list == null) {
		API.sendNotification('Contacts list empty');
		return;
	}

	var list = JSON.parse(contacts_list);

	// add menu item to contactsMenu foreach contact from server
	list.forEach(function(element, index, array) {
		var item = API.createMenuItem(element.name, "");
		item.SetRightLabel(element.number);
		contactsMenu.AddItem(item);
	});

	// refresh contacts menu index
	contactsMenu.RefreshIndex();
}

function addContactToList() {
	addName = API.getUserInput("Name", 15);
	addNumber = API.getUserInput("Number", 10);

	if (isNaN(addNumber)) {
		API.sendNotification('~g~Keine gültige Nummer.');
		return;
	}

	API.triggerServerEvent("ADD_CONTACT_REQUEST", addName, addNumber);
	return;
}