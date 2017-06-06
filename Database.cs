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

        /*
         Players data
         */
        public static bool DoesPlayerContactsExists(string name)
        {
            var path = Path.Combine(CONTACTS_FOLDER, name);
            return File.Exists(path);
        }

        public static void CreatePlayerContactsList(string name)
        {
            // initialize player contact data
            var PlayerContactData = new PlayerContactData()
            {
                socialClubName = name,
                contacts = new List<ContactData>(),
            };
            WritePlayerContactData(name, PlayerContactData);
            return;
        }

        public static PlayerContactData GetPlayerContactData(string name)
        {
            PlayerContactData PlayerContactData = ReadPlayerContactData(name);
            return PlayerContactData;
        }

        public static string[] AddContactToPlayerContactsList(string name, params string[] args)
        {
            var addName = args[0];
            var addNumber = args[1];

            var PlayerContactData = GetPlayerContactData(name);

            // check for duplicates entry (name)
            var nameDuplicate = PlayerContactData.contacts.Where(c => c.name == addName);
            if (nameDuplicate.Any())
            {
                string[] resultErrorName = new string[] {"error_name", addName};
                return resultErrorName;
            }

            // check for duplicates entry (number)
            var numberDuplicate = PlayerContactData.contacts.Where(c => c.number == addNumber);
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
            PlayerContactData.contacts.Add(contactData);

            // reorder contact data
            PlayerContactData.contacts.OrderBy(c=>c.name);

            // save contact data to database
            WritePlayerContactData(name, PlayerContactData);

            string[] resultSucces = new string[] {"success", addName, addNumber};
            return resultSucces;
        }

        public static string[] RemoveContactFromPlayerContactsList(string name, string removeName)
        {
            PlayerContactData PlayerContactData = GetPlayerContactData(name);

            // validate the name of the contact exists
            var contacts = PlayerContactData.contacts.Where(c=>c.name==removeName);
            if (!contacts.Any()) {
                string[] resultError = new string[] {"error", removeName};
                return resultError;
            }

            // remove contact from players contacts list and save
            PlayerContactData.contacts.Remove(contacts.FirstOrDefault());
            WritePlayerContactData(name, PlayerContactData);

            string[] result = new string[] {"success", removeName}; 
            return result;
        }

        public static PlayerContactData ReadPlayerContactData(string name)
        {
            // read from simple file in path and load as json
            var path = Path.Combine(CONTACTS_FOLDER, name);
            var txt = File.ReadAllText(path);
            PlayerContactData playerContacts = API.shared.fromJson(txt).ToObject<PlayerContactData>();
            return playerContacts;
        }

        public static void WritePlayerContactData(string name, PlayerContactData data)
        {
            // write json to simple text file
            var path = Path.Combine(CONTACTS_FOLDER, name);
            var ser = API.shared.toJson(data);
            File.WriteAllText(path, ser);
            return;
        }

        /*
         Server PlayerList 
         */

        public static PlayerContactData GetPlayerContactDataByNumber(string number)
        {
            PlayerList playerList = ReadPlayerList();
            var target = playerList.players.Where(p => p.number == number);
            if (target.Any())
            {
                return target.FirstOrDefault();
            }
            return null;
        }

        public static bool DoesPlayersExists()
        {
            // does the player file exist
            var path = Path.Combine(CONTACTS_FOLDER, "players");
            return File.Exists(path);
        }

        public static bool DoesPlayerExist(Client player, string name)
        {
            PlayerList playerList = ReadPlayerList();
            if (!playerList.players.Where(p => p.socialClubName == player.socialClubName).Any())
            {
                // create new phone number
                bool numberAvailable = false;
                int number = 0;
                Random r = new Random();

                while (!numberAvailable)
                {
                    number = r.Next(12000, 19999);
                    number += 42000000; // add number prefix
                    if (!playerList.players.Where(p => p.number == number.ToString()).Any())
                    {
                        numberAvailable = true;
                    }
                }

                PlayerContactData newPlayer = new PlayerContactData {
                    name = name,
                    socialClubName = player.socialClubName,
                    number = number.ToString(),
                    credits = 0,
                    contacts = new List<ContactData>(),
                };

                playerList.players.Add(newPlayer);
                WritePlayers(playerList);

                return false;
            }
            else
            {
                return true;
            }
        }

        public static void CreatePlayers()
        {
            // initialize player contact data
            PlayerList players = new PlayerList() {
                players = new List<PlayerContactData>(),
            };
            WritePlayers(players);
            return;
        }

        public static PlayerList ReadPlayerList()
        {
            // read from simple file in path and load as json
            var path = Path.Combine(CONTACTS_FOLDER, "players");
            var txt = File.ReadAllText(path);
            PlayerList playerList = API.shared.fromJson(txt).ToObject<PlayerList>();
            return playerList;
        }

        public static void WritePlayers(PlayerList data)
        {
            // write json to simple text file
            var path = Path.Combine(CONTACTS_FOLDER, "players");
            var ser = API.shared.toJson(data);
            File.WriteAllText(path, ser);
            return;
        }
    }

    public class PlayerContactData
    {
        public string name { get; set; }
        public string socialClubName { get; set; }
        public string number { get; set; }
        public int credits { get; set; }
        public List<ContactData> contacts { get; set; }
        //public DateTime lastAction { get; set; }
    }

    public class ContactData
    {
        public string name { get; set; }
        public string number { get; set; }
    }

    public class PlayerList
    {
        public List<PlayerContactData> players { get; set; }
    }
}