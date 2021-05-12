using System;
using System.Collections.Generic;

namespace UnityEngine.Localization.SmartFormat.Tests
{
    [Serializable]
    public class Person
    {
        [SerializeField]
        string m_FirstName = string.Empty;

        [SerializeField]
        string m_LastName = string.Empty;

        [SerializeField]
        string m_MiddleName = string.Empty;

        [SerializeField]
        long m_BirthdayTicks;

        [SerializeField]
        Address m_Address;

        [SerializeReference]
        List<Person> m_Friends;

        [SerializeReference]
        Person m_Spouse;

        [SerializeField]
        Gender m_Gender;

        public Person()
        {
            Friends = new List<Person>();
        }

        public Person(string newName, Gender gender, DateTime newBirthday, string newAddress, params Person[] newFriends)
        {
            FullName = newName;
            Gender = gender;
            Birthday = newBirthday;
            if (!string.IsNullOrEmpty(newAddress))
                Address = Address.Parse(newAddress);
            Friends = new List<Person>(newFriends);
        }

        public string FirstName { get => m_FirstName; set => m_FirstName = value; }

        public string LastName { get => m_LastName; set => m_LastName = value; }

        public string MiddleName { get => m_MiddleName; set => m_MiddleName = value; }

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(this.MiddleName))
                {
                    return this.FirstName + " " + this.LastName;
                }
                else
                {
                    return this.FirstName + " " + this.MiddleName + " " + this.LastName;
                }
            }
            set
            {
                string[] names = value.Split(' ');
                this.FirstName = names[0];
                if (names.Length == 2)
                {
                    this.LastName = names[1];
                }
                else if (names.Length == 3)
                {
                    this.MiddleName = names[1];
                    this.LastName = names[2];
                }
                else
                {
                    this.MiddleName = names[1];
                    this.LastName = string.Join(" ", names, 2, names.Length - 2);
                }
            }
        }

        public string Name => FirstName + " " + LastName;

        public DateTime Birthday
        {
            get
            {
                return new DateTime(m_BirthdayTicks);
            }
            set
            {
                m_BirthdayTicks = value.Ticks;
            }
        }

        public int Age
        {
            get
            {
                if (Birthday.Month < DateTime.Now.Month || (Birthday.Month == DateTime.Now.Month && Birthday.Day <= DateTime.Now.Day))
                {
                    return DateTime.Now.Year - Birthday.Year;
                }
                else
                {
                    return DateTime.Now.Year - 1 - Birthday.Year;
                }
            }
        }

        public Address Address { get => m_Address; set => m_Address = value; }

        public List<Person> Friends { get => m_Friends; set => m_Friends = value; }

        public int NumberOfFriends => Friends.Count;

        public override string ToString()
        {
            return LastName + ", " + FirstName;
        }

        public Person Spouse { get => m_Spouse; set => m_Spouse = value; }

        public Gender Gender { get => m_Gender; set => m_Gender = value; }

        public int Random()
        {
            return DateTime.Now.Second % 3;
        }
    }

    public enum Gender
    {
        Male = 0,
        Female = 1,
    }
}
