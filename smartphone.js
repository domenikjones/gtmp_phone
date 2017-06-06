// prepare menu
var menuPool = API.getMenuPool();
var phoneMenu = API.createMenu("SMARTPHONE", "", 0, 0, 6);

// prepare menu pool
menuPool.Add(phoneMenu);

// smartphone class instance
let smartphone = null;

// menu
let contactListMenu = null;
let contactMenu = null;

// menu items
let creditsMenuItem = null;
let addcontactListMenuItem = null;
let contactListMenuItem = null;

// various globals
let addName = null;
let addNumber = null;

//
// Resource start event
//
API.onResourceStart.connect(function (sender, e) {
	// initialize phone
	smartphone = new Smartphone();

	// menu decleration
	contactListMenu = API.createMenu("SMARTPHONE", "Telefonbuch", 0, 0, 6);
	contactMenu =  API.createMenu("SMARTPHONE", "", 0, 0, 6);

	// menu item: close menu
	contactListMenuCloseItem = API.createMenuItem("Schliessen", "");
	contactListMenuCloseItem.Activated.connect(function(menu, item) {
		phoneMenu.Visible = false;
	});
	phoneMenu.AddItem(contactListMenuCloseItem);
	
	// menu item: contactList
	contactListMenuItem = API.createMenuItem("Telefonbuch", "");
	contactListMenuItem.Activated.connect(function(menu, item) {
		// trigger server event to get contact list
		smartphone.setContactList();
	});
	phoneMenu.AddItem(contactListMenuItem);

	// menu item: addContact
	addContactMenuItem = API.createMenuItem("Kontakt hinzufügen", "");
	addContactMenuItem.Activated.connect(function(menu, item) {
		smartphone.addContactToClient();
	});
	phoneMenu.AddItem(addContactMenuItem);

	// menu item: credits
	creditsMenuItem = API.createMenuItem("Guthaben", "");
	phoneMenu.AddItem(creditsMenuItem);

	// listener: open conact menu
	contactListMenu.OnItemSelect.connect(function(sender, item, index) {
		if (index == 0)
			return;
		contactListMenu.Visible = false;
		//API.sendChatMessage("You selected: ~g~" + item.Text + " ~w~(" +  item.RightLabel + ")");
		smartphone.createClientContactMenu(item.Text, item.RightLabel);
	});

	// bind contactListMenuItem to contactListMenu
	phoneMenu.BindMenuToItem(contactListMenu, contactListMenuItem);
	
	// register menus in menuPool and refresh index
	menuPool.Add(contactListMenu);
	menuPool.Add(contactMenu);
	contactListMenu.RefreshIndex();
});

//
// Keys event
//
API.onKeyDown.connect(function(sender, keyEventArgs) {
	if (keyEventArgs.KeyCode == Keys.F3) {
		if (!phoneMenu.Visible) {
			smartphone.setSmartPhone();
		}

		phoneMenu.Visible = !phoneMenu.Visible;
	}
	
	if (keyEventArgs.KeyCode == Keys.ESC || keyEventArgs.KeyCode == Keys.F3) {
		contactListMenu.Visible = false;
		contactMenu.Visible = false;
	}
});

//
// Update event
//
API.onUpdate.connect(function(sender, events) {
	menuPool.ProcessMenus();
});

//
// Events triggered
//
API.onServerEventTrigger.connect(function(eventName, args) {
	
	switch(eventName) {

		case 'CONTACT_INFO_RESPONSE':
			creditsMenuItem.SetRightLabel(args[0].toString());
			break;
		
		case 'CONTACT_LIST_RESPONSE':
			// rebuild the contact list with the recieved json in text format
			smartphone.setServerContactList(args[0]);
			break;

		case 'ADD_CONTACT_RESPONSE_ERROR':
			// name error, mostly because a contact with this name already exists
			if (args[0] == 'error_name') {
				API.sendNotification("Kontakt '" + args[1] + "' existiert bereits.");
				return;
			}

			// number error, mostly because a contact with this number already exists
			if (args[0] == 'error_number') {
				API.sendNotification('Ein Kontakt mit der Nummer ' + args[1] + " existiert bereits.");
				return;
			}

			API.sendNotification('Unbekannter Status erhalten ' + args[0] + ". " + args[1]);
			break;

		case 'ADD_CONTACT_RESPONSE':
			// contact has been created, rebuild contact list
			// will also be triggered after editting a contact
			smartphone.createClientContactListMenu();
			API.sendNotification('Kontakt hinzugefügt: ' + args[0] + " - " + args[1]);
			break;

		case 'DELETE_CONTACT_RESPONSE':
			// contact has succesfully been deleted
			API.sendNotification('Kontakt gelöscht: ' + args[1]);
			break;

		case 'DELETE_CONTACT_RESPONSE_ERROR':
			// an error occoured while deleting the contact from the database
			API.sendNotification('Kontakt konnte nicht gelöscht werden: ' + args[1]);
			break;

		case 'CONTACT_CALL_RESPONSE':
			API.sendNotification('CONTACT_CALL_RESPONSE: ' + args[1]);
			break;

		case 'CONTACT_CALL_REQUEST_ERROR':
			// request number to call has not been found
			if (args[0] == 'number_not_found') {
				API.sendNotification('Nummer unbekannt: ' + args[1]);
				return;
			}
			// request number unavailable
			else if (args[0] == 'number_unavailable') {
				API.sendNotification('Nummer nicht verfügbar: ' + args[1]);
				return;
			}
			// not implenented yes
			else if (args[0] == 'not_implemented') {
				API.sendNotification('Noch nicht implementiert.');
				return;
			}
			
	}
});

//
// Smartphone
//
class Smartphone {

	// initialize smartphone
	constructor() {
		API.sendChatMessage("Smartphone gestartet");
		this.credits = "0";	
		this.isCalling = false;
	}

	setSmartPhone() {
		// set credits menuItem rightLabel
		API.triggerServerEvent("CONTACT_INFO_REQUEST");
	}

	requestSmartPhone() {		
	}

	setContactList() {
		// no initial data set
		if (this.contacts == null) {
			this.getServerContactList();
		}	
	}

	getServerContactList() {
		//API.sendChatMessage("Smartphone:getServerContactList");
		API.triggerServerEvent("CONTACT_LIST_REQUEST");
	}

	setServerContactList(contact_list) {
		//API.sendChatMessage("Smartphone:setServerContactList");
		this.createClientContactListMenu(contact_list);
	}

	createClientContactListMenu(contact_list) {
		// rebuild contacts list
		contactListMenu.Clear();

		// close menu item
		let contactListCloseMenuItem = API.createMenuItem("Schliessen", "");
		contactListCloseMenuItem.Activated.connect(function(menu, item) {
			contactListMenu.Visible = false;
		});
		contactListMenu.AddItem(contactListCloseMenuItem);

		// empty contacts list
		if (contact_list == null) {
			return;
		}		

		// add menu item to contactListMenu foreach contact from server
		let list = JSON.parse(contact_list);
		list.forEach(function(element, index, array) {
			// a single contact
			let contactMenuItem = API.createMenuItem(element.name, "");
			contactMenuItem.SetRightLabel(element.number);
			contactListMenu.AddItem(contactMenuItem);
		});

		// refresh contacts menu index
		contactListMenu.RefreshIndex();
	}

	addContactToClient(skip_save=false) {
		// prompt for name
		//API.sendNotification('Name des Kontaktes eingeben.');
		addName = API.getUserInput("", 20);

		// verify input is a number
		if (addName == "") {
			API.sendNotification('~r~Keine gültiger Name.');
			return null;
		}

		// prompt for number
		//API.sendNotification('Nummer des Kontaktes eingeben.');
		addNumber = API.getUserInput("", 8);

		// verify input is a number
		if (isNaN(addNumber) || addNumber == "") {
			API.sendNotification('~r~Keine gültige Nummer.');
			return null;
		}

		// skip triggering event, because response event is handled somewhere else
		if (skip_save) {
			let result = new Array(addName, addNumber);
			return result;
		}

		// send contact data to onServerEventTrigger
		API.triggerServerEvent("ADD_CONTACT_REQUEST", addName, addNumber);
		return;
	}

	deleteContactFromClient(name) {
		// confirm the contact should be deleted by promting for "ja" or "Ja"
		API.sendNotification('Ja um den Kontakt zu löschen.');
		let confirm = API.getUserInput("", 4);
		if (confirm == "ja" || confirm == "Ja")
		{
			// close menu and send delete contact request to server
			contactMenu.Visible = false;
			API.triggerServerEvent("DELETE_CONTACT_REQUEST", name);
		}

		API.sendNotification("Ungültige Eingabe um einen Kontakt zu löschen");
		return;
	}

	editContact(number) {
		// get user input for name and number via, skip the response event from addContactToClient
		let newContact = this.addContactToClient(true);

		//  contact information could not be entered
		if (newContact == null) {
			API.sendNotification("Kontakt konnte nicht bearbeitet werden. Error Code Smarphone 1001.");
			return;
		}

		API.triggerServerEvent("EDIT_CONTACT_REQUEST", newContact[0], newContact[1], number);
		contactMenu.Visible = false;
	}

	createClientContactMenu(name, number) {
		// rebuild contact menu
		contactMenu.Clear();

		// contact menu item: close menu
		let contactMenuCloseItem = API.createMenuItem("Schliessen", "");
		contactMenuCloseItem.Activated.connect(function(menu, item) {
			contactMenu.Visible = false;
		});
		contactMenu.AddItem(contactMenuCloseItem);

		// contact menu item: number
		let contactMenuNumberItem = API.createMenuItem(name , "");
		contactMenuNumberItem.SetRightLabel(number);
		contactMenu.AddItem(contactMenuNumberItem);

		// contact menu item: call
		let contactMenuCallItem = API.createMenuItem("~g~Anrufen", "");
		contactMenuCallItem.Activated.connect(function(menu, item) {
			smartphone.callContact(name, number);
			contactMenu.Visible = false;
		});
		contactMenu.AddItem(contactMenuCallItem);

		// contact menu item: edit
		let contactMenuEditItem = API.createMenuItem("~o~Bearbeiten", "");
		contactMenuEditItem.Activated.connect(function(menu, item) {
			smartphone.editContact(number);
		});
		contactMenu.AddItem(contactMenuEditItem);

		// contact menu item: delete contact
		let contactMenuDeleteItem = API.createMenuItem("~r~Löschen", "");
		contactMenuDeleteItem.Activated.connect(function(menu, item) {
			API.sendNotification('Löschen');
			smartphone.deleteContactFromClient(name);
		});
		contactMenu.AddItem(contactMenuDeleteItem);

		// show and refresh contactMenu index
		contactMenu.Visible = true;
		contactMenu.RefreshIndex();
	}

	callContact(name, number) {
		API.sendNotification("Rufe ~g~" + name + "~w~ an");
		API.triggerServerEvent("CONTACT_CALL_REQUEST", number);
	}
}