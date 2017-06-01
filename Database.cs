using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;


namespace Smartphone
{
    public static class Database
    {
    	public const string CONTACTS_FOLDER = "cnc_contacts";

    	public static void Init()
    	{
    		// initialize phone database
    		if (!Directory.Exists(CONTACTS_FOLDER))
                Directory.CreateDirectory(CONTACTS_FOLDER);

            API.shared.consoleOutput("Smartphone contacts database initialized!");
            return;
    	}

    	public static bool DoesPlayerContactsExists(string name)
        {
            var path = Path.Combine(CONTACTS_FOLDER, name);
            return File.Exists(path);
        }

        public static void CreatePlayerContactsList(string name)
        {
            // initialize player contact data
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

        public static string[] AddContactToPlayerContactsList(string name, params string[] args)
        {
            var addName = args[0];
            var addNumber = args[1];

            var playerContactsData = GetPlayerContactsData(name);

            // check for duplicates entry (name)
            var nameDuplicate = playerContactsData.contacts.Where(c => c.name == addName);
            if (nameDuplicate.Any())
            {
                string[] resultErrorName = new string[] {"error_name", addName};
                return resultErrorName;
            }

            // check for duplicates entry (number)
            var numberDuplicate = playerContactsData.contacts.Where(c => c.number == addNumber);
            if (numberDuplicate.Any())
            {
                string[] resultErrorNumber = new string[] {"error_number", addNumber};
                return resultErrorNumber;
            }

            // valid data, create new contact object
            var contactData = new ContactData() {
                name = addName,
                number = addNumber,
            };
            playerContactsData.contacts.Add(contactData);

            // reorder contact data
            playerContactsData.contacts.OrderBy(c=>c.name);

            // save contact data to database
            WritePlayerContactsData(name, playerContactsData);

            string[] resultSucces = new string[] {"success", addName, addNumber};
            return resultSucces;
        }

        public static string[] RemoveContactFromPlayerContactsList(string name, string removeName)
        {
            PlayerContactsData playerContactsData = GetPlayerContactsData(name);

            // validate the name of the contact exists
            var contacts = playerContactsData.contacts.Where(c=>c.name==removeName);
            if (!contacts.Any()) {
                string[] resultError = new string[] {"error", removeName};
                return resultError;
            }

            // remove contact from players contacts list and save
            playerContactsData.contacts.Remove(contacts.FirstOrDefault());
            WritePlayerContactsData(name, playerContactsData);

            string[] result = new string[] {"success", removeName}; 
            return result;
        }

        public static PlayerContactsData ReadPlayerContactsData(string name)
        {
            // read from simple file in path and load as json
            var path = Path.Combine(CONTACTS_FOLDER, name);
            var txt = File.ReadAllText(path);
            PlayerContactsData playerContacts = API.shared.fromJson(txt).ToObject<PlayerContactsData>();
            return playerContacts;
        }

        public static void WritePlayerContactsData(string name, PlayerContactsData data)
        {
            // write json to simple text file
            var path = Path.Combine(CONTACTS_FOLDER, name);
            var ser = API.shared.toJson(data);
            File.WriteAllText(path, ser);
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