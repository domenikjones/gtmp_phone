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
            // CONTACT_CALL_REQUEST
            if (eventName == "CONTACT_CALL_REQUEST")
            {
                // trigger client event and send data
                API.shared.consoleOutput("Player '" + player.socialClubName + " requests number " + arguments[0]);
                // handle call request
                HandleCallRequest(player, "123456");
            }

        	// contact list request
        	if (eventName == "CONTACT_LIST_REQUEST")
        	{
                // load contact from local database
                PlayerContactData contactData = Database.GetPlayerContactData(player.socialClubName);
                // send contact list as json to client
                API.triggerClientEvent(player, "CONTACT_LIST_RESPONSE", API.shared.toJson(contactData.contacts));
        	}
	        
	        // add contact request
	        else if (eventName == "ADD_CONTACT_REQUEST")
	        {
	        	var result = Database.AddContactToPlayerContactsList(player.socialClubName, arguments[0].ToString(), arguments[1].ToString());
        		//API.shared.consoleOutput("AddContactToPlayerContactsList.Result: " + result[0] + " " + result[1]);
	        	// error while trying to save new contact
        		if (result[0] == "error_name" || result[0] == "error_number")
        		{
                    //API.shared.consoleOutput("AddContactToPlayerContactsList.ErrorResult: " + result[0] + " " + result[1]);
        			API.triggerClientEvent(player, "ADD_CONTACT_RESPONSE_ERROR", result[0], result[1]);
        			return;
    			}
    			//API.shared.consoleOutput("AddContactToPlayerContactsList: " + result[0] + " " + result[1] + " " + result[2]);
				API.triggerClientEvent(player, "ADD_CONTACT_RESPONSE", result[1], result[2]);
				return;
	        }

	        // recieve edit contact request
            //

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
	        }
        }

        //
        // call handling
        //
        public void HandleCallRequest(Client player, string requestedNumber) {
            API.shared.consoleOutput("HandleCallRequest");
            var playerDb = Database.GetPlayerContactData(player.socialClubName);

            // requested player coulf not be found
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