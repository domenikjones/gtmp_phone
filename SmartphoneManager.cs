using System;
using System.Linq;

using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;


namespace Smartphone
{
    public class SmartphoneManager : Script
    {
    	public SmartphoneManager()
    	{
    		Database.Init();

    		API.onPlayerConnected += onPlayerConnected;
    		API.onClientEventTrigger += OnClientEvent;
    	}

    	public void onPlayerConnected(Client player)
        {
            if(!Database.DoesPlayerContactsExists(player.socialClubName))
            {
            	Database.CreatePlayerContactsList(player.socialClubName);
            }
        }

        //
        // triggered client events
        //

        public void OnClientEvent(Client player, string eventName, params object[] arguments)
        {
            // CONTACT_INFO
            if (eventName == "CONTACT_INFO_REQUEST")
            {
                // trigger client event and send data
                PlayerContactData contactData = Database.GetPlayerContactData(player.socialClubName);
                API.triggerClientEvent(player, "CONTACT_INFO_RESPONSE", contactData.credits);
                return;
            }

        	// contact list request
        	else if (eventName == "CONTACT_LIST_REQUEST")
        	{
                // load contact from local database
                PlayerContactData contactData = Database.GetPlayerContactData(player.socialClubName);
                // send contact list as json to client
                API.triggerClientEvent(player, "CONTACT_LIST_RESPONSE", API.shared.toJson(contactData.contacts));
                return;
        	}
	        
	        // add contact request
	        else if (eventName == "ADD_CONTACT_REQUEST")
	        {
                // adding number handeled separetly, the lgic will be used on edit cantact request
	        	AddNumber(player, arguments);
                return;
	        }

            //
	        // recieve edit contact request
            //
            else if (eventName == "EDIT_CONTACT_REQUEST")
            {
                // load contact from local database
                var contactData = Database.GetPlayerContactData(player.socialClubName);

                // check for existing number
                API.shared.consoleOutput("Existing number: " + arguments[2]);
                foreach (var numberData in contactData.contacts)
                {
                    API.shared.consoleOutput("Number: " + numberData.number + " Name: " + numberData.name);
                }

                var existingNumberEntry = contactData.contacts.Where(c=>c.number == arguments[2].ToString());

                if (!existingNumberEntry.Any())
                {
                    API.shared.consoleOutput("Existing number entry not found: " + arguments[2]);
                    API.triggerClientEvent(player, "EDIT_CONTACT_RESPONE_ERROR", "number_does_not_exist", arguments[2]);
                    return;
                }

                var existingEntry = existingNumberEntry.FirstOrDefault();
                API.shared.consoleOutput("Existing number entry name: " + existingEntry.name);
                API.shared.consoleOutput("Existing number entry number: " + existingEntry.number);

                // delete existing entry
                var resultDelete = Database.RemoveContactFromPlayerContactsList(player.socialClubName, existingEntry.name);
                if (resultDelete[0] != "success")
                {
                    //API.shared.consoleOutput("4444");
                    API.triggerClientEvent(player, "DELETE_CONTACT_RESPONSE_ERROR", resultDelete[0], existingEntry.name);
                    return;
                }

                // add number and send repsonse
                AddNumber(player, arguments);
                return;
            }

	        // recieve delete contact request
	        else if (eventName == "DELETE_CONTACT_REQUEST")
	        {
	        	var name = arguments[0].ToString();
        		var result = Database.RemoveContactFromPlayerContactsList(player.socialClubName, name);
        		if (result[0] != "success")
                {
        			API.triggerClientEvent(player, "DELETE_CONTACT_RESPONSE_ERROR", result[0], name);
                }
				API.triggerClientEvent(player, "DELETE_CONTACT_RESPONSE", result[0], name);
                return;
	        }

            // CONTACT_CALL_REQUEST
            else if (eventName == "CONTACT_CALL_REQUEST")
            {
                // trigger client event and send data
                API.shared.consoleOutput("Player '" + player.socialClubName + " requests number " + arguments[0]);
                // handle call request
                HandleCallRequest(player, arguments[0].ToString());
                return;
            }
        }

        //
        // add number handling
        // 
        public void AddNumber(Client player, params object[] arguments)
        {
            API.shared.consoleOutput("AddContactToPlayerContactsList: " + arguments[0] + " " + arguments[1]);
            var result = Database.AddContactToPlayerContactsList(player.socialClubName, arguments[0].ToString(), arguments[1].ToString());
            // error while trying to save new contact
            if (result[0] == "error_name" || result[0] == "error_number")
            {
                API.shared.consoleOutput("AddContactToPlayerContactsList.ErrorResult: " + result[0] + " " + result[1]);
                API.triggerClientEvent(player, "ADD_CONTACT_RESPONSE_ERROR", result[0], result[1]);
                return;
            }
            API.shared.consoleOutput("AddContactToPlayerContactsList: " + result[0] + " " + result[1] + " " + result[2]);
            API.triggerClientEvent(player, "ADD_CONTACT_RESPONSE", result[1], result[2]);
            return;
        }

        //
        // call handling
        //
        public void HandleCallRequest(Client player, string requestedNumber) {
            API.shared.consoleOutput("HandleCallRequest");
            var playerDb = Database.GetPlayerContactData(player.socialClubName);

            // requested plaer data could not be found
            try {
                var playerRequestDb = Database.GetPlayerContactData(player.socialClubName);
            }
            catch {
                API.triggerClientEvent(player, "CONTACT_CALL_REQUEST_ERROR", "number_not_found", requestedNumber);
                return;
            }

            // requested player online state
            // nothing happens, send something back
            API.triggerClientEvent(player, "CONTACT_CALL_REQUEST_ERROR", "number_not_found", requestedNumber);
            return;
        }
    }
}