using System;
using System.Linq;

using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;


namespace Smartphone
{
    public class PhoneManager : Script
    {
    	public PhoneManager()
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
        	// contact list request
        	if (eventName == "CONTACT_LIST_REQUEST")
        	{
    			// trigger client event and send data
    			SendContactsList(player);
        	}
	        
	        // add contact request
	        else if (eventName == "ADD_CONTACT_REQUEST")
	        {
	        	var result = Database.AddContactToPlayerContactsList(player.socialClubName, arguments[0].ToString(), arguments[1].ToString());
        		API.shared.consoleOutput("AddContactToPlayerContactsList.Result: " + result[0] + " " + result[1]);

	        	// error while trying to save new contact
        		if (result[0] == "error_name" || result[0] == "error_number")
        		{
        			API.triggerClientEvent(player, "ADD_CONTACT_RESPONSE_ERROR", result[0], result[1]);
        			return;
    			}

    			API.shared.consoleOutput("AddContactToPlayerContactsList: " + result[0] + " " + result[1] + " " + result[2]);
				API.triggerClientEvent(player, "ADD_CONTACT_RESPONSE", result[1], result[2]);
				return;
	        }

	        // recieve edit contact request
	        // recieve delete contact request
	        else if (eventName == "DELETE_CONTACT_REQUEST")
	        {
	        	var name = arguments[0].ToString();

        		var result = Database.RemoveContactFromPlayerContactsList(player.socialClubName, name);
        		if (result[0] != "success")
        			API.triggerClientEvent(player, "DELETE_CONTACT_RESPONSE_ERROR", result[0], name);

				API.triggerClientEvent(player, "DELETE_CONTACT_RESPONSE", result[0], name);
	        }
        }

        //
        // trigger client feedback
        //

        // send contact list
        public void SendContactsList(Client player)
        {
            // load contacts from local database
            PlayerContactsData contactsData = Database.GetPlayerContactsData(player.socialClubName);
            // send contacts data as list to client
            API.triggerClientEvent(player, "CONTACT_LIST_RESPONSE", API.shared.toJson(contactsData.contacts));
        }
        // send added contact confirmation
        // send edit contact confirmation
        // send delete contact confirmation
    }
}