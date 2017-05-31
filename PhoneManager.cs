using System.Collections.Generic;

using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;

namespace RPGResource
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
	        	Database.AddContactToPlayerContactsList(player.socialClubName, arguments[0].ToString(), arguments[1].ToString());
				API.triggerClientEvent(player, "ADD_CONTACT_RESPONSE", arguments[0].ToString(), arguments[1].ToString());
	        }

	        // recieve edit contact request
	        // recieve delete contact request
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