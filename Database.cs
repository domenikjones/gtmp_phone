using System.Collections.Generic;
using System.IO;
using System;
using System.Xml.Serialization;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;

namespace RPGResource
{
    public static class Database
    {
    	public const string CONTACTS_FOLDER = "cnc_contacts";

    	public static void Init()
    	{
    		// initialize phone database
    		if (!Directory.Exists(CONTACTS_FOLDER))
                Directory.CreateDirectory(CONTACTS_FOLDER);

            API.shared.consoleOutput("Database initialized!");
            return;
    	}

    	public static bool DoesPlayerContactsExists(string name)
        {
            var path = Path.Combine(CONTACTS_FOLDER, name);
            return File.Exists(path);
        }

        public static void CreatePlayerContactsList(string name)
        {
            var playerContactsData = new PlayerContactsData()
            {
                socialClubName = name,
                contacts = new List<ContactData>(),
            };
            WritePlayerContactsData(name, playerContactsData);
            return;
        }

        public static PlayerContactsData GetPlayerContactsData(string name)
        {
            var playerContactsData = ReadPlayerContactsData(name);
            return playerContactsData;
        }

        public static void AddContactToPlayerContactsList(string name, params string[] args)
        {
            var playerContactsData = GetPlayerContactsData(name);
            var contactData = new ContactData() {
                name = args[0],
                number = args[1],
            };
            playerContactsData.contacts.Add(contactData);
            //playerContactsData.contacts.OrderBy(c=>c.name);
            WritePlayerContactsData(name, playerContactsData);
            return;
        }

        public static void RemoveContactFromPlayerContactsList(string name)
        {
            PlayerContactsData playerContacts = GetPlayerContactsData(name);
            return;
        }

        public static PlayerContactsData ReadPlayerContactsData(string name)
        {
            var path = Path.Combine(CONTACTS_FOLDER, name);
            var txt = File.ReadAllText(path);
            PlayerContactsData playerContacts = API.shared.fromJson(txt).ToObject<PlayerContactsData>();
            API.shared.consoleOutput("Read PlayerContactsData!");
            return playerContacts;
        }

        public static void WritePlayerContactsData(string name, PlayerContactsData data)
        {
            var path = Path.Combine(CONTACTS_FOLDER, name);
            var ser = API.shared.toJson(data);
            File.WriteAllText(path, ser);
            API.shared.consoleOutput("Wrote PlayerContactsData!");
            return;
        }

    }

    public class PlayerContactsData
    {
        public string socialClubName { get; set; }
        public List<ContactData> contacts { get; set; }
    }

    public class ContactData
    {
        public string name { get; set; }
        public string number { get; set; }
    }
}